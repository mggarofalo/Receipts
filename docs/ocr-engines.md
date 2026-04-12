# OCR Engine Comparison

The Receipts app supports two OCR backends, selectable via `Ocr:Engine` in `appsettings.json`.

## Supported Engines

| Engine | Config Value | Default | NuGet Package |
|--------|-------------|---------|---------------|
| Tesseract | `Tesseract` | Yes | `Tesseract` (5.2.0) |
| PaddleOCR | `PaddleOCR` | No | `Sdcb.PaddleOCR` + `Sdcb.PaddleOCR.Models.LocalV4` |

## Configuration

```json
{
  "Ocr": {
    "Engine": "Tesseract",
    "TimeoutSeconds": 30,
    "MaxImageBytes": 10485760
  }
}
```

Set `"Engine": "PaddleOCR"` to switch. The change takes effect on application restart.

## Accuracy Comparison

Based on testing with receipt images of varying quality:

| Scenario | Tesseract | PaddleOCR PP-OCRv4 |
|----------|-----------|---------------------|
| Clean printed receipt (thermal) | Good | Good |
| Faded/low-contrast thermal | Fair - misses faded text | Good - better contrast handling |
| Skewed/rotated receipt | Poor - requires preprocessing | Good - built-in angle correction |
| Crumpled/wrinkled receipt | Fair | Good - more robust to distortion |
| Multi-column receipt (Costco) | Fair | Good - better layout detection |
| Handwritten annotations | Poor | Poor |
| Overall accuracy (structured text) | ~85-90% | ~92-96% |

PaddleOCR's advantage comes from its deep-learning detection + recognition pipeline (DBNet + SVTR) which handles layout analysis and character recognition more robustly than Tesseract's traditional approach.

## Resource Usage (Intel N150 Hardware)

| Metric | Tesseract | PaddleOCR PP-OCRv4 Mobile |
|--------|-----------|---------------------------|
| RAM at idle | ~30 MB | ~200 MB |
| RAM during inference | ~50 MB | ~350 MB |
| First inference (cold start) | ~0.5 s | ~2-3 s (model loading) |
| Subsequent inference | ~0.3-0.5 s | ~1-2 s |
| CPU usage during inference | 1 core, ~100% | 1-2 cores, ~150% |
| Disk (models) | ~15 MB (eng.traineddata) | ~12 MB (det + rec + cls models) |

### Recommendations for N150

- **Tesseract** is the safe default for the N150's 8 GB RAM / 4-core configuration. It leaves more headroom for PostgreSQL, the API server, and the React frontend.
- **PaddleOCR** is viable on the N150 but consumes roughly 4x the RAM. If receipt scanning accuracy is a priority and the system is not under heavy concurrent load, PaddleOCR is the better choice.
- For production deployments handling frequent concurrent scans, consider Tesseract to minimize resource contention.

## Docker Runtime Requirements

The Tesseract NuGet package (`Tesseract` 5.2.0) ships managed bindings only; it expects native shared libraries to be present at runtime in a platform-specific subdirectory relative to the application root (e.g. `x64/libleptonica-1.82.0.so`).

### Native packages

The Dockerfile runtime stage installs these Ubuntu Noble (24.04) packages:

| Package | Provides |
|---------|----------|
| `libtesseract5` | `libtesseract.so.5` shared library |
| `liblept5` | `liblept.so.5` shared library (version 1.82.0) |

### Symlink mechanism

Ubuntu installs libraries under `/usr/lib/<arch-triple>/` (e.g. `/usr/lib/x86_64-linux-gnu/liblept.so.5`), but the NuGet package probes `<ARCH_DIR>/libleptonica-1.82.0.so` relative to the app directory. The Dockerfile creates architecture-aware symlinks at build time:

```
x64/libleptonica-1.82.0.so  -> /usr/lib/x86_64-linux-gnu/liblept.so.5
x64/libtesseract50.so        -> /usr/lib/x86_64-linux-gnu/libtesseract.so.5
```

On arm64 the target triple is `aarch64-linux-gnu` and the directory is `arm64/`.

### Tessdata

The English trained data file (`eng.traineddata`) is tracked in the repository at `src/Infrastructure/Models/Tessdata/eng.traineddata`. It flows into the Docker image via the `COPY --from=api-build /app/publish .` step (the .NET publish output includes it as a content file). No manual download or volume mount is required.

## Architecture

Both engines implement the `IOcrEngine` interface:

```csharp
public interface IOcrEngine
{
    Task<OcrResult> ExtractTextAsync(byte[] imageBytes, CancellationToken ct);
}
```

The engine is registered as a singleton in DI based on the `Ocr:Engine` configuration value. Both engines share the same OCR text correction pipeline (`OcrCorrectionHelper`) which fixes common character confusions (S to $, O to 0, l/I to 1) and normalizes whitespace.

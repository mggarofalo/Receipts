# VlmEval

Dev-only sidecar that exercises the Ollama + GLM-OCR receipt-extraction pipeline
against a local set of real receipt fixtures and prints a scorecard.

It exists to answer one question on demand: **is the current VLM model + prompt
still accurate enough to avoid substantial manual correction?**

Checked in per RECEIPTS-621 (epic RECEIPTS-616).

## How to use it

1. Put real receipt files in `fixtures/vlm-eval/` at the repo root (see
   [Fixtures](#fixtures) for the naming and sidecar format). This directory is
   **gitignored** — nothing you drop in it is ever committed.
2. Start Aspire: `dotnet run --project src/Receipts.AppHost`.
3. In the Aspire dashboard, click **Start** on the `vlm-eval` resource.
4. Read the log panel for per-fixture pass/fail and the final summary.

The resource is registered with `.WithExplicitStart()`, so it stays parked
until you trigger it. Hit Start again to re-run after changing fixtures or
swapping models.

You can also run it outside Aspire:

```bash
# requires a reachable Ollama with glm-ocr:q8_0 (or whatever model is configured)
dotnet run --project src/Tools/VlmEval
```

Exit code `0` means every fixture passed its declared assertions; `1` means
at least one failed or Ollama was unreachable. Set
`VlmEval__FailOnAnyFixtureFailure=false` to always exit `0`.

## Fixtures

Each fixture is a pair of files in `fixtures/vlm-eval/`:

| File | Purpose |
|------|---------|
| `<name>.<jpg\|jpeg\|png\|pdf>` | The receipt itself — a real photo or PDF. |
| `<name>.<ext>.expected.json` | Known-truth values to assert against. |

Files without a sidecar are skipped with a warning. Sidecars without a
matching receipt file are ignored. Every sidecar field is optional — only
declared fields are asserted.

### Example sidecar

`2026-01-14-walmart-photo.jpg.expected.json`:

```json
{
  "store": "Walmart",
  "date": "2026-01-14",
  "subtotal": 69.68,
  "total": 70.43,
  "taxLines": [{ "amount": 0.75 }],
  "paymentMethod": "MASTERCARD",
  "minItemCount": 9,
  "notes": "RECEIPTS-611 regression — PaddleOCR misread SUBTOTAL 69.68 as SAL6968"
}
```

Pair `2026-01-14-walmart-photo.jpg` (photo) and `2026-01-14-walmart-photo.pdf`
(PDF export) in the same directory to cover both ingest paths with one set of
expected values.

### Diff rules

| Field | Rule |
|-------|------|
| `store` | case-insensitive substring match |
| `date` | exact match |
| `subtotal`, `total` | within $0.01 of expected |
| `taxLines[].amount` | each expected amount must match some actual amount within $0.01; actual may contain more lines |
| `taxLines[].label` | case-insensitive substring when declared |
| `paymentMethod` | case-insensitive substring (`"MASTERCARD"` matches `"MasterCard ****1234"`) |
| `minItemCount` | `actual items count >= expected` |
| `items[].description` | case-insensitive substring, matched against the line with the closest `totalPrice` if declared, else by index |
| `items[].totalPrice` | within $0.01 |

Undeclared fields are ignored (reported as `NotDeclared`), never failed.

### Naming convention

`<YYYY-MM-DD>-<merchant>-<medium>.<ext>` keeps fixtures sortable and obvious.
Examples:

- `2026-01-14-walmart-photo.jpg`
- `2026-01-14-walmart-photo.pdf`
- `2026-02-20-shell-gas.jpg`
- `2026-03-05-restaurant-tip.jpg`
- `2026-03-10-paperless-textlayer.pdf`
- `2026-03-10-multipage.pdf`

### Privacy / hygiene

Fixtures never leave your machine — they're gitignored and never uploaded by
the tool. Still, keep them sensible:

- Strip card numbers, signatures, loyalty numbers, and names wherever
  possible.
- Prefer compact files (≤ 1 MB per fixture) to keep eval runs fast.

### Walmart regression

The Walmart photo from RECEIPTS-611 is the named regression fixture: it's the
receipt that PaddleOCR couldn't read (reading `SUBTOTAL 69.68` as `SAL6968`)
and that motivated the switch to a VLM.

Drop the same receipt as both `.jpg` and `.pdf` alongside sidecars asserting:

- `subtotal` = `69.68`
- `total` = `70.43`
- `taxLines[0].amount` = `0.75`
- `paymentMethod` contains `MASTERCARD`
- `minItemCount` = `9`

If the suite passes on those values, RECEIPTS-611 is closed end-to-end.

## Configuration

| Setting | Default | Source |
|---------|---------|--------|
| `VlmEval:FixturesPath` | `fixtures/vlm-eval` | `appsettings.json`, env (`VlmEval__FixturesPath`), Aspire injects an absolute path |
| `VlmEval:OllamaTimeoutSeconds` | `180` | `appsettings.json`, env (`VlmEval__OllamaTimeoutSeconds`) |
| `VlmEval:FailOnAnyFixtureFailure` | `true` | `appsettings.json`, env (`VlmEval__FailOnAnyFixtureFailure`) |
| `Ocr:Vlm:OllamaUrl` | *(unset)* | env (`Ocr__Vlm__OllamaUrl`), takes precedence over `Ollama:BaseUrl` |
| `Ollama:BaseUrl` | *(Aspire-injected)* | env; falls back to `http://localhost:11434` |
| `Ocr:Vlm:Model` | `glm-ocr:q8_0` | env (`Ocr__Vlm__Model`) |

When VlmEval runs inside Aspire, `Ollama__BaseUrl` is injected to point at the
`vlm-ocr` container endpoint; `VlmEval__FixturesPath` is set to the absolute
path of `<repo-root>/fixtures/vlm-eval`.

## CI

Not wired into CI — this is a local dev tool that depends on a running Ollama
with a multi-GB model. Run it on demand to validate a model change or to
spot-check a new prompt.

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

# write a structured artifact alongside the console log
dotnet run --project src/Tools/VlmEval -- --output json --report-path eval-report.json
```

### Exit codes

| Code | Meaning |
|------|---------|
| `0` | All declared fixtures passed (or `VlmEval__FailOnAnyFixtureFailure=false` and no hard error). |
| `1` | At least one fixture failed, Ollama was unreachable, the fixtures directory was missing, or no valid fixtures were found while `FailOnAnyFixtureFailure=true`. |
| `130` | Cancelled mid-run (Ctrl+C / SIGINT). The structured report still flushes with the partial results. |

`FailOnAnyFixtureFailure=true` (the default) is strict: missing/empty fixtures
exit `1` so a typo'd `VlmEval__FixturesPath` cannot pass green. Set
`VlmEval__FailOnAnyFixtureFailure=false` to relax this and exit `0` whenever
the run completes without an infrastructure error.

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
| `subtotal`, `total` | inclusive within `MoneyTolerance` of expected (default $0.01: a delta of exactly $0.01 passes) |
| `taxLines[].amount` | each expected amount must match some actual amount within `MoneyTolerance`; actual may contain more lines |
| `taxLines[].label` | case-insensitive substring when declared |
| `paymentMethod` | case-insensitive substring (`"MASTERCARD"` matches `"MasterCard ****1234"`) |
| `minItemCount` | `actual items count >= expected` |
| `items[].description` | case-insensitive substring; matched against the line with the closest `totalPrice` if declared, else first-substring-hit fallback |
| `items[].totalPrice` | inclusive within `MoneyTolerance` |

Undeclared fields are ignored (reported as `NotDeclared`), never failed.

#### Tolerance overrides

`MoneyTolerance` defaults to `$0.01` and can be overridden per run via
`VlmEval__MoneyTolerance` or per fixture by adding `"moneyTolerance": 0.05` to
the sidecar JSON. Per-fixture overrides take precedence and apply to
`subtotal`, `total`, `taxLines[].amount`, and `items[].totalPrice` for that
single fixture only.

#### Matching algorithm

The tax-line and item matchers are **greedy and input-order-stable**:

- Expected entries are processed in declaration order.
- For each expected entry, the matcher picks the still-unmatched actual entry
  with the smallest delta within `MoneyTolerance`. On ties, the earlier index
  wins.
- The matched actual entry is consumed and cannot be reused.
- For `items[]`, if no price-based match exists (or no price was declared),
  the matcher falls back to the **first** unmatched actual line whose
  description contains the expected description (case-insensitive substring).
  This is deterministic but not optimal — when multiple actual lines share a
  description, the earlier one wins.

This is not a globally optimal assignment (no Hungarian algorithm) — it is
deliberately predictable so failures point at a specific actual line rather
than shuffling on every run.

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
| `VlmEval:MoneyTolerance` | `0.01` | `appsettings.json`, env (`VlmEval__MoneyTolerance`); per-fixture override via sidecar `"moneyTolerance"` |
| `VlmEval:OutputFormat` | `Console` | `appsettings.json`, env (`VlmEval__OutputFormat`), CLI `--output console\|json\|markdown` |
| `VlmEval:ReportPath` | *(unset)* | `appsettings.json`, env (`VlmEval__ReportPath`), CLI `--report-path <path>` |
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

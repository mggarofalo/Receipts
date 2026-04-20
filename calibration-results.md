# OnnxEmbeddingService Threshold Calibration Results

Model: `bge-large-en-v1.5` (1024 dim, L2-normalized, CLS-pooled)
Generated: 2026-04-20 01:39:26 UTC

## Per-Cluster Within-Cluster Similarity

| Cluster | N pairs | Min | Max | Mean | Median |
| --- | ---: | ---: | ---: | ---: | ---: |
| Grapes | 15 | 0.7345 | 0.9226 | 0.8138 | 0.8063 |
| Canned Tomatoes | 10 | 0.7543 | 0.9130 | 0.8486 | 0.8410 |
| Bread | 10 | 0.7857 | 0.9397 | 0.8286 | 0.8197 |
| Milk | 10 | 0.6244 | 0.8298 | 0.7285 | 0.7230 |
| Apples | 10 | 0.6968 | 0.8402 | 0.7672 | 0.7651 |

## Overall Within-Cluster Similarity

- Pairs: **55**
- Min: **0.6244**
- Max: **0.9397**
- Mean: **0.7988**
- Median: **0.8006**

## Overall Cross-Cluster Similarity

- Pairs: **270**
- Min: **0.4623**
- Max: **0.7842**
- Mean: **0.5671**
- Median: **0.5619**

## Top 10 Highest Within-Cluster Similarities

| Cluster | Text A | Text B | Similarity |
| --- | --- | --- | ---: |
| Bread | Whole Wheat Bread | Wheat Bread | 0.9397 |
| Grapes | Red Seedless Grapes 2LB | Grapes Red 1LB | 0.9226 |
| Canned Tomatoes | Diced Tomatoes 14.5oz | Diced Tomatoes 28oz | 0.9130 |
| Canned Tomatoes | Diced Tomatoes 28oz | Whole Peeled Tomatoes 28oz | 0.9126 |
| Canned Tomatoes | Diced Tomatoes 28oz | Crushed Tomatoes 28oz | 0.9063 |
| Grapes | Green Grapes | Green Seedless Grapes | 0.8985 |
| Canned Tomatoes | Crushed Tomatoes 28oz | Whole Peeled Tomatoes 28oz | 0.8823 |
| Grapes | Red Grapes | Organic Red Grapes | 0.8651 |
| Grapes | Red Grapes | Grapes Red 1LB | 0.8650 |
| Bread | Sourdough Bread | Wheat Bread | 0.8441 |

## Top 10 Highest Cross-Cluster Similarities

(These should be LOW. If any are high, thresholds need careful tuning.)

| Cluster A | Cluster B | Text A | Text B | Similarity |
| --- | --- | --- | --- | ---: |
| Grapes | Apples | Grapes Red 1LB | Red Delicious Apples 3lb | 0.7842 |
| Grapes | Apples | Red Seedless Grapes 2LB | Red Delicious Apples 3lb | 0.7679 |
| Grapes | Canned Tomatoes | Grapes Red 1LB | Crushed Tomatoes 28oz | 0.7238 |
| Grapes | Milk | Organic Red Grapes | Organic Whole Milk | 0.7104 |
| Canned Tomatoes | Apples | Crushed Tomatoes 28oz | Red Delicious Apples 3lb | 0.7048 |
| Grapes | Canned Tomatoes | Grapes Red 1LB | Whole Peeled Tomatoes 28oz | 0.7042 |
| Grapes | Canned Tomatoes | Red Seedless Grapes 2LB | Crushed Tomatoes 28oz | 0.7024 |
| Grapes | Canned Tomatoes | Grapes Red 1LB | Diced Tomatoes 28oz | 0.7023 |
| Grapes | Canned Tomatoes | Grapes Red 1LB | Diced Tomatoes 14.5oz | 0.6947 |
| Grapes | Canned Tomatoes | Red Seedless Grapes 2LB | Whole Peeled Tomatoes 28oz | 0.6913 |

## Band Overlap Diagnostic

- Within-cluster MIN: **0.6244**
- Cross-cluster MAX: **0.7842**
- Overlap: **0.1598** (bands overlap — see recommendation)

## Threshold Recommendations

These values are derived from the measured distribution. Re-running this test
after changing the model or the description corpus will regenerate them.

- Within-cluster p10 / p25 / p50: **0.7119 / 0.7608 / 0.8006**
- Cross-cluster p95 / p99 / max: **0.6750 / 0.7146 / 0.7842**

**Bands overlap.** The model is not fully discriminative for this corpus
(e.g., cultivar-vs-cultivar apples score lower than fruit-vs-fruit cross pairs).
Recommended compromise defaults — prefer false-negatives (human review) over
false-positives (wrong merges):

- `AutoAcceptThreshold`: **0.81** (above cross-p99 0.7146 and within-p50 0.8006)
- `PendingReviewThreshold`: **0.68** (around cross-p95 0.6750)

At these thresholds:
- Auto-accept would fire on 24/55 true-equivalent pairs and 0/270 distinct pairs
- Pending-review would catch 27 additional true-equivalent pairs and 14 distinct pairs for human verification

## All Within-Cluster Pairs (sorted ascending)

| Cluster | Text A | Text B | Similarity |
| --- | --- | --- | ---: |
| Milk | Skim Milk Gallon | Organic Whole Milk | 0.6244 |
| Milk | Skim Milk Gallon | Lactose-Free Milk | 0.6553 |
| Milk | 2% Milk 1/2 Gallon | Lactose-Free Milk | 0.6694 |
| Milk | 2% Milk 1/2 Gallon | Organic Whole Milk | 0.6784 |
| Milk | Whole Milk 1 Gallon | Lactose-Free Milk | 0.6897 |
| Apples | Red Delicious Apples 3lb | Fuji Apples | 0.6968 |
| Grapes | Green Grapes | Red Seedless Grapes 2LB | 0.7345 |
| Apples | Fuji Apples | Pink Lady Apples | 0.7496 |
| Apples | Granny Smith Apples | Red Delicious Apples 3lb | 0.7526 |
| Grapes | Green Seedless Grapes | Grapes Red 1LB | 0.7530 |
| Canned Tomatoes | Canned Diced Tomatoes | Whole Peeled Tomatoes 28oz | 0.7543 |
| Milk | Organic Whole Milk | Lactose-Free Milk | 0.7564 |
| Apples | Granny Smith Apples | Fuji Apples | 0.7569 |
| Grapes | Green Grapes | Grapes Red 1LB | 0.7592 |
| Apples | Honeycrisp Apples | Red Delicious Apples 3lb | 0.7624 |
| Canned Tomatoes | Crushed Tomatoes 28oz | Canned Diced Tomatoes | 0.7673 |
| Milk | Whole Milk 1 Gallon | Organic Whole Milk | 0.7675 |
| Apples | Red Delicious Apples 3lb | Pink Lady Apples | 0.7677 |
| Apples | Honeycrisp Apples | Fuji Apples | 0.7719 |
| Grapes | Green Grapes | Organic Red Grapes | 0.7755 |
| Apples | Honeycrisp Apples | Granny Smith Apples | 0.7771 |
| Grapes | Red Seedless Grapes 2LB | Organic Red Grapes | 0.7807 |
| Grapes | Green Seedless Grapes | Organic Red Grapes | 0.7810 |
| Bread | White Bread Loaf | Sourdough Bread | 0.7857 |
| Grapes | Red Grapes | Green Seedless Grapes | 0.7880 |
| Apples | Honeycrisp Apples | Pink Lady Apples | 0.7964 |
| Bread | White Bread Loaf | Rye Bread | 0.7997 |
| Bread | Whole Wheat Bread | White Bread Loaf | 0.8006 |
| Milk | 2% Milk 1/2 Gallon | Skim Milk Gallon | 0.8028 |
| Bread | White Bread Loaf | Wheat Bread | 0.8047 |
| Grapes | Organic Red Grapes | Grapes Red 1LB | 0.8063 |
| Milk | Whole Milk 1 Gallon | Skim Milk Gallon | 0.8110 |
| Bread | Whole Wheat Bread | Rye Bread | 0.8138 |
| Grapes | Red Grapes | Red Seedless Grapes 2LB | 0.8229 |
| Grapes | Red Seedless Grapes 2LB | Green Seedless Grapes | 0.8240 |
| Bread | Sourdough Bread | Rye Bread | 0.8257 |
| Bread | Wheat Bread | Rye Bread | 0.8283 |
| Milk | Whole Milk 1 Gallon | 2% Milk 1/2 Gallon | 0.8298 |
| Canned Tomatoes | Diced Tomatoes 14.5oz | Canned Diced Tomatoes | 0.8303 |
| Grapes | Red Grapes | Green Grapes | 0.8304 |
| Canned Tomatoes | Diced Tomatoes 14.5oz | Crushed Tomatoes 28oz | 0.8376 |
| Apples | Granny Smith Apples | Pink Lady Apples | 0.8402 |
| Canned Tomatoes | Diced Tomatoes 14.5oz | Whole Peeled Tomatoes 28oz | 0.8409 |
| Canned Tomatoes | Diced Tomatoes 28oz | Canned Diced Tomatoes | 0.8410 |
| Bread | Whole Wheat Bread | Sourdough Bread | 0.8433 |
| Bread | Sourdough Bread | Wheat Bread | 0.8441 |
| Grapes | Red Grapes | Grapes Red 1LB | 0.8650 |
| Grapes | Red Grapes | Organic Red Grapes | 0.8651 |
| Canned Tomatoes | Crushed Tomatoes 28oz | Whole Peeled Tomatoes 28oz | 0.8823 |
| Grapes | Green Grapes | Green Seedless Grapes | 0.8985 |
| Canned Tomatoes | Diced Tomatoes 28oz | Crushed Tomatoes 28oz | 0.9063 |
| Canned Tomatoes | Diced Tomatoes 28oz | Whole Peeled Tomatoes 28oz | 0.9126 |
| Canned Tomatoes | Diced Tomatoes 14.5oz | Diced Tomatoes 28oz | 0.9130 |
| Grapes | Red Seedless Grapes 2LB | Grapes Red 1LB | 0.9226 |
| Bread | Whole Wheat Bread | Wheat Bread | 0.9397 |

## All Cross-Cluster Pairs (sorted descending, top 30)

| Cluster A | Cluster B | Text A | Text B | Similarity |
| --- | --- | --- | --- | ---: |
| Grapes | Apples | Grapes Red 1LB | Red Delicious Apples 3lb | 0.7842 |
| Grapes | Apples | Red Seedless Grapes 2LB | Red Delicious Apples 3lb | 0.7679 |
| Grapes | Canned Tomatoes | Grapes Red 1LB | Crushed Tomatoes 28oz | 0.7238 |
| Grapes | Milk | Organic Red Grapes | Organic Whole Milk | 0.7104 |
| Canned Tomatoes | Apples | Crushed Tomatoes 28oz | Red Delicious Apples 3lb | 0.7048 |
| Grapes | Canned Tomatoes | Grapes Red 1LB | Whole Peeled Tomatoes 28oz | 0.7042 |
| Grapes | Canned Tomatoes | Red Seedless Grapes 2LB | Crushed Tomatoes 28oz | 0.7024 |
| Grapes | Canned Tomatoes | Grapes Red 1LB | Diced Tomatoes 28oz | 0.7023 |
| Grapes | Canned Tomatoes | Grapes Red 1LB | Diced Tomatoes 14.5oz | 0.6947 |
| Grapes | Canned Tomatoes | Red Seedless Grapes 2LB | Whole Peeled Tomatoes 28oz | 0.6913 |
| Canned Tomatoes | Apples | Whole Peeled Tomatoes 28oz | Red Delicious Apples 3lb | 0.6912 |
| Canned Tomatoes | Apples | Diced Tomatoes 28oz | Red Delicious Apples 3lb | 0.6845 |
| Grapes | Canned Tomatoes | Red Seedless Grapes 2LB | Diced Tomatoes 14.5oz | 0.6834 |
| Grapes | Canned Tomatoes | Red Seedless Grapes 2LB | Diced Tomatoes 28oz | 0.6809 |
| Grapes | Apples | Red Grapes | Red Delicious Apples 3lb | 0.6678 |
| Canned Tomatoes | Apples | Diced Tomatoes 14.5oz | Red Delicious Apples 3lb | 0.6616 |
| Bread | Milk | Whole Wheat Bread | Organic Whole Milk | 0.6535 |
| Grapes | Milk | Red Seedless Grapes 2LB | 2% Milk 1/2 Gallon | 0.6448 |
| Grapes | Milk | Grapes Red 1LB | Whole Milk 1 Gallon | 0.6448 |
| Canned Tomatoes | Milk | Whole Peeled Tomatoes 28oz | Whole Milk 1 Gallon | 0.6404 |
| Grapes | Apples | Organic Red Grapes | Red Delicious Apples 3lb | 0.6359 |
| Grapes | Apples | Grapes Red 1LB | Granny Smith Apples | 0.6346 |
| Grapes | Milk | Grapes Red 1LB | 2% Milk 1/2 Gallon | 0.6334 |
| Grapes | Apples | Grapes Red 1LB | Pink Lady Apples | 0.6311 |
| Grapes | Apples | Red Grapes | Honeycrisp Apples | 0.6292 |
| Grapes | Canned Tomatoes | Grapes Red 1LB | Canned Diced Tomatoes | 0.6277 |
| Grapes | Apples | Red Seedless Grapes 2LB | Pink Lady Apples | 0.6262 |
| Grapes | Canned Tomatoes | Red Seedless Grapes 2LB | Canned Diced Tomatoes | 0.6235 |
| Grapes | Apples | Organic Red Grapes | Honeycrisp Apples | 0.6224 |
| Canned Tomatoes | Milk | Crushed Tomatoes 28oz | Skim Milk Gallon | 0.6207 |

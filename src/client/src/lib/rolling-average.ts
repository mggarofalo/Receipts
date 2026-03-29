export interface TimeSeriesPoint {
  period: string;
  amount: number;
}

/**
 * Compute a rolling average over a time-series dataset.
 *
 * Uses a trailing window: the value at index `i` is the average of
 * points from `max(0, i - windowSize + 1)` to `i` (inclusive).
 * When fewer than `windowSize` points are available (at the start),
 * a partial-window average is used so there are no gaps.
 */
export function computeRollingAverage(
  data: TimeSeriesPoint[],
  windowSize: number,
): TimeSeriesPoint[] {
  if (data.length === 0 || windowSize < 1) return [];

  const effectiveWindow = Math.max(1, Math.floor(windowSize));

  return data.map((point, i) => {
    const start = Math.max(0, i - effectiveWindow + 1);
    const windowSlice = data.slice(start, i + 1);
    const sum = windowSlice.reduce((acc, p) => acc + p.amount, 0);
    return {
      period: point.period,
      amount: sum / windowSlice.length,
    };
  });
}

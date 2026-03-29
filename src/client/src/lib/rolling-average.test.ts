import { computeRollingAverage, type TimeSeriesPoint } from "./rolling-average";

const sampleData: TimeSeriesPoint[] = [
  { period: "Jan", amount: 100 },
  { period: "Feb", amount: 200 },
  { period: "Mar", amount: 300 },
  { period: "Apr", amount: 400 },
  { period: "May", amount: 500 },
  { period: "Jun", amount: 600 },
];

describe("computeRollingAverage", () => {
  it("returns empty array for empty input", () => {
    expect(computeRollingAverage([], 3)).toEqual([]);
  });

  it("returns empty array for window size < 1", () => {
    expect(computeRollingAverage(sampleData, 0)).toEqual([]);
    expect(computeRollingAverage(sampleData, -1)).toEqual([]);
  });

  it("computes 3-month rolling average with partial windows at start", () => {
    const result = computeRollingAverage(sampleData, 3);
    expect(result).toHaveLength(6);
    // Partial windows
    expect(result[0]).toEqual({ period: "Jan", amount: 100 }); // avg(100)
    expect(result[1]).toEqual({ period: "Feb", amount: 150 }); // avg(100, 200)
    // Full windows
    expect(result[2]).toEqual({ period: "Mar", amount: 200 }); // avg(100, 200, 300)
    expect(result[3]).toEqual({ period: "Apr", amount: 300 }); // avg(200, 300, 400)
    expect(result[4]).toEqual({ period: "May", amount: 400 }); // avg(300, 400, 500)
    expect(result[5]).toEqual({ period: "Jun", amount: 500 }); // avg(400, 500, 600)
  });

  it("computes 6-month rolling average", () => {
    const result = computeRollingAverage(sampleData, 6);
    expect(result).toHaveLength(6);
    expect(result[5]).toEqual({ period: "Jun", amount: 350 }); // avg(100..600)
  });

  it("handles window size of 1 (returns original values)", () => {
    const result = computeRollingAverage(sampleData, 1);
    expect(result).toEqual(sampleData);
  });

  it("handles window size larger than data length", () => {
    const result = computeRollingAverage(sampleData, 12);
    expect(result).toHaveLength(6);
    // All use partial windows; last element averages everything
    expect(result[5]).toEqual({ period: "Jun", amount: 350 });
  });

  it("handles single data point", () => {
    const single = [{ period: "Jan", amount: 42 }];
    const result = computeRollingAverage(single, 3);
    expect(result).toEqual([{ period: "Jan", amount: 42 }]);
  });

  it("preserves period strings", () => {
    const result = computeRollingAverage(sampleData, 3);
    expect(result.map((p) => p.period)).toEqual([
      "Jan",
      "Feb",
      "Mar",
      "Apr",
      "May",
      "Jun",
    ]);
  });

  it("handles fractional window size by flooring", () => {
    const result = computeRollingAverage(sampleData, 3.7);
    const expected = computeRollingAverage(sampleData, 3);
    expect(result).toEqual(expected);
  });
});

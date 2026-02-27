import { describe, it, expect } from "vitest";
import { formatCurrency, formatDecimal, parseCurrencyInput } from "./format";

describe("formatCurrency", () => {
  it("formats a positive number as USD", () => {
    expect(formatCurrency(1234.56)).toBe("$1,234.56");
  });

  it("formats zero", () => {
    expect(formatCurrency(0)).toBe("$0.00");
  });

  it("formats negative numbers", () => {
    expect(formatCurrency(-42.5)).toBe("-$42.50");
  });

  it("rounds to two decimal places", () => {
    expect(formatCurrency(9.999)).toBe("$10.00");
  });

  it("formats large numbers with grouping", () => {
    expect(formatCurrency(1000000)).toBe("$1,000,000.00");
  });
});

describe("formatDecimal", () => {
  it("pads to two decimal places by default", () => {
    expect(formatDecimal(3.1)).toBe("3.10");
  });

  it("formats whole numbers with decimals", () => {
    expect(formatDecimal(5)).toBe("5.00");
  });

  it("uses custom decimal places", () => {
    expect(formatDecimal(3.1, 3)).toBe("3.100");
  });

  it("rounds when necessary", () => {
    expect(formatDecimal(1.999, 2)).toBe("2.00");
  });

  it("handles zero decimal places", () => {
    expect(formatDecimal(3.7, 0)).toBe("4");
  });
});

describe("parseCurrencyInput", () => {
  it("strips dollar signs", () => {
    expect(parseCurrencyInput("$123.45")).toBe("123.45");
  });

  it("strips commas", () => {
    expect(parseCurrencyInput("1,234.56")).toBe("1234.56");
  });

  it("strips letters", () => {
    expect(parseCurrencyInput("abc123")).toBe("123");
  });

  it("prevents multiple decimal points", () => {
    expect(parseCurrencyInput("1.2.3")).toBe("1.23");
  });

  it("allows a single decimal point", () => {
    expect(parseCurrencyInput("99.99")).toBe("99.99");
  });

  it("returns empty string for non-numeric input", () => {
    expect(parseCurrencyInput("abc")).toBe("");
  });
});

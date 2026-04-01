import { describe, it, expect } from "vitest";
import { formatCurrency, formatDecimal, parseCurrencyInput, camelToTitle, capitalize, evaluateMathExpression } from "./format";

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

describe("camelToTitle", () => {
  it("converts camelCase to Title Case", () => {
    expect(camelToTitle("taxAmount")).toBe("Tax Amount");
  });

  it("converts PascalCase to Title Case", () => {
    expect(camelToTitle("TokenRefresh")).toBe("Token Refresh");
  });

  it("handles multiple humps", () => {
    expect(camelToTitle("loyaltyRedemption")).toBe("Loyalty Redemption");
  });

  it("handles single word", () => {
    expect(camelToTitle("discount")).toBe("Discount");
  });

  it("handles already capitalized single word", () => {
    expect(camelToTitle("Login")).toBe("Login");
  });

  it("handles consecutive capitals", () => {
    expect(camelToTitle("apiKeyAuth")).toBe("Api Key Auth");
  });
});

describe("evaluateMathExpression", () => {
  it("evaluates a single number", () => {
    expect(evaluateMathExpression("42")).toBe(42);
  });

  it("evaluates a decimal number", () => {
    expect(evaluateMathExpression("24.99")).toBe(24.99);
  });

  it("evaluates addition", () => {
    expect(evaluateMathExpression("10+5")).toBe(15);
  });

  it("evaluates subtraction", () => {
    expect(evaluateMathExpression("24.99-7.30")).toBeCloseTo(17.69);
  });

  it("evaluates multiplication", () => {
    expect(evaluateMathExpression("3*4")).toBe(12);
  });

  it("evaluates division", () => {
    expect(evaluateMathExpression("10/4")).toBe(2.5);
  });

  it("respects operator precedence (multiply before add)", () => {
    expect(evaluateMathExpression("2+3*4")).toBe(14);
  });

  it("respects operator precedence (divide before subtract)", () => {
    expect(evaluateMathExpression("10-6/3")).toBe(8);
  });

  it("evaluates chained operations", () => {
    expect(evaluateMathExpression("1+2+3+4")).toBe(10);
  });

  it("handles parentheses", () => {
    expect(evaluateMathExpression("(2+3)*4")).toBe(20);
  });

  it("handles nested parentheses", () => {
    expect(evaluateMathExpression("((2+3)*4)+1")).toBe(21);
  });

  it("handles leading negative number", () => {
    expect(evaluateMathExpression("-5+3")).toBe(-2);
  });

  it("handles negative in parentheses", () => {
    expect(evaluateMathExpression("10+(-3)")).toBe(7);
  });

  it("ignores whitespace", () => {
    expect(evaluateMathExpression(" 10 + 5 ")).toBe(15);
  });

  it("returns NaN for empty string", () => {
    expect(evaluateMathExpression("")).toBeNaN();
  });

  it("returns NaN for non-numeric input", () => {
    expect(evaluateMathExpression("abc")).toBeNaN();
  });

  it("returns NaN for incomplete expression", () => {
    expect(evaluateMathExpression("5+")).toBeNaN();
  });

  it("returns NaN for unmatched parenthesis", () => {
    expect(evaluateMathExpression("(5+3")).toBeNaN();
  });

  it("returns Infinity for division by zero", () => {
    expect(evaluateMathExpression("5/0")).toBe(Infinity);
  });

  it("handles complex real-world expression", () => {
    // e.g. item price minus discount
    expect(evaluateMathExpression("24.99-7.30")).toBeCloseTo(17.69);
  });

  it("handles multiplication with decimals", () => {
    expect(evaluateMathExpression("4.50*3")).toBeCloseTo(13.5);
  });
});

describe("capitalize", () => {
  it("capitalizes lowercase string", () => {
    expect(capitalize("active")).toBe("Active");
  });

  it("leaves already capitalized string unchanged", () => {
    expect(capitalize("Active")).toBe("Active");
  });

  it("handles empty string", () => {
    expect(capitalize("")).toBe("");
  });
});

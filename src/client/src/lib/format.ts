export function formatCurrency(amount: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(amount);
}

export function formatDecimal(value: number, decimals = 2): string {
  return value.toFixed(decimals);
}

export function parseCurrencyInput(raw: string): string {
  return raw.replace(/[^0-9.]/g, "").replace(/(\..*)\./g, "$1");
}

/**
 * Parse and evaluate a simple arithmetic expression containing +, -, *, /
 * with standard operator precedence and optional parentheses.
 *
 * Uses a recursive-descent parser — no eval() or Function() for safety.
 * Returns NaN for invalid or empty expressions, and Infinity/-Infinity for
 * division by zero (callers should treat those as invalid).
 */
export function evaluateMathExpression(input: string): number {
  const expr = input.replace(/\s/g, "");
  if (expr === "") return NaN;

  let pos = 0;

  function peek(): string {
    return expr[pos] ?? "";
  }

  function consume(): string {
    return expr[pos++];
  }

  // number = ["-"] digit+ ["." digit+]
  function parseNumber(): number {
    const start = pos;
    if (peek() === "-") consume();
    if (!/[0-9.]/.test(peek())) return NaN;
    while (/[0-9]/.test(peek())) consume();
    if (peek() === ".") {
      consume();
      while (/[0-9]/.test(peek())) consume();
    }
    return parseFloat(expr.slice(start, pos));
  }

  // atom = number | "(" expression ")"
  function parseAtom(): number {
    if (peek() === "(") {
      consume(); // "("
      const val = parseExpression();
      if (peek() === ")") consume();
      else return NaN; // unmatched paren
      return val;
    }
    return parseNumber();
  }

  // unary = ["-"] atom  (handles negation before parenthesised sub-expressions)
  function parseUnary(): number {
    if (peek() === "-") {
      consume();
      return -parseAtom();
    }
    return parseAtom();
  }

  // term = unary (("*" | "/") unary)*
  function parseTerm(): number {
    let left = parseUnary();
    while (peek() === "*" || peek() === "/") {
      const op = consume();
      const right = parseUnary();
      left = op === "*" ? left * right : left / right;
    }
    return left;
  }

  // expression = term (("+" | "-") term)*
  function parseExpression(): number {
    let left = parseTerm();
    while (peek() === "+" || peek() === "-") {
      const op = consume();
      const right = parseTerm();
      left = op === "+" ? left + right : left - right;
    }
    return left;
  }

  const result = parseExpression();

  // If there are leftover characters the expression was malformed
  if (pos < expr.length) return NaN;

  return result;
}

/**
 * Convert a camelCase or PascalCase string to Title Case with spaces.
 * e.g. "loyaltyRedemption" → "Loyalty Redemption", "taxAmount" → "Tax Amount"
 */
export function camelToTitle(str: string): string {
  return str
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .replace(/^./, (c) => c.toUpperCase());
}

/**
 * Capitalize the first letter of a string.
 */
export function capitalize(str: string): string {
  return str.charAt(0).toUpperCase() + str.slice(1);
}

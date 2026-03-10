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

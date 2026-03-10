export const ADJUSTMENT_TYPES = [
  { value: "tip", label: "Tip" },
  { value: "discount", label: "Discount" },
  { value: "rounding", label: "Rounding" },
  { value: "loyaltyRedemption", label: "Loyalty Redemption" },
  { value: "coupon", label: "Coupon" },
  { value: "storeCredit", label: "Store Credit" },
  { value: "other", label: "Other" },
] as const;

const ADJUSTMENT_TYPE_LABELS: Record<string, string> = Object.fromEntries(
  ADJUSTMENT_TYPES.map((t) => [t.value, t.label]),
);

export function formatAdjustmentType(type: string): string {
  return ADJUSTMENT_TYPE_LABELS[type] ?? type;
}

import type { components } from "@/generated/api";

type ReceiptResponse = components["schemas"]["ReceiptResponse"];

export const receipts: ReceiptResponse[] = [
  {
    id: "aaaa1111-1111-1111-1111-111111111111",
    location: "Grocery Store",
    date: "2025-01-15",
    taxAmount: 5.25,
  },
  {
    id: "aaaa2222-2222-2222-2222-222222222222",
    location: "Hardware Store",
    date: "2025-01-20",
    taxAmount: 3.5,
  },
  {
    id: "aaaa3333-3333-3333-3333-333333333333",
    location: "Restaurant",
    date: "2025-02-01",
    taxAmount: 2.75,
  },
];

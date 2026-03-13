import type { components } from "@/generated/api";

type TransactionResponse = components["schemas"]["TransactionResponse"];

export const transactions: TransactionResponse[] = [
  {
    id: "cccc1111-1111-1111-1111-111111111111",
    receiptId: "aaaa1111-1111-1111-1111-111111111111",
    accountId: "11111111-1111-1111-1111-111111111111",
    amount: 17.73,
    date: "2025-01-15",
  },
  {
    id: "cccc2222-2222-2222-2222-222222222222",
    receiptId: "aaaa2222-2222-2222-2222-222222222222",
    accountId: "22222222-2222-2222-2222-222222222222",
    amount: 28.49,
    date: "2025-01-20",
  },
  {
    id: "cccc3333-3333-3333-3333-333333333333",
    receiptId: "aaaa3333-3333-3333-3333-333333333333",
    accountId: "11111111-1111-1111-1111-111111111111",
    amount: 15.0,
    date: "2025-02-01",
  },
];

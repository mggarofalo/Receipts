import type { components } from "@/generated/api";
import { receipts } from "./receipts";
import { receiptItems } from "./receipt-items";
import { transactions } from "./transactions";
import { accounts } from "./accounts";

type TripResponse = components["schemas"]["TripResponse"];

const receipt = receipts[0];
const items = receiptItems.filter((i) => i.receiptId === receipt.id);
const txns = transactions.filter((t) => t.receiptId === receipt.id);

export const tripResponse: TripResponse = {
  receipt: {
    receipt,
    items,
    adjustments: [],
    subtotal: items.reduce((sum, i) => sum + Number(i.quantity ?? 0) * Number(i.unitPrice ?? 0), 0),
    adjustmentTotal: 0,
    expectedTotal: items.reduce((sum, i) => sum + Number(i.quantity ?? 0) * Number(i.unitPrice ?? 0), 0) + Number(receipt.taxAmount ?? 0),
    warnings: [],
  },
  transactions: txns.map((t) => ({
    transaction: t,
    account: accounts.find((a) => a.id === t.accountId)!,
  })),
  warnings: [],
};

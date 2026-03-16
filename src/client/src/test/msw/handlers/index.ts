import { authHandlers } from "./auth";
import { accountHandlers } from "./accounts";
import { receiptHandlers } from "./receipts";
import { receiptItemHandlers } from "./receipt-items";
import { transactionHandlers } from "./transactions";
import { tripHandlers } from "./trips";
import { metadataHandlers } from "./metadata";

export const handlers = [
  ...authHandlers,
  ...accountHandlers,
  ...receiptHandlers,
  ...receiptItemHandlers,
  ...transactionHandlers,
  ...tripHandlers,
  ...metadataHandlers,
];

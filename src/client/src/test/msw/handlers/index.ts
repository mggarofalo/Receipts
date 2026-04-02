import { authHandlers } from "./auth";
import { accountHandlers } from "./accounts";
import { categoryHandlers } from "./categories";
import { subcategoryHandlers } from "./subcategories";
import { receiptHandlers } from "./receipts";
import { receiptItemHandlers } from "./receipt-items";
import { transactionHandlers } from "./transactions";
import { tripHandlers } from "./trips";
import { metadataHandlers } from "./metadata";
import { scanHandlers } from "./scan";

export const handlers = [
  ...authHandlers,
  ...accountHandlers,
  ...categoryHandlers,
  ...subcategoryHandlers,
  ...receiptHandlers,
  ...receiptItemHandlers,
  ...transactionHandlers,
  ...tripHandlers,
  ...metadataHandlers,
  ...scanHandlers,
];

import { authHandlers } from "./auth";
import { cardHandlers } from "./cards";
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
  ...cardHandlers,
  ...categoryHandlers,
  ...subcategoryHandlers,
  ...receiptHandlers,
  ...receiptItemHandlers,
  ...transactionHandlers,
  ...tripHandlers,
  ...metadataHandlers,
  ...scanHandlers,
];

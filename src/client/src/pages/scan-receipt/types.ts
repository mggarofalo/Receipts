import type { components } from "@/generated/api";

export type ConfidenceLevel = components["schemas"]["ConfidenceLevel"];
export type ProposedReceiptResponse =
  components["schemas"]["ProposedReceiptResponse"];
export type ProposedReceiptItemResponse =
  components["schemas"]["ProposedReceiptItemResponse"];
export type ProposedTaxLineResponse =
  components["schemas"]["ProposedTaxLineResponse"];

// Internal UI types with no API counterpart
export interface ReceiptConfidenceMap {
  location?: ConfidenceLevel;
  date?: ConfidenceLevel;
  taxAmount?: ConfidenceLevel;
}

export interface ScanInitialValues {
  header: {
    location: string;
    date: string;
    taxAmount: number;
  };
  items: Array<{
    receiptItemCode: string;
    description: string;
    pricingMode: "quantity" | "flat";
    quantity: number;
    unitPrice: number;
    category: string;
    subcategory: string;
  }>;
}

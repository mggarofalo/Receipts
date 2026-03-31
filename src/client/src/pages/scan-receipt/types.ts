export type ConfidenceLevel = "low" | "medium" | "high";

export interface ProposedReceiptItem {
  code: string | null;
  codeConfidence: ConfidenceLevel;
  description: string | null;
  descriptionConfidence: ConfidenceLevel;
  quantity: number | null;
  quantityConfidence: ConfidenceLevel;
  unitPrice: number | null;
  unitPriceConfidence: ConfidenceLevel;
  totalPrice: number | null;
  totalPriceConfidence: ConfidenceLevel;
}

export interface ProposedTaxLine {
  label: string | null;
  labelConfidence: ConfidenceLevel;
  amount: number | null;
  amountConfidence: ConfidenceLevel;
}

export interface ProposedReceiptResponse {
  storeName: string | null;
  storeNameConfidence: ConfidenceLevel;
  date: string | null;
  dateConfidence: ConfidenceLevel;
  items: ProposedReceiptItem[];
  subtotal: number | null;
  subtotalConfidence: ConfidenceLevel;
  taxLines: ProposedTaxLine[];
  total: number | null;
  totalConfidence: ConfidenceLevel;
  paymentMethod: string | null;
  paymentMethodConfidence: ConfidenceLevel;
  rawOcrText: string;
  ocrConfidence: number;
}

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

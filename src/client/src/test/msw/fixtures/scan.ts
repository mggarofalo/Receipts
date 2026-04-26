import type { components } from "@/generated/api";

type ProposedReceiptResponse = components["schemas"]["ProposedReceiptResponse"];

/**
 * Fixture for the VLM scan endpoint (`POST /api/receipts/scan`).
 *
 * Typed as {@link ProposedReceiptResponse} so TypeScript enforces field
 * completeness against the OpenAPI contract — adding a new required field
 * upstream causes this file to fail to type-check, preventing the silent
 * runtime crash described in RECEIPTS-632.
 *
 * Confidence values are intentionally a mix of high/medium/low so the
 * new-receipt wizard's badge rendering is exercised under realistic data.
 */
export const scanProposal: ProposedReceiptResponse = {
  storeName: "Walmart Supercenter",
  storeNameConfidence: "high",
  storeAddress: "123 Main St, Springfield, IL 62701",
  storeAddressConfidence: "medium",
  storePhone: "(555) 123-4567",
  storePhoneConfidence: "low",
  date: "2024-06-15",
  dateConfidence: "high",
  items: [
    {
      code: "MILK-GAL",
      codeConfidence: "high",
      description: "Great Value Whole Milk",
      descriptionConfidence: "high",
      quantity: 2,
      quantityConfidence: "high",
      unitPrice: 3.99,
      unitPriceConfidence: "medium",
      totalPrice: 7.98,
      totalPriceConfidence: "high",
      taxCode: "F",
      taxCodeConfidence: "high",
    },
    {
      code: null,
      codeConfidence: "low",
      description: "Bananas",
      descriptionConfidence: "medium",
      quantity: 1,
      quantityConfidence: "high",
      unitPrice: 1.29,
      unitPriceConfidence: "low",
      totalPrice: 1.29,
      totalPriceConfidence: "medium",
      taxCode: null,
      taxCodeConfidence: "low",
    },
  ],
  subtotal: 9.27,
  subtotalConfidence: "high",
  taxLines: [
    {
      label: "Tax",
      labelConfidence: "high",
      amount: 0.74,
      amountConfidence: "medium",
    },
  ],
  total: 10.01,
  totalConfidence: "high",
  paymentMethod: "VISA",
  paymentMethodConfidence: "medium",
  payments: [
    {
      method: "VISA",
      methodConfidence: "high",
      amount: 10.01,
      amountConfidence: "high",
      lastFour: "4242",
      lastFourConfidence: "medium",
    },
  ],
  receiptId: "TX-987654321",
  receiptIdConfidence: "high",
  storeNumber: "0042",
  storeNumberConfidence: "medium",
  terminalId: "T01",
  terminalIdConfidence: "low",
};

import type { components } from "@/generated/api";
import type { ScanInitialValues, ReceiptConfidenceMap } from "./types";

type ProposedReceiptResponse = components["schemas"]["ProposedReceiptResponse"];

/**
 * Map a {@link ProposedReceiptResponse} (returned by the VLM scan endpoint)
 * to the {@link ScanInitialValues} shape consumed by the new-receipt wizard.
 */
export function mapProposalToInitialValues(
  proposal: ProposedReceiptResponse,
): ScanInitialValues {
  const taxAmount = Number(proposal.taxLines[0]?.amount ?? 0);

  let date = "";
  if (proposal.date) {
    // The API returns a DateOnly (YYYY-MM-DD). If it comes as ISO datetime, extract the date part.
    date = proposal.date.split("T")[0];
  }

  return {
    header: {
      location: proposal.storeName ?? "",
      date,
      taxAmount,
      storeAddress: proposal.storeAddress ?? "",
      storePhone: proposal.storePhone ?? "",
    },
    metadata: {
      receiptId: proposal.receiptId ?? "",
      storeNumber: proposal.storeNumber ?? "",
      terminalId: proposal.terminalId ?? "",
    },
    payments: proposal.payments.map((p) => ({
      method: p.method ?? "",
      amount: Number(p.amount ?? 0),
      lastFour: p.lastFour ?? "",
    })),
    items: proposal.items.map((item) => ({
      receiptItemCode: item.code ?? "",
      description: item.description ?? "",
      pricingMode: "quantity" as const,
      quantity: Number(item.quantity ?? 1),
      unitPrice: Number(item.unitPrice ?? 0),
      category: "",
      subcategory: "",
      taxCode: item.taxCode ?? "",
    })),
  };
}

/**
 * Build a confidence map from the proposal that highlights low/medium fields
 * (so the new-receipt wizard can render review badges).
 *
 * Fields whose confidence is "high" are intentionally omitted — the absence
 * of a key signals "no badge needed".
 */
export function mapProposalToConfidenceMap(
  proposal: ProposedReceiptResponse,
): ReceiptConfidenceMap {
  const map: ReceiptConfidenceMap = {};

  if (proposal.storeNameConfidence !== "high") {
    map.location = proposal.storeNameConfidence;
  }
  if (proposal.dateConfidence !== "high") {
    map.date = proposal.dateConfidence;
  }

  // Use the first tax line's confidence, or the subtotal confidence as fallback.
  const taxConfidence =
    proposal.taxLines[0]?.amountConfidence ?? proposal.subtotalConfidence;
  if (taxConfidence !== "high") {
    map.taxAmount = taxConfidence;
  }

  if (proposal.storeAddressConfidence !== "high") {
    map.storeAddress = proposal.storeAddressConfidence;
  }
  if (proposal.storePhoneConfidence !== "high") {
    map.storePhone = proposal.storePhoneConfidence;
  }
  if (proposal.receiptIdConfidence !== "high") {
    map.receiptId = proposal.receiptIdConfidence;
  }
  if (proposal.storeNumberConfidence !== "high") {
    map.storeNumber = proposal.storeNumberConfidence;
  }
  if (proposal.terminalIdConfidence !== "high") {
    map.terminalId = proposal.terminalIdConfidence;
  }

  // Per-payment confidences. Always emit an entry per payment so indices align,
  // omitting fields whose confidence is "high".
  if (proposal.payments.length > 0) {
    map.payments = proposal.payments.map((p) => {
      const entry: NonNullable<ReceiptConfidenceMap["payments"]>[number] = {};
      if (p.methodConfidence !== "high") entry.method = p.methodConfidence;
      if (p.amountConfidence !== "high") entry.amount = p.amountConfidence;
      if (p.lastFourConfidence !== "high") entry.lastFour = p.lastFourConfidence;
      return entry;
    });
  }

  // Per-item taxCode confidences.
  if (proposal.items.length > 0) {
    const itemEntries = proposal.items.map((item) => {
      const entry: NonNullable<ReceiptConfidenceMap["items"]>[number] = {};
      if (item.taxCodeConfidence !== "high") {
        entry.taxCode = item.taxCodeConfidence;
      }
      return entry;
    });
    // Only include if any entry has at least one non-high confidence
    if (itemEntries.some((e) => Object.keys(e).length > 0)) {
      map.items = itemEntries;
    }
  }

  return map;
}

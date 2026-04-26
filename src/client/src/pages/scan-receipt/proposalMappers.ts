import type { components } from "@/generated/api";
import { generateId } from "@/lib/id";
import type { ReceiptLineItem } from "@/pages/new-receipt/LineItemsSection";
import type { ReceiptPayment } from "@/pages/new-receipt/PaymentsSection";
import type {
  ConfidenceLevel,
  ScanInitialValues,
  ReceiptConfidenceMap,
} from "./types";

type ProposedReceiptResponse = components["schemas"]["ProposedReceiptResponse"];

type ItemConfidenceEntry = NonNullable<ReceiptConfidenceMap["items"]>[number];
type PaymentConfidenceEntry = NonNullable<
  ReceiptConfidenceMap["payments"]
>[number];

/**
 * True when a confidence value indicates the user needs to review the field.
 * "high" — extracted with high confidence; no review needed.
 * "none" — the source receipt did not contain this field; there is nothing to review.
 * "low" / "medium" — extracted with reduced confidence; surface to the user.
 */
function needsReview(confidence: ConfidenceLevel): boolean {
  return confidence !== "high" && confidence !== "none";
}

/**
 * Map a {@link ProposedReceiptResponse} (returned by the VLM scan endpoint)
 * to the {@link ScanInitialValues} shape consumed by the new-receipt wizard.
 */
export function mapProposalToInitialValues(
  proposal: ProposedReceiptResponse,
): ScanInitialValues {
  // Defensive guards: the OpenAPI contract declares these arrays as required
  // (non-optional), but a stale fixture or partially-stubbed test handler can
  // omit them. Coalescing to [] prevents a hard `TypeError: Cannot read
  // properties of undefined` and keeps the wizard usable. See RECEIPTS-632.
  const taxLines = proposal.taxLines ?? [];
  const payments = proposal.payments ?? [];
  const items = proposal.items ?? [];

  const taxAmount = Number(taxLines[0]?.amount ?? 0);

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
    payments: payments.map((p) => ({
      method: p.method ?? "",
      amount: Number(p.amount ?? 0),
      lastFour: p.lastFour ?? "",
    })),
    items: items.map((item) => ({
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
 * Fields whose confidence is "high" or "none" are intentionally omitted — the
 * absence of a key signals "no badge needed". "high" means we are confident in
 * the extracted value; "none" means the source receipt did not contain that
 * field at all (so there is nothing for the user to review).
 */
export function mapProposalToConfidenceMap(
  proposal: ProposedReceiptResponse,
): ReceiptConfidenceMap {
  // See note in mapProposalToInitialValues — same defensive guards.
  const taxLines = proposal.taxLines ?? [];
  const payments = proposal.payments ?? [];
  const items = proposal.items ?? [];

  const map: ReceiptConfidenceMap = {};

  if (needsReview(proposal.storeNameConfidence)) {
    map.location = proposal.storeNameConfidence;
  }
  if (needsReview(proposal.dateConfidence)) {
    map.date = proposal.dateConfidence;
  }

  // Use the first tax line's confidence, or the subtotal confidence as fallback.
  const taxConfidence =
    taxLines[0]?.amountConfidence ?? proposal.subtotalConfidence;
  if (needsReview(taxConfidence)) {
    map.taxAmount = taxConfidence;
  }

  if (needsReview(proposal.storeAddressConfidence)) {
    map.storeAddress = proposal.storeAddressConfidence;
  }
  if (needsReview(proposal.storePhoneConfidence)) {
    map.storePhone = proposal.storePhoneConfidence;
  }
  if (needsReview(proposal.receiptIdConfidence)) {
    map.receiptId = proposal.receiptIdConfidence;
  }
  if (needsReview(proposal.storeNumberConfidence)) {
    map.storeNumber = proposal.storeNumberConfidence;
  }
  if (needsReview(proposal.terminalIdConfidence)) {
    map.terminalId = proposal.terminalIdConfidence;
  }

  // Per-payment confidences. Always emit an entry per payment so indices align,
  // omitting fields whose confidence is "high" or "none".
  if (payments.length > 0) {
    map.payments = payments.map((p) => {
      const entry: PaymentConfidenceEntry = {};
      if (needsReview(p.methodConfidence)) entry.method = p.methodConfidence;
      if (needsReview(p.amountConfidence)) entry.amount = p.amountConfidence;
      if (needsReview(p.lastFourConfidence))
        entry.lastFour = p.lastFourConfidence;
      return entry;
    });
  }

  // Per-item taxCode confidences.
  if (items.length > 0) {
    const itemEntries = items.map((item) => {
      const entry: ItemConfidenceEntry = {};
      if (needsReview(item.taxCodeConfidence)) {
        entry.taxCode = item.taxCodeConfidence;
      }
      return entry;
    });
    // Only include if any entry has at least one non-high/non-none confidence
    if (itemEntries.some((e) => Object.keys(e).length > 0)) {
      map.items = itemEntries;
    }
  }

  return map;
}

/**
 * Build the new-receipt wizard's initial line items along with a confidence
 * map keyed by the freshly-generated row id. Pairing confidence with id
 * (rather than index) keeps confidence correctly attached to a row after
 * additions or deletions. The map is write-once: stale entries for deleted
 * rows are harmless because the row will never be looked up again.
 */
export function initialItemsAndConfidence(
  initialValues: ScanInitialValues | undefined,
  confidenceMap: ReceiptConfidenceMap | undefined,
): {
  items: ReceiptLineItem[];
  itemConfidenceById: Map<string, ItemConfidenceEntry>;
} {
  const sourceItems = initialValues?.items ?? [];
  const sourceConfidence = confidenceMap?.items ?? [];

  const items: ReceiptLineItem[] = sourceItems.map((item) => ({
    id: generateId(),
    ...item,
  }));
  const itemConfidenceById = new Map<string, ItemConfidenceEntry>();
  for (let i = 0; i < items.length; i++) {
    const entry = sourceConfidence[i];
    if (entry) {
      itemConfidenceById.set(items[i].id, entry);
    }
  }
  return { items, itemConfidenceById };
}

/**
 * Build the new-receipt wizard's initial payments along with a confidence
 * map keyed by the freshly-generated row id. See {@link initialItemsAndConfidence}
 * for rationale.
 */
export function initialPaymentsAndConfidence(
  initialValues: ScanInitialValues | undefined,
  confidenceMap: ReceiptConfidenceMap | undefined,
): {
  payments: ReceiptPayment[];
  paymentConfidenceById: Map<string, PaymentConfidenceEntry>;
} {
  const sourcePayments = initialValues?.payments ?? [];
  const sourceConfidence = confidenceMap?.payments ?? [];

  const payments: ReceiptPayment[] = sourcePayments.map((p) => ({
    id: generateId(),
    method: p.method,
    amount: p.amount,
    lastFour: p.lastFour,
  }));
  const paymentConfidenceById = new Map<string, PaymentConfidenceEntry>();
  for (let i = 0; i < payments.length; i++) {
    const entry = sourceConfidence[i];
    if (entry) {
      paymentConfidenceById.set(payments[i].id, entry);
    }
  }
  return { payments, paymentConfidenceById };
}

import { describe, expect, it } from "vitest";
import {
  mapProposalToInitialValues,
  mapProposalToConfidenceMap,
} from "./proposalMappers";
import type { components } from "@/generated/api";

type ProposedReceiptResponse = components["schemas"]["ProposedReceiptResponse"];

function makeProposal(
  overrides: Partial<ProposedReceiptResponse> = {},
): ProposedReceiptResponse {
  return {
    storeName: "Test Store",
    storeNameConfidence: "high",
    storeAddress: null,
    storeAddressConfidence: "high",
    storePhone: null,
    storePhoneConfidence: "high",
    date: "2024-06-15",
    dateConfidence: "high",
    items: [],
    subtotal: 0,
    subtotalConfidence: "high",
    taxLines: [],
    total: 0,
    totalConfidence: "high",
    paymentMethod: null,
    paymentMethodConfidence: "high",
    payments: [],
    receiptId: null,
    receiptIdConfidence: "high",
    storeNumber: null,
    storeNumberConfidence: "high",
    terminalId: null,
    terminalIdConfidence: "high",
    ...overrides,
  };
}

describe("mapProposalToInitialValues", () => {
  it("populates header from proposal including new address/phone fields", () => {
    const proposal = makeProposal({
      storeName: "Walmart",
      storeAddress: "123 Main St, Springfield",
      storePhone: "(555) 123-4567",
      date: "2024-06-15",
      taxLines: [
        {
          label: "Tax",
          labelConfidence: "high",
          amount: 1.25,
          amountConfidence: "high",
        },
      ],
    });

    const result = mapProposalToInitialValues(proposal);

    expect(result.header).toEqual({
      location: "Walmart",
      date: "2024-06-15",
      taxAmount: 1.25,
      storeAddress: "123 Main St, Springfield",
      storePhone: "(555) 123-4567",
    });
  });

  it("populates metadata with receiptId, storeNumber, and terminalId", () => {
    const proposal = makeProposal({
      receiptId: "TX-987654",
      storeNumber: "0042",
      terminalId: "T01",
    });

    const result = mapProposalToInitialValues(proposal);

    expect(result.metadata).toEqual({
      receiptId: "TX-987654",
      storeNumber: "0042",
      terminalId: "T01",
    });
  });

  it("populates payments array preserving order", () => {
    const proposal = makeProposal({
      payments: [
        {
          method: "MASTERCARD",
          methodConfidence: "high",
          amount: 54.32,
          amountConfidence: "high",
          lastFour: "4538",
          lastFourConfidence: "high",
        },
        {
          method: "Cash",
          methodConfidence: "high",
          amount: 5.0,
          amountConfidence: "high",
          lastFour: null,
          lastFourConfidence: "high",
        },
      ],
    });

    const result = mapProposalToInitialValues(proposal);

    expect(result.payments).toEqual([
      { method: "MASTERCARD", amount: 54.32, lastFour: "4538" },
      { method: "Cash", amount: 5.0, lastFour: "" },
    ]);
  });

  it("populates per-item taxCode", () => {
    const proposal = makeProposal({
      items: [
        {
          code: "MILK-GAL",
          codeConfidence: "high",
          description: "Whole milk",
          descriptionConfidence: "high",
          quantity: 1,
          quantityConfidence: "high",
          unitPrice: 3.99,
          unitPriceConfidence: "high",
          totalPrice: 3.99,
          totalPriceConfidence: "high",
          taxCode: "F",
          taxCodeConfidence: "high",
        },
      ],
    });

    const result = mapProposalToInitialValues(proposal);

    expect(result.items[0].taxCode).toBe("F");
  });

  it("defaults nullable fields to empty strings or zero", () => {
    const proposal = makeProposal({
      storeName: null,
      storeAddress: null,
      storePhone: null,
      date: null,
      receiptId: null,
      storeNumber: null,
      terminalId: null,
    });

    const result = mapProposalToInitialValues(proposal);

    expect(result.header.location).toBe("");
    expect(result.header.date).toBe("");
    expect(result.header.storeAddress).toBe("");
    expect(result.header.storePhone).toBe("");
    expect(result.metadata).toEqual({
      receiptId: "",
      storeNumber: "",
      terminalId: "",
    });
    expect(result.payments).toEqual([]);
    expect(result.items).toEqual([]);
  });

  it("strips ISO time component from a datetime date", () => {
    const proposal = makeProposal({ date: "2024-06-15T12:34:56Z" });

    const result = mapProposalToInitialValues(proposal);

    expect(result.header.date).toBe("2024-06-15");
  });
});

describe("mapProposalToConfidenceMap", () => {
  it("omits high-confidence fields", () => {
    const proposal = makeProposal();

    const result = mapProposalToConfidenceMap(proposal);

    expect(result).toEqual({});
  });

  it("flags low/medium confidence on the new fields", () => {
    const proposal = makeProposal({
      storeAddress: "123 Main St",
      storeAddressConfidence: "low",
      storePhone: "(555) 123-4567",
      storePhoneConfidence: "medium",
      receiptId: "TX-1",
      receiptIdConfidence: "low",
      storeNumber: "0042",
      storeNumberConfidence: "medium",
      terminalId: "T01",
      terminalIdConfidence: "low",
    });

    const result = mapProposalToConfidenceMap(proposal);

    expect(result).toMatchObject({
      storeAddress: "low",
      storePhone: "medium",
      receiptId: "low",
      storeNumber: "medium",
      terminalId: "low",
    });
  });

  it("emits a payments array entry for each payment, dropping high-confidence fields", () => {
    const proposal = makeProposal({
      payments: [
        {
          method: "MASTERCARD",
          methodConfidence: "high",
          amount: 54.32,
          amountConfidence: "low",
          lastFour: "4538",
          lastFourConfidence: "medium",
        },
        {
          method: "Cash",
          methodConfidence: "high",
          amount: 5,
          amountConfidence: "high",
          lastFour: null,
          lastFourConfidence: "high",
        },
      ],
    });

    const result = mapProposalToConfidenceMap(proposal);

    expect(result.payments).toEqual([
      { amount: "low", lastFour: "medium" },
      {},
    ]);
  });

  it("does not include payments key when no payments exist", () => {
    const proposal = makeProposal({ payments: [] });

    const result = mapProposalToConfidenceMap(proposal);

    expect(result.payments).toBeUndefined();
  });

  it("includes items entries only when at least one taxCode confidence is non-high", () => {
    const lowItem = {
      code: "X",
      codeConfidence: "high" as const,
      description: "x",
      descriptionConfidence: "high" as const,
      quantity: 1,
      quantityConfidence: "high" as const,
      unitPrice: 1,
      unitPriceConfidence: "high" as const,
      totalPrice: 1,
      totalPriceConfidence: "high" as const,
      taxCode: "F",
      taxCodeConfidence: "low" as const,
    };
    const highItem = { ...lowItem, taxCodeConfidence: "high" as const };

    const allHigh = mapProposalToConfidenceMap(
      makeProposal({ items: [highItem] }),
    );
    expect(allHigh.items).toBeUndefined();

    const someLow = mapProposalToConfidenceMap(
      makeProposal({ items: [highItem, lowItem] }),
    );
    expect(someLow.items).toEqual([{}, { taxCode: "low" }]);
  });
});

import { describe, expect, it } from "vitest";
import {
  initialItemsAndConfidence,
  initialPaymentsAndConfidence,
  mapProposalToInitialValues,
  mapProposalToConfidenceMap,
} from "./proposalMappers";
import type { ScanInitialValues, ReceiptConfidenceMap } from "./types";
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

  it("omits 'none' confidence fields (RECEIPTS-631: absent fields need no badge)", () => {
    // 'none' means the source receipt did not contain that field at all — the user
    // has nothing to review, so no badge should appear in the confidence map.
    const proposal = makeProposal({
      storeAddress: null,
      storeAddressConfidence: "none",
      storePhone: null,
      storePhoneConfidence: "none",
      receiptId: null,
      receiptIdConfidence: "none",
      storeNumber: null,
      storeNumberConfidence: "none",
      terminalId: null,
      terminalIdConfidence: "none",
    });

    const result = mapProposalToConfidenceMap(proposal);

    expect(result).toEqual({});
  });

  it("falls back to subtotalConfidence when first tax line's amount is 'none'", () => {
    // Regression for the bug-finder review of RECEIPTS-631: the `??` operator
    // does NOT fall through for the non-nullish string "none". Without an
    // explicit check, a tax line whose amount is absent would silently drop
    // the subtotal-based review badge — the user would never be prompted to
    // verify a low-confidence subtotal.
    const proposal = makeProposal({
      taxLines: [
        {
          label: "Tax",
          labelConfidence: "high",
          amount: null,
          amountConfidence: "none",
        },
      ],
      subtotalConfidence: "low",
    });

    const result = mapProposalToConfidenceMap(proposal);

    expect(result.taxAmount).toBe("low");
  });

  it("uses first tax line's amountConfidence when present and not 'none'", () => {
    const proposal = makeProposal({
      taxLines: [
        {
          label: "Tax",
          labelConfidence: "high",
          amount: 1.25,
          amountConfidence: "low",
        },
      ],
      subtotalConfidence: "high",
    });

    const result = mapProposalToConfidenceMap(proposal);

    expect(result.taxAmount).toBe("low");
  });

  it("flags low/medium confidence but never 'none' on per-payment fields", () => {
    const proposal = makeProposal({
      payments: [
        {
          method: "VISA",
          methodConfidence: "low",
          amount: 5,
          amountConfidence: "high",
          lastFour: null,
          lastFourConfidence: "none",
        },
      ],
    });

    const result = mapProposalToConfidenceMap(proposal);

    // method: "low" surfaces; amount/lastFour are dropped (high + none).
    expect(result.payments).toEqual([{ method: "low" }]);
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

describe("initialItemsAndConfidence", () => {
  function makeInitial(
    items: ScanInitialValues["items"],
  ): ScanInitialValues {
    return {
      header: {
        location: "",
        date: "",
        taxAmount: 0,
        storeAddress: "",
        storePhone: "",
      },
      metadata: { receiptId: "", storeNumber: "", terminalId: "" },
      payments: [],
      items,
    };
  }

  it("returns empty items + empty Map when initialValues is undefined", () => {
    const result = initialItemsAndConfidence(undefined, undefined);
    expect(result.items).toEqual([]);
    expect(result.itemConfidenceById.size).toBe(0);
  });

  it("returns empty items + empty Map when initialValues has no items", () => {
    const result = initialItemsAndConfidence(makeInitial([]), undefined);
    expect(result.items).toEqual([]);
    expect(result.itemConfidenceById.size).toBe(0);
  });

  it("assigns a unique generated id to each item", () => {
    const result = initialItemsAndConfidence(
      makeInitial([
        {
          receiptItemCode: "MILK",
          description: "Milk",
          pricingMode: "quantity",
          quantity: 1,
          unitPrice: 3.5,
          category: "",
          subcategory: "",
          taxCode: "",
        },
        {
          receiptItemCode: "BREAD",
          description: "Bread",
          pricingMode: "quantity",
          quantity: 2,
          unitPrice: 2.5,
          category: "",
          subcategory: "",
          taxCode: "",
        },
      ]),
      undefined,
    );

    expect(result.items).toHaveLength(2);
    expect(result.items[0].id).toBeTruthy();
    expect(result.items[1].id).toBeTruthy();
    expect(result.items[0].id).not.toBe(result.items[1].id);
  });

  it("pairs each item id with its corresponding confidence entry", () => {
    const result = initialItemsAndConfidence(
      makeInitial([
        {
          receiptItemCode: "MILK",
          description: "Milk",
          pricingMode: "quantity",
          quantity: 1,
          unitPrice: 3.5,
          category: "",
          subcategory: "",
          taxCode: "F",
        },
        {
          receiptItemCode: "BREAD",
          description: "Bread",
          pricingMode: "quantity",
          quantity: 1,
          unitPrice: 2,
          category: "",
          subcategory: "",
          taxCode: "",
        },
      ]),
      { items: [{ taxCode: "low" }, { taxCode: "medium" }] },
    );

    expect(result.itemConfidenceById.size).toBe(2);
    expect(result.itemConfidenceById.get(result.items[0].id)).toEqual({
      taxCode: "low",
    });
    expect(result.itemConfidenceById.get(result.items[1].id)).toEqual({
      taxCode: "medium",
    });
  });

  it("omits Map entries for items lacking a confidence record", () => {
    const result = initialItemsAndConfidence(
      makeInitial([
        {
          receiptItemCode: "MILK",
          description: "Milk",
          pricingMode: "quantity",
          quantity: 1,
          unitPrice: 3.5,
          category: "",
          subcategory: "",
          taxCode: "",
        },
      ]),
      {} as ReceiptConfidenceMap,
    );

    expect(result.items).toHaveLength(1);
    expect(result.itemConfidenceById.size).toBe(0);
  });
});

describe("initialPaymentsAndConfidence", () => {
  function makeInitial(
    payments: ScanInitialValues["payments"],
  ): ScanInitialValues {
    return {
      header: {
        location: "",
        date: "",
        taxAmount: 0,
        storeAddress: "",
        storePhone: "",
      },
      metadata: { receiptId: "", storeNumber: "", terminalId: "" },
      payments,
      items: [],
    };
  }

  it("returns empty payments + empty Map when initialValues is undefined", () => {
    const result = initialPaymentsAndConfidence(undefined, undefined);
    expect(result.payments).toEqual([]);
    expect(result.paymentConfidenceById.size).toBe(0);
  });

  it("preserves method/amount/lastFour fields and assigns ids", () => {
    const result = initialPaymentsAndConfidence(
      makeInitial([
        { method: "MASTERCARD", amount: 54.32, lastFour: "4538" },
        { method: "Cash", amount: 5, lastFour: "" },
      ]),
      undefined,
    );

    expect(result.payments).toHaveLength(2);
    expect(result.payments[0]).toMatchObject({
      method: "MASTERCARD",
      amount: 54.32,
      lastFour: "4538",
    });
    expect(result.payments[1]).toMatchObject({
      method: "Cash",
      amount: 5,
      lastFour: "",
    });
    expect(result.payments[0].id).toBeTruthy();
    expect(result.payments[0].id).not.toBe(result.payments[1].id);
  });

  it("pairs each payment id with its confidence entry", () => {
    const result = initialPaymentsAndConfidence(
      makeInitial([
        { method: "MASTERCARD", amount: 54.32, lastFour: "4538" },
        { method: "Cash", amount: 5, lastFour: "" },
      ]),
      {
        payments: [{ method: "low" }, { amount: "medium" }],
      },
    );

    expect(result.paymentConfidenceById.get(result.payments[0].id)).toEqual({
      method: "low",
    });
    expect(result.paymentConfidenceById.get(result.payments[1].id)).toEqual({
      amount: "medium",
    });
  });
});

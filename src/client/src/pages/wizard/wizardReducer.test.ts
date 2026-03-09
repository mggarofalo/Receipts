import { wizardReducer, INITIAL_STATE, type WizardState, type WizardAction } from "./wizardReducer";

describe("wizardReducer", () => {
  it("returns initial state for unknown action", () => {
    const state = wizardReducer(INITIAL_STATE, { type: "UNKNOWN" } as unknown as WizardAction);
    expect(state).toBe(INITIAL_STATE);
  });

  it("SET_STEP updates currentStep", () => {
    const result = wizardReducer(INITIAL_STATE, { type: "SET_STEP", step: 2 });
    expect(result.currentStep).toBe(2);
  });

  it("SET_RECEIPT updates receipt data", () => {
    const data = { location: "Walmart", date: "2024-01-15", taxAmount: 5.25 };
    const result = wizardReducer(INITIAL_STATE, { type: "SET_RECEIPT", data });
    expect(result.receipt).toEqual(data);
  });

  it("SET_TRANSACTIONS updates transactions", () => {
    const data = [
      { id: "1", accountId: "acct-1", amount: 50, date: "2024-01-15" },
    ];
    const result = wizardReducer(INITIAL_STATE, { type: "SET_TRANSACTIONS", data });
    expect(result.transactions).toEqual(data);
  });

  it("SET_ITEMS updates items", () => {
    const data = [
      {
        id: "1",
        receiptItemCode: "MILK",
        description: "Milk",
        pricingMode: "quantity" as const,
        quantity: 2,
        unitPrice: 3.99,
        category: "Food",
        subcategory: "Dairy",
      },
    ];
    const result = wizardReducer(INITIAL_STATE, { type: "SET_ITEMS", data });
    expect(result.items).toEqual(data);
  });

  it("MARK_STEP_COMPLETE adds step to completedSteps", () => {
    const result = wizardReducer(INITIAL_STATE, { type: "MARK_STEP_COMPLETE", step: 0 });
    expect(result.completedSteps.has(0)).toBe(true);
  });

  it("MARK_STEP_COMPLETE preserves previously completed steps", () => {
    const state: WizardState = {
      ...INITIAL_STATE,
      completedSteps: new Set([0]),
    };
    const result = wizardReducer(state, { type: "MARK_STEP_COMPLETE", step: 1 });
    expect(result.completedSteps.has(0)).toBe(true);
    expect(result.completedSteps.has(1)).toBe(true);
  });

  it("RESET returns to initial state", () => {
    const state: WizardState = {
      currentStep: 3,
      receipt: { location: "Target", date: "2024-01-20", taxAmount: 2 },
      transactions: [{ id: "1", accountId: "a", amount: 10, date: "2024-01-20" }],
      items: [],
      completedSteps: new Set([0, 1, 2]),
    };
    const result = wizardReducer(state, { type: "RESET" });
    expect(result.currentStep).toBe(0);
    expect(result.receipt).toEqual({ location: "", date: "", taxAmount: 0 });
    expect(result.transactions).toEqual([]);
    expect(result.items).toEqual([]);
    expect(result.completedSteps.size).toBe(0);
  });
});

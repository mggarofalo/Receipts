import { renderHook, act } from "@testing-library/react";
import { useWizard } from "./useWizard";

describe("useWizard", () => {
  it("starts at step 0 with empty state", () => {
    const { result } = renderHook(() => useWizard());
    expect(result.current.state.currentStep).toBe(0);
    expect(result.current.isFirstStep).toBe(true);
    expect(result.current.isLastStep).toBe(false);
  });

  it("goNext marks step complete and advances", () => {
    const { result } = renderHook(() => useWizard());
    act(() => result.current.goNext());
    expect(result.current.state.currentStep).toBe(1);
    expect(result.current.state.completedSteps.has(0)).toBe(true);
  });

  it("goBack decrements the step", () => {
    const { result } = renderHook(() => useWizard());
    act(() => result.current.goNext());
    act(() => result.current.goBack());
    expect(result.current.state.currentStep).toBe(0);
  });

  it("goBack does not go below 0", () => {
    const { result } = renderHook(() => useWizard());
    act(() => result.current.goBack());
    expect(result.current.state.currentStep).toBe(0);
  });

  it("goToStep navigates directly", () => {
    const { result } = renderHook(() => useWizard());
    act(() => result.current.goToStep(3));
    expect(result.current.state.currentStep).toBe(3);
    expect(result.current.isLastStep).toBe(true);
  });

  it("setReceipt updates receipt data", () => {
    const { result } = renderHook(() => useWizard());
    act(() =>
      result.current.setReceipt({
        location: "Costco",
        date: "2024-06-01",
        taxAmount: 4.5,
      }),
    );
    expect(result.current.state.receipt.location).toBe("Costco");
    expect(result.current.state.receipt.taxAmount).toBe(4.5);
  });

  it("setTransactions updates transactions", () => {
    const { result } = renderHook(() => useWizard());
    const txns = [{ id: "1", accountId: "a", amount: 10, date: "2024-06-01" }];
    act(() => result.current.setTransactions(txns));
    expect(result.current.state.transactions).toEqual(txns);
  });

  it("setItems updates items", () => {
    const { result } = renderHook(() => useWizard());
    const items = [
      {
        id: "1",
        receiptItemCode: "X",
        description: "Test",
        pricingMode: "quantity" as const,
        quantity: 1,
        unitPrice: 5,
        category: "Food",
        subcategory: "",
      },
    ];
    act(() => result.current.setItems(items));
    expect(result.current.state.items).toEqual(items);
  });

  it("reset clears all state back to initial", () => {
    const { result } = renderHook(() => useWizard());
    act(() => {
      result.current.setReceipt({
        location: "Target",
        date: "2024-06-01",
        taxAmount: 2,
      });
      result.current.goNext();
    });
    act(() => result.current.reset());
    expect(result.current.state.currentStep).toBe(0);
    expect(result.current.state.receipt.location).toBe("");
  });

  it("canGoToStep returns true for step 0 always", () => {
    const { result } = renderHook(() => useWizard());
    expect(result.current.canGoToStep(0)).toBe(true);
  });

  it("canGoToStep returns false for step 1 if step 0 not complete", () => {
    const { result } = renderHook(() => useWizard());
    expect(result.current.canGoToStep(1)).toBe(false);
  });

  it("canGoToStep returns true for step 1 if step 0 is complete", () => {
    const { result } = renderHook(() => useWizard());
    act(() => result.current.goNext());
    expect(result.current.canGoToStep(1)).toBe(true);
  });

  it("canGoToStep requires all prior steps completed", () => {
    const { result } = renderHook(() => useWizard());
    act(() => result.current.goNext()); // complete step 0, now at 1
    act(() => result.current.goNext()); // complete step 1, now at 2
    expect(result.current.canGoToStep(2)).toBe(true);
    expect(result.current.canGoToStep(3)).toBe(false);
  });
});

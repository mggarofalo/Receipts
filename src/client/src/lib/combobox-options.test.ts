import { accountToOption, receiptToOption } from "./combobox-options";

describe("accountToOption", () => {
  it("maps account to ComboboxOption", () => {
    const result = accountToOption({
      id: "abc-123",
      name: "Checking",
      accountCode: "1000",
    });
    expect(result).toEqual({
      value: "abc-123",
      label: "Checking",
      sublabel: "1000",
    });
  });
});

describe("receiptToOption", () => {
  it("uses description as label when present", () => {
    const result = receiptToOption({
      id: "r-1",
      description: "Groceries",
      location: "Walmart",
      date: "2024-01-15",
    });
    expect(result).toEqual({
      value: "r-1",
      label: "Groceries",
      sublabel: "Walmart — 2024-01-15",
    });
  });

  it("falls back to location when description is null", () => {
    const result = receiptToOption({
      id: "r-2",
      description: null,
      location: "Target",
      date: "2024-02-20",
    });
    expect(result.label).toBe("Target");
  });

  it("falls back to location when description is empty", () => {
    const result = receiptToOption({
      id: "r-3",
      description: "",
      location: "Costco",
      date: "2024-03-01",
    });
    expect(result.label).toBe("Costco");
  });
});

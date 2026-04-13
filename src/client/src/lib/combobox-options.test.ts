import { cardToOption, receiptToOption } from "./combobox-options";

describe("cardToOption", () => {
  it("maps card to ComboboxOption", () => {
    const result = cardToOption({
      id: "abc-123",
      name: "Checking",
      cardCode: "1000",
    });
    expect(result).toEqual({
      value: "abc-123",
      label: "Checking",
      sublabel: "1000",
    });
  });
});

describe("receiptToOption", () => {
  it("uses location as label", () => {
    const result = receiptToOption({
      id: "r-1",
      location: "Walmart",
      date: "2024-01-15",
    });
    expect(result).toEqual({
      value: "r-1",
      label: "Walmart",
      sublabel: "Walmart — 2024-01-15",
    });
  });
});

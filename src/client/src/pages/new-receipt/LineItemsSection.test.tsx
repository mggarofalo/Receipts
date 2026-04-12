import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult, mockMutationResult } from "@/test/mock-hooks";
import "@/test/setup-combobox-polyfills";
import { LineItemsSection, type ReceiptLineItem } from "./LineItemsSection";

vi.mock("@/hooks/useCategories", () => ({
  useCategories: vi.fn(() =>
    mockQueryResult({
      data: [
        { id: "cat-1", name: "Food" },
        { id: "cat-2", name: "Household" },
      ],
      isLoading: false,
      isSuccess: true,
    }),
  ),
}));

vi.mock("@/hooks/useSubcategories", () => ({
  useSubcategoriesByCategoryId: vi.fn(() =>
    mockQueryResult({
      data: [],
      isLoading: false,
      isSuccess: true,
    }),
  ),
  useCreateSubcategory: vi.fn(() => mockMutationResult()),
}));

vi.mock("@/hooks/useSimilarItems", () => ({
  useSimilarItems: vi.fn(() =>
    mockQueryResult({
      data: [],
      isFetching: false,
    }),
  ),
  useCategoryRecommendations: vi.fn(() =>
    mockQueryResult({
      data: [],
    }),
  ),
}));

vi.mock("@/hooks/useReceiptItemSuggestions", () => ({
  useReceiptItemSuggestions: vi.fn(() =>
    mockQueryResult({
      data: undefined,
      isFetching: false,
      isSuccess: false,
    }),
  ),
}));

describe("LineItemsSection", () => {
  const defaultProps = {
    items: [] as ReceiptLineItem[],
    onChange: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders the card title", () => {
    renderWithProviders(<LineItemsSection {...defaultProps} />);
    expect(screen.getByText("Line Items")).toBeInTheDocument();
  });

  it("renders the form fields", () => {
    renderWithProviders(<LineItemsSection {...defaultProps} />);
    expect(screen.getByPlaceholderText("Item description")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("e.g. MILK-GAL")).toBeInTheDocument();
  });

  it("renders Add Item button", () => {
    renderWithProviders(<LineItemsSection {...defaultProps} />);
    expect(
      screen.getByRole("button", { name: /add item/i }),
    ).toBeInTheDocument();
  });

  it("displays subtotal", () => {
    renderWithProviders(<LineItemsSection {...defaultProps} />);
    expect(screen.getByText("Subtotal: $0.00")).toBeInTheDocument();
  });

  it("renders existing items", () => {
    const items: ReceiptLineItem[] = [
      {
        id: "1",
        receiptItemCode: "",
        description: "Milk",
        pricingMode: "quantity",
        quantity: 2,
        unitPrice: 3.5,
        category: "Food",
        subcategory: "",
      },
    ];
    renderWithProviders(
      <LineItemsSection {...defaultProps} items={items} />,
    );
    expect(screen.getByText("Milk")).toBeInTheDocument();
    expect(screen.getByText("$3.50")).toBeInTheDocument();
    expect(screen.getByText("$7.00")).toBeInTheDocument(); // line total
    expect(screen.getByText("Food")).toBeInTheDocument();
  });

  it("displays subtotal with existing items", () => {
    const items: ReceiptLineItem[] = [
      {
        id: "1",
        receiptItemCode: "",
        description: "Milk",
        pricingMode: "quantity",
        quantity: 2,
        unitPrice: 3.5,
        category: "Food",
        subcategory: "",
      },
      {
        id: "2",
        receiptItemCode: "",
        description: "Bread",
        pricingMode: "flat",
        quantity: 1,
        unitPrice: 4.0,
        category: "Food",
        subcategory: "",
      },
    ];
    renderWithProviders(
      <LineItemsSection {...defaultProps} items={items} />,
    );
    expect(screen.getByText("Subtotal: $11.00")).toBeInTheDocument();
  });

  it("floors per-item totals when computing subtotal (half-cent rounding)", () => {
    // 3 x $1.005 = $3.015 → Math.floor(3.015 * 100) / 100 = $3.01
    // Without floor, naive multiply gives $3.015 which formats as $3.02
    const items: ReceiptLineItem[] = [
      {
        id: "1",
        receiptItemCode: "",
        description: "Fractional item",
        pricingMode: "quantity",
        quantity: 3,
        unitPrice: 1.005,
        category: "Food",
        subcategory: "",
      },
    ];
    renderWithProviders(
      <LineItemsSection {...defaultProps} items={items} />,
    );
    // Floor-rounded: 3 x 1.005 = 3.015 → floor → $3.01
    expect(screen.getByText("Subtotal: $3.01")).toBeInTheDocument();
  });

  it("calls onChange when an item is removed", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    const items: ReceiptLineItem[] = [
      {
        id: "1",
        receiptItemCode: "",
        description: "Milk",
        pricingMode: "quantity",
        quantity: 1,
        unitPrice: 3.5,
        category: "Food",
        subcategory: "",
      },
    ];
    renderWithProviders(
      <LineItemsSection items={items} onChange={onChange} />,
    );

    await user.click(screen.getByRole("button", { name: /remove/i }));
    expect(onChange).toHaveBeenCalledWith([]);
  });

  it("shows category/subcategory for items", () => {
    const items: ReceiptLineItem[] = [
      {
        id: "1",
        receiptItemCode: "",
        description: "Soap",
        pricingMode: "flat",
        quantity: 1,
        unitPrice: 5,
        category: "Household",
        subcategory: "Cleaning",
      },
    ];
    renderWithProviders(
      <LineItemsSection {...defaultProps} items={items} />,
    );
    expect(screen.getByText("Household / Cleaning")).toBeInTheDocument();
  });

  // --- Inline editing tests ---

  it("shows edit button for each item row", () => {
    const items: ReceiptLineItem[] = [
      {
        id: "1",
        receiptItemCode: "",
        description: "Milk",
        pricingMode: "quantity",
        quantity: 2,
        unitPrice: 3.5,
        category: "Food",
        subcategory: "",
      },
    ];
    renderWithProviders(
      <LineItemsSection {...defaultProps} items={items} />,
    );
    expect(screen.getByRole("button", { name: /edit/i })).toBeInTheDocument();
  });

  it("enters edit mode when edit button is clicked", async () => {
    const user = userEvent.setup();
    const items: ReceiptLineItem[] = [
      {
        id: "1",
        receiptItemCode: "",
        description: "Milk",
        pricingMode: "quantity",
        quantity: 2,
        unitPrice: 3.5,
        category: "Food",
        subcategory: "",
      },
    ];
    renderWithProviders(
      <LineItemsSection {...defaultProps} items={items} />,
    );

    await user.click(screen.getByRole("button", { name: /edit/i }));

    expect(screen.getByLabelText("Edit description")).toBeInTheDocument();
    expect(screen.getByLabelText("Edit quantity")).toBeInTheDocument();
    expect(screen.getByLabelText("Edit unit price")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /save/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancel/i })).toBeInTheDocument();
  });

  it("saves edited values and calls onChange", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    const items: ReceiptLineItem[] = [
      {
        id: "1",
        receiptItemCode: "",
        description: "Milk",
        pricingMode: "quantity",
        quantity: 2,
        unitPrice: 3.5,
        category: "Food",
        subcategory: "",
      },
    ];
    renderWithProviders(
      <LineItemsSection items={items} onChange={onChange} />,
    );

    await user.click(screen.getByRole("button", { name: /edit/i }));

    const descInput = screen.getByLabelText("Edit description");
    await user.clear(descInput);
    await user.type(descInput, "Whole Milk");

    await user.click(screen.getByRole("button", { name: /save/i }));

    expect(onChange).toHaveBeenCalledWith([
      expect.objectContaining({ description: "Whole Milk" }),
    ]);
  });

  it("cancels editing without calling onChange", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    const items: ReceiptLineItem[] = [
      {
        id: "1",
        receiptItemCode: "",
        description: "Milk",
        pricingMode: "quantity",
        quantity: 2,
        unitPrice: 3.5,
        category: "Food",
        subcategory: "",
      },
    ];
    renderWithProviders(
      <LineItemsSection items={items} onChange={onChange} />,
    );

    await user.click(screen.getByRole("button", { name: /edit/i }));

    const descInput = screen.getByLabelText("Edit description");
    await user.clear(descInput);
    await user.type(descInput, "Changed");

    await user.click(screen.getByRole("button", { name: /cancel/i }));

    // onChange should not have been called for editing (only for remove)
    expect(onChange).not.toHaveBeenCalled();
    expect(screen.getByText("Milk")).toBeInTheDocument();
  });

  it("does not save when description is empty", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    const items: ReceiptLineItem[] = [
      {
        id: "1",
        receiptItemCode: "",
        description: "Milk",
        pricingMode: "quantity",
        quantity: 2,
        unitPrice: 3.5,
        category: "Food",
        subcategory: "",
      },
    ];
    renderWithProviders(
      <LineItemsSection items={items} onChange={onChange} />,
    );

    await user.click(screen.getByRole("button", { name: /edit/i }));

    const descInput = screen.getByLabelText("Edit description");
    await user.clear(descInput);

    await user.click(screen.getByRole("button", { name: /save/i }));

    // Should still be in edit mode (save rejected)
    expect(screen.getByLabelText("Edit description")).toBeInTheDocument();
    expect(onChange).not.toHaveBeenCalled();
  });

  it("disables quantity input in edit mode for flat pricing items", async () => {
    const user = userEvent.setup();
    const items: ReceiptLineItem[] = [
      {
        id: "1",
        receiptItemCode: "",
        description: "Service Fee",
        pricingMode: "flat",
        quantity: 1,
        unitPrice: 25,
        category: "Food",
        subcategory: "",
      },
    ];
    renderWithProviders(
      <LineItemsSection {...defaultProps} items={items} />,
    );

    await user.click(screen.getByRole("button", { name: /edit/i }));

    expect(screen.getByLabelText("Edit quantity")).toBeDisabled();
  });
});

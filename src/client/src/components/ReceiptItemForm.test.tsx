import "@/test/setup-combobox-polyfills";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ReceiptItemForm } from "./ReceiptItemForm";

vi.mock("@/hooks/useFormShortcuts", () => ({
  useFormShortcuts: vi.fn(),
}));

vi.mock("@/hooks/useEnumMetadata", () => ({
  useEnumMetadata: vi.fn(() => ({
    adjustmentTypes: [],
    authEventTypes: [],
    pricingModes: [{ value: "quantity", label: "Quantity" }, { value: "flat", label: "Flat" }],
    auditActions: [],
    entityTypes: [],
    adjustmentTypeLabels: {},
    authEventLabels: {},
    pricingModeLabels: { quantity: "Quantity", flat: "Flat" },
    auditActionLabels: {},
    entityTypeLabels: {},
    isLoading: false,
  })),
}));

vi.mock("@/hooks/useReceipts", () => ({
  useReceipts: vi.fn(() => ({
    data: [
      { id: "r-1", location: "Walmart", date: "2024-01-15" },
    ],
    total: 1,
    isLoading: false,
  })),
}));

vi.mock("@/hooks/useCategories", () => ({
  useCategories: vi.fn(() => ({
    data: [
      { id: "cat-1", name: "Groceries" },
      { id: "cat-2", name: "Electronics" },
    ],
    total: 2,
  })),
}));

vi.mock("@/hooks/useSubcategories", () => ({
  useSubcategoriesByCategoryId: vi.fn(() => ({
    data: [
      { id: "sub-1", name: "Dairy" },
      { id: "sub-2", name: "Bakery" },
    ],
    total: 2,
  })),
  useCreateSubcategory: vi.fn(() => ({
    mutateAsync: vi.fn(),
  })),
}));

vi.mock("@/hooks/useItemTemplates", () => ({
  useItemTemplates: vi.fn(() => ({
    data: [
      {
        id: "tmpl-1",
        name: "Milk",
        defaultCategory: "Groceries",
        defaultSubcategory: "Dairy",
        defaultUnitPrice: 3.99,
        defaultItemCode: "MLK-001",
      },
    ],
    total: 1,
  })),
}));

vi.mock("@/lib/combobox-options", () => ({
  receiptToOption: vi.fn((r: { id: string; location: string; date: string }) => ({
    value: r.id,
    label: r.location,
    sublabel: `${r.location} — ${r.date}`,
  })),
}));

vi.mock("@/lib/format", async (importOriginal) => {
  const actual = await importOriginal<typeof import("@/lib/format")>();
  return {
    ...actual,
    formatCurrency: vi.fn((amount: number) => `$${amount.toFixed(2)}`),
  };
});

describe("ReceiptItemForm", () => {
  const defaultProps = {
    mode: "create" as const,
    onSubmit: vi.fn(),
    onCancel: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  it("renders in create mode with correct submit button text and all field labels", () => {
    render(<ReceiptItemForm {...defaultProps} />);

    expect(screen.getByText("Receipt")).toBeInTheDocument();
    expect(screen.getByText("Item Code")).toBeInTheDocument();
    expect(screen.getByText("Description")).toBeInTheDocument();
    expect(screen.getByText("Pricing Mode")).toBeInTheDocument();
    expect(screen.getByLabelText("Quantity")).toBeInTheDocument();
    expect(screen.getByLabelText("Unit Price")).toBeInTheDocument();
    expect(screen.getByText("Category")).toBeInTheDocument();
    expect(screen.getByText("Subcategory")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /create item/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancel/i })).toBeInTheDocument();
  });

  it("renders in edit mode with pre-populated fields and correct submit button text", () => {
    render(
      <ReceiptItemForm
        {...defaultProps}
        mode="edit"
        defaultValues={{
          receiptId: "r-1",
          receiptItemCode: "ITM-001",
          description: "Whole Milk",
          pricingMode: "quantity",
          quantity: 2,
          unitPrice: 3.99,
          category: "Groceries",
          subcategory: "Dairy",
        }}
      />,
    );

    // Item Code is a Combobox; it shows the value as text content
    expect(screen.getByLabelText("Item Code")).toHaveTextContent("ITM-001");
    expect(screen.getByRole("button", { name: /update item/i })).toBeInTheDocument();
  });

  it("shows validation errors when required fields are empty", async () => {
    const user = userEvent.setup();
    render(<ReceiptItemForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /create item/i }));

    await waitFor(() => {
      expect(screen.getByText("Receipt is required")).toBeInTheDocument();
      expect(screen.getByText("Item code is required")).toBeInTheDocument();
      expect(screen.getByText("Description is required")).toBeInTheDocument();
      expect(screen.getByText("Category is required")).toBeInTheDocument();
      expect(screen.getByText("Subcategory is required")).toBeInTheDocument();
    });
    expect(defaultProps.onSubmit).not.toHaveBeenCalled();
  });

  it("calls onCancel when cancel button is clicked", async () => {
    const user = userEvent.setup();
    render(<ReceiptItemForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /cancel/i }));

    expect(defaultProps.onCancel).toHaveBeenCalledTimes(1);
  });

  it("disables submit button and shows spinner when isSubmitting is true", () => {
    render(<ReceiptItemForm {...defaultProps} isSubmitting={true} />);

    const submitButton = screen.getByRole("button", { name: /saving/i });
    expect(submitButton).toBeDisabled();
  });

  it("displays computed total based on quantity and unit price", () => {
    render(
      <ReceiptItemForm
        {...defaultProps}
        defaultValues={{
          receiptId: "r-1",
          receiptItemCode: "ITM-001",
          description: "Milk",
          pricingMode: "quantity",
          quantity: 3,
          unitPrice: 2.50,
          category: "Groceries",
          subcategory: "Dairy",
        }}
      />,
    );

    expect(screen.getByText(/total/i)).toBeInTheDocument();
  });

  it("calls onSubmit with correct data when all fields are valid", async () => {
    const user = userEvent.setup();
    render(
      <ReceiptItemForm
        {...defaultProps}
        defaultValues={{
          receiptId: "r-1",
          receiptItemCode: "ITM-001",
          description: "Milk",
          pricingMode: "quantity",
          quantity: 1,
          unitPrice: 3.99,
          category: "Groceries",
          subcategory: "Dairy",
        }}
      />,
    );

    await user.click(screen.getByRole("button", { name: /create item/i }));

    await waitFor(() => {
      expect(defaultProps.onSubmit).toHaveBeenCalledWith(
        expect.objectContaining({
          receiptId: "r-1",
          receiptItemCode: "ITM-001",
          description: "Milk",
          category: "Groceries",
          subcategory: "Dairy",
        }),
      );
    });
  });

  it("defaults quantity to 1 and pricing mode to 'quantity'", () => {
    render(<ReceiptItemForm {...defaultProps} />);

    expect(screen.getByLabelText("Quantity")).toBeInTheDocument();
    expect(screen.getByLabelText("Unit Price")).toBeInTheDocument();
  });

  it("shows recent description history and item template groups in description autocomplete", async () => {
    const user = userEvent.setup();
    localStorage.setItem(
      "receipts:item-description-history",
      JSON.stringify(["Whole Milk", "Bread"]),
    );

    render(<ReceiptItemForm {...defaultProps} />);

    const descriptionInput = screen.getByLabelText("Description");
    await user.click(descriptionInput);

    // History entries should appear under "Recent Descriptions"
    await waitFor(() => {
      expect(screen.getByText("Recent Descriptions")).toBeInTheDocument();
      expect(screen.getByText("Whole Milk")).toBeInTheDocument();
      expect(screen.getByText("Bread")).toBeInTheDocument();
    });
  });

  it("shows item template suggestions when typing a matching description", async () => {
    const user = userEvent.setup();
    render(<ReceiptItemForm {...defaultProps} />);

    const descriptionInput = screen.getByLabelText("Description");
    await user.type(descriptionInput, "Milk");

    // Fuzzy-matched template should appear under "Item Templates"
    await waitFor(() => {
      expect(screen.getByText("Item Templates")).toBeInTheDocument();
    });
  });

  it("selects a description history entry and populates the field", async () => {
    const user = userEvent.setup();
    localStorage.setItem(
      "receipts:item-description-history",
      JSON.stringify(["Whole Milk"]),
    );

    render(<ReceiptItemForm {...defaultProps} />);

    const descriptionInput = screen.getByLabelText("Description");
    await user.click(descriptionInput);

    await waitFor(() => {
      expect(screen.getByText("Whole Milk")).toBeInTheDocument();
    });

    // Select the history item (rendered as a CommandItem with value prefixed "history: ")
    await user.click(screen.getByText("Whole Milk"));

    expect(descriptionInput).toHaveValue("Whole Milk");
  });

  it("persists description and item code to history on submit", async () => {
    const user = userEvent.setup();
    render(
      <ReceiptItemForm
        {...defaultProps}
        defaultValues={{
          receiptId: "r-1",
          receiptItemCode: "ITM-001",
          description: "Bananas",
          pricingMode: "quantity",
          quantity: 1,
          unitPrice: 1.29,
          category: "Groceries",
          subcategory: "Dairy",
        }}
      />,
    );

    await user.click(screen.getByRole("button", { name: /create item/i }));

    await waitFor(() => {
      expect(defaultProps.onSubmit).toHaveBeenCalled();
    });

    const storedDescriptions = JSON.parse(
      localStorage.getItem("receipts:item-description-history") ?? "[]",
    ) as string[];
    expect(storedDescriptions).toContain("Bananas");

    const storedItemCodes = JSON.parse(
      localStorage.getItem("receipts:item-code-history") ?? "[]",
    ) as string[];
    expect(storedItemCodes).toContain("ITM-001");
  });

  it("shows saved item codes in the item code Combobox dropdown", async () => {
    const user = userEvent.setup();
    localStorage.setItem(
      "receipts:item-code-history",
      JSON.stringify(["ITM-001", "ITM-002"]),
    );

    render(<ReceiptItemForm {...defaultProps} />);

    // The Item Code field is a Combobox; click its trigger to open
    const itemCodeCombobox = screen.getByLabelText("Item Code");
    await user.click(itemCodeCombobox);

    await waitFor(() => {
      expect(screen.getByText("ITM-001")).toBeInTheDocument();
      expect(screen.getByText("ITM-002")).toBeInTheDocument();
    });
  });

  it("does not allow custom category values (no 'Use' option for arbitrary text)", async () => {
    const user = userEvent.setup();
    render(<ReceiptItemForm {...defaultProps} />);

    // Open the category combobox
    const categoryCombobox = screen.getByLabelText("Category");
    await user.click(categoryCombobox);

    // Type a non-existent category (like a store name)
    const searchInput = screen.getByPlaceholderText("Search categories...");
    await user.type(searchInput, "Costco");

    // Should NOT show a "Use" button for arbitrary text
    await waitFor(() => {
      expect(screen.queryByText(/use.*costco/i)).not.toBeInTheDocument();
    });
  });
});

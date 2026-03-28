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
});

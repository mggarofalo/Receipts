import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult, mockMutationResult } from "@/test/mock-hooks";
import { Step3Items } from "./Step3Items";

vi.mock("@/hooks/useCategories", () => ({
  useCategories: vi.fn(() =>
    mockQueryResult({
      data: {
        data: [
          { id: "cat-1", name: "Food" },
          { id: "cat-2", name: "Transport" },
        ],
      },
      isLoading: false,
      isSuccess: true,
    }),
  ),
}));

vi.mock("@/hooks/useSubcategories", () => ({
  useSubcategoriesByCategoryId: vi.fn(() =>
    mockQueryResult({
      data: { data: [] },
      isLoading: false,
      isSuccess: true,
    }),
  ),
  useCreateSubcategory: vi.fn(() => mockMutationResult()),
}));

vi.mock("@/hooks/useSimilarItems", () => ({
  useSimilarItems: vi.fn(() =>
    mockQueryResult({
      data: undefined,
      isFetching: false,
      isSuccess: false,
    }),
  ),
  useCategoryRecommendations: vi.fn(() =>
    mockQueryResult({
      data: undefined,
      isSuccess: false,
    }),
  ),
}));

describe("Step3Items", () => {
  const defaultProps = {
    data: [],
    taxAmount: 5,
    transactionTotal: 50,
    onNext: vi.fn(),
    onBack: vi.fn(),
  };

  it("renders the card title", () => {
    renderWithProviders(<Step3Items {...defaultProps} />);
    expect(screen.getByText("Line Items")).toBeInTheDocument();
  });

  it("renders the form fields", () => {
    renderWithProviders(<Step3Items {...defaultProps} />);
    expect(screen.getByText("Description")).toBeInTheDocument();
    // "Quantity" appears as both a label and a select option; verify at least one exists
    expect(screen.getAllByText("Quantity").length).toBeGreaterThanOrEqual(1);
  });

  it("renders Back and Next buttons", () => {
    renderWithProviders(<Step3Items {...defaultProps} />);
    expect(screen.getByRole("button", { name: /back/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /next/i })).toBeInTheDocument();
  });

  it("Next button is disabled when there are no items", () => {
    renderWithProviders(<Step3Items {...defaultProps} />);
    expect(screen.getByRole("button", { name: /next/i })).toBeDisabled();
  });

  it("renders Add Item button", () => {
    renderWithProviders(<Step3Items {...defaultProps} />);
    expect(
      screen.getByRole("button", { name: /add item/i }),
    ).toBeInTheDocument();
  });

  it("shows balance status", () => {
    renderWithProviders(<Step3Items {...defaultProps} />);
    // With 0 items subtotal = 0, tax = 5, expected = 5, txn total = 50 => Unbalanced
    expect(screen.getByText(/unbalanced/i)).toBeInTheDocument();
  });

  it("calls onBack when Back is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const onBack = vi.fn();
    renderWithProviders(<Step3Items {...defaultProps} onBack={onBack} />);
    await user.click(screen.getByRole("button", { name: /back/i }));
    expect(onBack).toHaveBeenCalled();
  });

  it("renders existing items", () => {
    const items = [
      {
        id: "1",
        receiptItemCode: "MILK",
        description: "Whole Milk",
        pricingMode: "quantity" as const,
        quantity: 2,
        unitPrice: 3.99,
        category: "Food",
        subcategory: "Dairy",
      },
    ];
    renderWithProviders(<Step3Items {...defaultProps} data={items} />);
    expect(screen.getByText("Whole Milk")).toBeInTheDocument();
    expect(screen.getByText("Food / Dairy")).toBeInTheDocument();
  });
});

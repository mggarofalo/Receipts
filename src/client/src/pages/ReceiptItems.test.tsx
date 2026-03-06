import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult, mockMutationResult } from "@/test/mock-hooks";
import ReceiptItems from "./ReceiptItems";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useReceiptItems", () => ({
  useReceiptItems: vi.fn(() => ({ data: { data: [], total: 0, offset: 0, limit: 50 }, isLoading: false })),
  useReceiptItemsByReceiptId: vi.fn(() => ({ data: { data: [], total: 0, offset: 0, limit: 200 }, isLoading: false })),
  useCreateReceiptItem: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useUpdateReceiptItem: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useDeleteReceiptItems: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

vi.mock("@/hooks/useFuzzySearch", () => ({
  useFuzzySearch: vi.fn(() => ({
    search: "",
    setSearch: vi.fn(),
    results: [],
    totalCount: 0,
    isSearching: false,
    clearSearch: vi.fn(),
  })),
}));

vi.mock("@/hooks/useSavedFilters", () => ({
  useSavedFilters: vi.fn(() => ({
    filters: [],
    save: vi.fn(),
    remove: vi.fn(),
  })),
}));

vi.mock("@/hooks/useServerPagination", () => ({
  useServerPagination: vi.fn(() => ({
    offset: 0,
    limit: 25,
    currentPage: 1,
    pageSize: 25,
    totalPages: vi.fn(() => 1),
    setPage: vi.fn(),
    setPageSize: vi.fn(),
  })),
}));

vi.mock("@/hooks/useListKeyboardNav", () => ({
  useListKeyboardNav: vi.fn(() => ({
    focusedId: null,
    setFocusedIndex: vi.fn(),
    tableRef: { current: null },
  })),
}));

// Mocks needed by ReceiptItemForm (rendered inside dialogs)
vi.mock("@/hooks/useReceipts", () => ({
  useReceipts: vi.fn(() => ({ data: [], isLoading: false })),
}));

vi.mock("@/hooks/useCategories", () => ({
  useCategories: vi.fn(() => ({ data: [], isLoading: false })),
}));

vi.mock("@/hooks/useSubcategories", () => ({
  useSubcategories: vi.fn(() => ({ data: [], isLoading: false })),
  useSubcategoriesByCategoryId: vi.fn(() => ({ data: [], isLoading: false })),
  useCreateSubcategory: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

vi.mock("@/hooks/useItemTemplates", () => ({
  useItemTemplates: vi.fn(() => ({ data: [], isLoading: false })),
}));

vi.mock("@/hooks/usePagination", () => ({
  usePagination: vi.fn(() => ({
    paginatedItems: [],
    currentPage: 1,
    pageSize: 10,
    totalItems: 0,
    totalPages: 1,
    setPage: vi.fn(),
    setPageSize: vi.fn(),
  })),
}));

describe("ReceiptItems", () => {
  it("renders the page heading", () => {
    renderWithProviders(<ReceiptItems />);
    expect(
      screen.getByRole("heading", { name: /receipt items/i }),
    ).toBeInTheDocument();
  });

  it("renders loading skeleton when data is loading", async () => {
    const { useReceiptItems } = await import("@/hooks/useReceiptItems");
    vi.mocked(useReceiptItems).mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: true,
    }));

    const { container } = renderWithProviders(<ReceiptItems />);
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
  });

  it("renders empty state when no receipt items exist", async () => {
    const { useReceiptItems } = await import("@/hooks/useReceiptItems");
    vi.mocked(useReceiptItems).mockReturnValue(mockQueryResult({
      data: { data: [], total: 0, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<ReceiptItems />);
    expect(
      screen.getByText(/no receipt items yet/i),
    ).toBeInTheDocument();
  });

  it("renders the New Item button", () => {
    renderWithProviders(<ReceiptItems />);
    expect(
      screen.getByRole("button", { name: /new item/i }),
    ).toBeInTheDocument();
  });

  it("renders the search input", () => {
    renderWithProviders(<ReceiptItems />);
    expect(
      screen.getByPlaceholderText(/search items/i),
    ).toBeInTheDocument();
  });

  it("renders table with receipt items when data exists", async () => {
    const items = [
      { id: "ri1", receiptId: "r1", receiptItemCode: "RI-001", description: "Milk", quantity: 2, unitPrice: 3.99, category: "Groceries", subcategory: "Dairy", pricingMode: "quantity" as const },
    ];

    const { useFuzzySearch } = await import("@/hooks/useFuzzySearch");
    vi.mocked(useFuzzySearch).mockReturnValue(mockQueryResult({
      search: "",
      setSearch: vi.fn(),
      results: items.map((item) => ({ item, matches: [], score: 0, refIndex: 0 })),
      totalCount: items.length,
      isSearching: false,
      clearSearch: vi.fn(),
    }));

    const { useReceiptItems } = await import("@/hooks/useReceiptItems");
    vi.mocked(useReceiptItems).mockReturnValue(mockQueryResult({
      data: { data: items, total: items.length, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<ReceiptItems />);
    expect(screen.getByText("Milk")).toBeInTheDocument();
    expect(screen.getByText("RI-001")).toBeInTheDocument();
    expect(screen.getByText("Groceries")).toBeInTheDocument();
    expect(screen.getByText("Dairy")).toBeInTheDocument();
  });

  it("renders NoResults when search returns no matches", async () => {
    const { useReceiptItems } = await import("@/hooks/useReceiptItems");
    vi.mocked(useReceiptItems).mockReturnValue(mockQueryResult({
      data: [{ id: "ri1", receiptId: "r1", receiptItemCode: "RI-001", description: "Milk", quantity: 2, unitPrice: 3.99, category: "Groceries", subcategory: "Dairy", pricingMode: "quantity" }],
      isLoading: false,
    }));

    const { useFuzzySearch } = await import("@/hooks/useFuzzySearch");
    vi.mocked(useFuzzySearch).mockReturnValue(mockQueryResult({
      search: "xyz",
      setSearch: vi.fn(),
      results: [],
      totalCount: 0,
      isSearching: false,
      clearSearch: vi.fn(),
    }));

    renderWithProviders(<ReceiptItems />);
    expect(screen.getByText(/try fewer keywords/i)).toBeInTheDocument();
  });

  it("opens edit dialog when Edit button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "ri1", receiptId: "r1", receiptItemCode: "RI-001", description: "Milk", quantity: 2, unitPrice: 3.99, category: "Groceries", subcategory: "Dairy", pricingMode: "quantity" as const },
    ];

    const { useFuzzySearch } = await import("@/hooks/useFuzzySearch");
    vi.mocked(useFuzzySearch).mockReturnValue(mockQueryResult({
      search: "",
      setSearch: vi.fn(),
      results: items.map((item) => ({ item, matches: [], score: 0, refIndex: 0 })),
      totalCount: items.length,
      isSearching: false,
      clearSearch: vi.fn(),
    }));

    const { usePagination } = await import("@/hooks/usePagination");
    vi.mocked(usePagination).mockReturnValue({
      paginatedItems: items,
      currentPage: 1,
      pageSize: 10,
      totalItems: items.length,
      totalPages: 1,
      setPage: vi.fn(),
      setPageSize: vi.fn(),
    });

    renderWithProviders(<ReceiptItems />);
    await user.click(screen.getByRole("button", { name: /edit/i }));

    expect(
      screen.getByRole("heading", { name: /edit receipt item/i }),
    ).toBeInTheDocument();
  });

  it("closes edit dialog when dismissed", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "ri1", receiptId: "r1", receiptItemCode: "RI-001", description: "Milk", quantity: 2, unitPrice: 3.99, category: "Groceries", subcategory: "Dairy", pricingMode: "quantity" as const },
    ];

    const { useFuzzySearch } = await import("@/hooks/useFuzzySearch");
    vi.mocked(useFuzzySearch).mockReturnValue(mockQueryResult({
      search: "",
      setSearch: vi.fn(),
      results: items.map((item) => ({ item, matches: [], score: 0, refIndex: 0 })),
      totalCount: items.length,
      isSearching: false,
      clearSearch: vi.fn(),
    }));

    const { usePagination } = await import("@/hooks/usePagination");
    vi.mocked(usePagination).mockReturnValue({
      paginatedItems: items,
      currentPage: 1,
      pageSize: 10,
      totalItems: items.length,
      totalPages: 1,
      setPage: vi.fn(),
      setPageSize: vi.fn(),
    });

    renderWithProviders(<ReceiptItems />);
    await user.click(screen.getByRole("button", { name: /edit/i }));
    expect(screen.getByRole("heading", { name: /edit receipt item/i })).toBeInTheDocument();

    await user.keyboard("{Escape}");
    await vi.waitFor(() => {
      expect(screen.queryByRole("heading", { name: /edit receipt item/i })).not.toBeInTheDocument();
    });
  });

  it("closes create dialog when Cancel is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<ReceiptItems />);
    await user.click(screen.getByRole("button", { name: /new item/i }));
    expect(screen.getByRole("heading", { name: /create receipt item/i })).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: /cancel/i }));
    await vi.waitFor(() => {
      expect(screen.queryByRole("heading", { name: /create receipt item/i })).not.toBeInTheDocument();
    });
  });

  it("opens create dialog on shortcut:new-item event", async () => {
    const { act } = await import("@testing-library/react");
    renderWithProviders(<ReceiptItems />);

    act(() => {
      window.dispatchEvent(new Event("shortcut:new-item"));
    });

    await screen.findByRole("heading", { name: /create receipt item/i });
    expect(
      screen.getByRole("heading", { name: /create receipt item/i }),
    ).toBeInTheDocument();
  });

  it("toggles checkbox selection and shows delete button", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "ri1", receiptId: "r1", receiptItemCode: "RI-001", description: "Milk", quantity: 2, unitPrice: 3.99, category: "Groceries", subcategory: "Dairy", pricingMode: "quantity" as const },
    ];

    const { useFuzzySearch } = await import("@/hooks/useFuzzySearch");
    vi.mocked(useFuzzySearch).mockReturnValue(mockQueryResult({
      search: "",
      setSearch: vi.fn(),
      results: items.map((item) => ({ item, matches: [], score: 0, refIndex: 0 })),
      totalCount: items.length,
      isSearching: false,
      clearSearch: vi.fn(),
    }));

    const { useReceiptItems } = await import("@/hooks/useReceiptItems");
    vi.mocked(useReceiptItems).mockReturnValue(mockQueryResult({
      data: { data: items, total: items.length, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<ReceiptItems />);
    await user.click(screen.getByLabelText("Select Milk"));

    expect(
      screen.getByRole("button", { name: /delete/i }),
    ).toBeInTheDocument();
  });

  it("opens delete dialog and confirms deletion", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const mockMutate = vi.fn();
    const { useDeleteReceiptItems } = await import("@/hooks/useReceiptItems");
    vi.mocked(useDeleteReceiptItems).mockReturnValue(mockMutationResult({
      mutate: mockMutate,
      isPending: false,
    }));

    const items = [
      { id: "ri1", receiptId: "r1", receiptItemCode: "RI-001", description: "Milk", quantity: 2, unitPrice: 3.99, category: "Groceries", subcategory: "Dairy", pricingMode: "quantity" as const },
    ];

    const { useFuzzySearch } = await import("@/hooks/useFuzzySearch");
    vi.mocked(useFuzzySearch).mockReturnValue(mockQueryResult({
      search: "",
      setSearch: vi.fn(),
      results: items.map((item) => ({ item, matches: [], score: 0, refIndex: 0 })),
      totalCount: items.length,
      isSearching: false,
      clearSearch: vi.fn(),
    }));

    const { useReceiptItems } = await import("@/hooks/useReceiptItems");
    vi.mocked(useReceiptItems).mockReturnValue(mockQueryResult({
      data: { data: items, total: items.length, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<ReceiptItems />);
    await user.click(screen.getByLabelText("Select Milk"));
    await user.click(screen.getByRole("button", { name: /delete/i }));

    expect(
      screen.getByRole("heading", { name: /delete receipt items/i }),
    ).toBeInTheDocument();

    const dialogDeleteBtn = screen
      .getAllByRole("button", { name: /delete/i })
      .find((btn) => btn.closest("[role='dialog']") !== null);
    if (dialogDeleteBtn) {
      await user.click(dialogDeleteBtn);
      expect(mockMutate).toHaveBeenCalledWith(["ri1"]);
    }
  });

  it("toggles select all checkbox", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "ri1", receiptId: "r1", receiptItemCode: "RI-001", description: "Milk", quantity: 2, unitPrice: 3.99, category: "Groceries", subcategory: "Dairy", pricingMode: "quantity" as const },
      { id: "ri2", receiptId: "r1", receiptItemCode: "RI-002", description: "Bread", quantity: 1, unitPrice: 2.50, category: "Groceries", subcategory: "Bakery", pricingMode: "quantity" as const },
    ];

    const { useFuzzySearch } = await import("@/hooks/useFuzzySearch");
    vi.mocked(useFuzzySearch).mockReturnValue(mockQueryResult({
      search: "",
      setSearch: vi.fn(),
      results: items.map((item) => ({ item, matches: [], score: 0, refIndex: 0 })),
      totalCount: items.length,
      isSearching: false,
      clearSearch: vi.fn(),
    }));

    const { useReceiptItems } = await import("@/hooks/useReceiptItems");
    vi.mocked(useReceiptItems).mockReturnValue(mockQueryResult({
      data: { data: items, total: items.length, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<ReceiptItems />);
    await user.click(screen.getByLabelText("Select all rows"));

    expect(
      screen.getByRole("button", { name: /delete \(2\)/i }),
    ).toBeInTheDocument();
  });

});

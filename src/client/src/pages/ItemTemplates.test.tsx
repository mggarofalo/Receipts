import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult, mockMutationResult } from "@/test/mock-hooks";
import ItemTemplates from "./ItemTemplates";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
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

vi.mock("@/hooks/useItemTemplates", () => ({
  useItemTemplates: vi.fn(() => ({ data: { data: [], total: 0, offset: 0, limit: 50 }, isLoading: false })),
  useCreateItemTemplate: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useUpdateItemTemplate: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useDeleteItemTemplates: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
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
    resetPage: vi.fn(),
  })),
}));

vi.mock("@/hooks/useServerSort", () => ({
  useServerSort: vi.fn(() => ({
    sortBy: "name",
    sortDirection: "asc",
    toggleSort: vi.fn(),
  })),
}));

vi.mock("@/hooks/useListKeyboardNav", () => ({
  useListKeyboardNav: vi.fn(() => ({
    focusedId: null,
    setFocusedIndex: vi.fn(),
    tableRef: { current: null },
  })),
}));

// Mocks needed by ItemTemplateForm (rendered inside dialogs)
vi.mock("@/hooks/useCategories", () => ({
  useCategories: vi.fn(() => ({ data: { data: [] }, isLoading: false })),
}));

vi.mock("@/hooks/useSubcategories", () => ({
  useSubcategoriesByCategoryId: vi.fn(() => ({ data: { data: [] }, isLoading: false })),
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

describe("ItemTemplates", () => {
  it("renders the page heading", () => {
    renderWithProviders(<ItemTemplates />);
    expect(
      screen.getByRole("heading", { name: /item templates/i }),
    ).toBeInTheDocument();
  });

  it("renders loading skeleton when data is loading", async () => {
    const { useItemTemplates } = await import("@/hooks/useItemTemplates");
    vi.mocked(useItemTemplates).mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: true,
    }));

    const { container } = renderWithProviders(<ItemTemplates />);
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
  });

  it("renders empty state when no templates exist", async () => {
    const { useItemTemplates } = await import("@/hooks/useItemTemplates");
    vi.mocked(useItemTemplates).mockReturnValue(mockQueryResult({
      data: { data: [], total: 0, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<ItemTemplates />);
    expect(
      screen.getByText(/no item templates yet/i),
    ).toBeInTheDocument();
  });

  it("renders the New Template button", () => {
    renderWithProviders(<ItemTemplates />);
    expect(
      screen.getByRole("button", { name: /new template/i }),
    ).toBeInTheDocument();
  });

  it("renders the search input", () => {
    renderWithProviders(<ItemTemplates />);
    expect(
      screen.getByPlaceholderText(/search item templates/i),
    ).toBeInTheDocument();
  });

  it("renders table with item templates when data exists", async () => {
    const items = [
      { id: "1", name: "Coffee", description: "Morning coffee", defaultCategory: "Food", defaultSubcategory: "Drinks", defaultUnitPrice: 4.50, defaultUnitPriceCurrency: "USD", defaultPricingMode: "quantity", defaultItemCode: "COF-001" },
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

    const { useItemTemplates } = await import("@/hooks/useItemTemplates");
    vi.mocked(useItemTemplates).mockReturnValue(mockQueryResult({
      data: { data: items, total: items.length, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<ItemTemplates />);
    expect(screen.getByText("Coffee")).toBeInTheDocument();
    expect(screen.getByText("Food")).toBeInTheDocument();
    expect(screen.getByText("Drinks")).toBeInTheDocument();
  });

  it("closes edit dialog when dismissed", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "1", name: "Coffee", description: "Morning coffee", defaultCategory: "Food", defaultSubcategory: "Drinks", defaultUnitPrice: 4.50, defaultUnitPriceCurrency: "USD", defaultPricingMode: "quantity", defaultItemCode: "COF-001" },
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

    renderWithProviders(<ItemTemplates />);
    await user.click(screen.getByRole("button", { name: /edit/i }));
    expect(screen.getByRole("heading", { name: /edit item template/i })).toBeInTheDocument();

    await user.keyboard("{Escape}");
    await vi.waitFor(() => {
      expect(screen.queryByRole("heading", { name: /edit item template/i })).not.toBeInTheDocument();
    });
  });

  it("closes create dialog when Cancel is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<ItemTemplates />);
    await user.click(screen.getByRole("button", { name: /new template/i }));
    expect(screen.getByRole("heading", { name: /create item template/i })).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: /cancel/i }));
    await vi.waitFor(() => {
      expect(screen.queryByRole("heading", { name: /create item template/i })).not.toBeInTheDocument();
    });
  });

  it("opens create dialog when New Template button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<ItemTemplates />);

    await user.click(screen.getByRole("button", { name: /new template/i }));

    expect(
      screen.getByRole("heading", { name: /create item template/i }),
    ).toBeInTheDocument();
  });

  it("opens edit dialog when Edit button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "1", name: "Coffee", description: "Morning coffee", defaultCategory: "Food", defaultSubcategory: "Drinks", defaultUnitPrice: 4.50, defaultUnitPriceCurrency: "USD", defaultPricingMode: "quantity", defaultItemCode: "COF-001" },
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

    const { useItemTemplates } = await import("@/hooks/useItemTemplates");
    vi.mocked(useItemTemplates).mockReturnValue(mockQueryResult({
      data: { data: items, total: items.length, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<ItemTemplates />);
    await user.click(screen.getByRole("button", { name: /edit/i }));

    expect(
      screen.getByRole("heading", { name: /edit item template/i }),
    ).toBeInTheDocument();
  });

  it("toggles checkbox selection and shows delete button", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "1", name: "Coffee", description: "Morning coffee", defaultCategory: "Food", defaultSubcategory: "Drinks", defaultUnitPrice: 4.50, defaultUnitPriceCurrency: "USD", defaultPricingMode: "quantity", defaultItemCode: "COF-001" },
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

    const { useItemTemplates } = await import("@/hooks/useItemTemplates");
    vi.mocked(useItemTemplates).mockReturnValue(mockQueryResult({
      data: { data: items, total: items.length, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<ItemTemplates />);
    await user.click(screen.getByLabelText("Select Coffee"));

    expect(
      screen.getByRole("button", { name: /delete/i }),
    ).toBeInTheDocument();
  });

  it("submits edit form and calls updateItemTemplate.mutate", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const mockMutate = vi.fn();
    const { useUpdateItemTemplate } = await import("@/hooks/useItemTemplates");
    vi.mocked(useUpdateItemTemplate).mockReturnValue(mockMutationResult({
      mutate: mockMutate,
      isPending: false,
    }));

    const items = [
      { id: "1", name: "Coffee", description: "Morning coffee", defaultCategory: "Food", defaultSubcategory: "Drinks", defaultUnitPrice: 4.50, defaultUnitPriceCurrency: "USD", defaultPricingMode: "quantity", defaultItemCode: "COF-001" },
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

    renderWithProviders(<ItemTemplates />);
    await user.click(screen.getByRole("button", { name: /edit/i }));

    const nameInput = screen.getByLabelText(/^name$/i);
    await user.clear(nameInput);
    await user.type(nameInput, "Updated Template");
    await user.click(screen.getByRole("button", { name: /update template/i }));

    await vi.waitFor(() => {
      expect(mockMutate).toHaveBeenCalled();
    });
  });

  it("submits create form and calls createItemTemplate.mutate", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const mockMutate = vi.fn();
    const { useCreateItemTemplate } = await import("@/hooks/useItemTemplates");
    vi.mocked(useCreateItemTemplate).mockReturnValue(mockMutationResult({
      mutate: mockMutate,
      isPending: false,
    }));

    renderWithProviders(<ItemTemplates />);
    await user.click(screen.getByRole("button", { name: /new template/i }));

    await user.type(screen.getByLabelText(/^name$/i), "Coffee Template");
    await user.click(screen.getByRole("button", { name: /create template/i }));

    await vi.waitFor(() => {
      expect(mockMutate).toHaveBeenCalled();
    });
  });

  it("renders NoResults when search returns no matches", async () => {
    const { useItemTemplates } = await import("@/hooks/useItemTemplates");
    vi.mocked(useItemTemplates).mockReturnValue(mockQueryResult({
      data: [{ id: "1", name: "Coffee", description: "Morning coffee", defaultCategory: "Food", defaultSubcategory: "Drinks", defaultUnitPrice: 4.50, defaultUnitPriceCurrency: "USD", defaultPricingMode: "quantity", defaultItemCode: "COF-001" }],
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

    renderWithProviders(<ItemTemplates />);
    expect(screen.getByText(/try fewer keywords/i)).toBeInTheDocument();
  });

  it("opens delete dialog and confirms deletion", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const mockMutate = vi.fn();
    const { useDeleteItemTemplates } = await import("@/hooks/useItemTemplates");
    vi.mocked(useDeleteItemTemplates).mockReturnValue(mockMutationResult({
      mutate: mockMutate,
      isPending: false,
    }));

    const items = [
      { id: "1", name: "Coffee", description: "Morning coffee", defaultCategory: "Food", defaultSubcategory: "Drinks", defaultUnitPrice: 4.50, defaultUnitPriceCurrency: "USD", defaultPricingMode: "quantity", defaultItemCode: "COF-001" },
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

    const { useItemTemplates } = await import("@/hooks/useItemTemplates");
    vi.mocked(useItemTemplates).mockReturnValue(mockQueryResult({
      data: { data: items, total: items.length, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<ItemTemplates />);
    await user.click(screen.getByLabelText("Select Coffee"));
    await user.click(screen.getByRole("button", { name: /delete/i }));

    expect(
      screen.getByRole("heading", { name: /delete item templates/i }),
    ).toBeInTheDocument();

    const dialogDeleteBtn = screen
      .getAllByRole("button", { name: /delete/i })
      .find((btn) => btn.closest("[role='dialog']") !== null);
    if (dialogDeleteBtn) {
      await user.click(dialogDeleteBtn);
      expect(mockMutate).toHaveBeenCalledWith(["1"]);
    }
  });

  it("opens create dialog on shortcut:new-item event", async () => {
    const { act } = await import("@testing-library/react");
    renderWithProviders(<ItemTemplates />);

    act(() => {
      window.dispatchEvent(new Event("shortcut:new-item"));
    });

    await screen.findByRole("heading", { name: /create item template/i });
    expect(
      screen.getByRole("heading", { name: /create item template/i }),
    ).toBeInTheDocument();
  });
});

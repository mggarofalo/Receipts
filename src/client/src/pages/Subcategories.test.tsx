import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult, mockMutationResult } from "@/test/mock-hooks";
import Subcategories from "./Subcategories";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useSubcategories", () => ({
  useSubcategories: vi.fn(() => ({ data: { data: [], total: 0, offset: 0, limit: 50 }, isLoading: false })),
  useSubcategoriesByCategoryId: vi.fn(() => ({ data: { data: [], total: 0, offset: 0, limit: 200 }, isLoading: false })),
  useCreateSubcategory: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useUpdateSubcategory: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useDeleteSubcategories: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

vi.mock("@/hooks/useCategories", () => ({
  useCategories: vi.fn(() => ({ data: [], isLoading: false })),
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

describe("Subcategories", () => {
  it("renders the page heading", () => {
    renderWithProviders(<Subcategories />);
    expect(
      screen.getByRole("heading", { name: /subcategories/i }),
    ).toBeInTheDocument();
  });

  it("renders loading skeleton when data is loading", async () => {
    const { useSubcategories } = await import("@/hooks/useSubcategories");
    vi.mocked(useSubcategories).mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: true,
    }));

    const { container } = renderWithProviders(<Subcategories />);
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
  });

  it("renders empty state when no subcategories exist", async () => {
    const { useSubcategories } = await import("@/hooks/useSubcategories");
    vi.mocked(useSubcategories).mockReturnValue(mockQueryResult({
      data: { data: [], total: 0, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<Subcategories />);
    expect(
      screen.getByText(/no subcategories yet/i),
    ).toBeInTheDocument();
  });

  it("renders the New Subcategory button", () => {
    renderWithProviders(<Subcategories />);
    expect(
      screen.getByRole("button", { name: /new subcategory/i }),
    ).toBeInTheDocument();
  });

  it("renders the search input", () => {
    renderWithProviders(<Subcategories />);
    expect(
      screen.getByPlaceholderText(/search subcategories/i),
    ).toBeInTheDocument();
  });

  it("renders table with subcategories when data exists", async () => {
    const items = [
      { id: "1", name: "Dairy", categoryId: "c1", categoryName: "Food", description: "Dairy products" },
      { id: "2", name: "Electronics", categoryId: "c2", categoryName: "Goods", description: null },
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

    const { useSubcategories } = await import("@/hooks/useSubcategories");
    vi.mocked(useSubcategories).mockReturnValue(mockQueryResult({
      data: { data: items, total: items.length, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<Subcategories />);
    expect(screen.getByText("Dairy")).toBeInTheDocument();
    expect(screen.getByText("Electronics")).toBeInTheDocument();
  });

  it("opens create dialog when New Subcategory button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<Subcategories />);

    await user.click(screen.getByRole("button", { name: /new subcategory/i }));

    expect(
      screen.getByRole("heading", { name: /create subcategory/i }),
    ).toBeInTheDocument();
  });

  it("toggles checkbox selection and shows delete button", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "1", name: "Dairy", categoryId: "c1", categoryName: "Food", description: "Dairy products" },
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

    const { useSubcategories } = await import("@/hooks/useSubcategories");
    vi.mocked(useSubcategories).mockReturnValue(mockQueryResult({
      data: { data: items, total: items.length, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<Subcategories />);
    await user.click(screen.getByLabelText("Select Dairy"));

    expect(
      screen.getByRole("button", { name: /delete/i }),
    ).toBeInTheDocument();
  });

  it("opens delete dialog and confirms deletion", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const mockMutate = vi.fn();
    const { useDeleteSubcategories } = await import("@/hooks/useSubcategories");
    vi.mocked(useDeleteSubcategories).mockReturnValue(mockMutationResult({
      mutate: mockMutate,
      isPending: false,
    }));

    const items = [
      { id: "1", name: "Dairy", categoryId: "c1", categoryName: "Food", description: "Dairy products" },
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

    const { useSubcategories } = await import("@/hooks/useSubcategories");
    vi.mocked(useSubcategories).mockReturnValue(mockQueryResult({
      data: { data: items, total: items.length, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<Subcategories />);
    await user.click(screen.getByLabelText("Select Dairy"));
    await user.click(screen.getByRole("button", { name: /delete/i }));

    expect(
      screen.getByRole("heading", { name: /delete subcategories/i }),
    ).toBeInTheDocument();

    const dialogDeleteBtn = screen
      .getAllByRole("button", { name: /delete/i })
      .find((btn) => btn.closest("[role='dialog']") !== null);
    if (dialogDeleteBtn) {
      await user.click(dialogDeleteBtn);
      expect(mockMutate).toHaveBeenCalledWith(["1"]);
    }
  });

  it("closes edit dialog when dismissed", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "1", name: "Dairy", categoryId: "c1", categoryName: "Food", description: "Dairy products" },
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

    renderWithProviders(<Subcategories />);
    await user.click(screen.getByRole("button", { name: /edit/i }));
    expect(screen.getByRole("heading", { name: /edit subcategory/i })).toBeInTheDocument();

    await user.keyboard("{Escape}");
    await vi.waitFor(() => {
      expect(screen.queryByRole("heading", { name: /edit subcategory/i })).not.toBeInTheDocument();
    });
  });

  it("closes create dialog when Cancel is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<Subcategories />);
    await user.click(screen.getByRole("button", { name: /new subcategory/i }));
    expect(screen.getByRole("heading", { name: /create subcategory/i })).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: /cancel/i }));
    await vi.waitFor(() => {
      expect(screen.queryByRole("heading", { name: /create subcategory/i })).not.toBeInTheDocument();
    });
  });

  it("renders NoResults when search returns no matches", async () => {
    const { useSubcategories } = await import("@/hooks/useSubcategories");
    vi.mocked(useSubcategories).mockReturnValue(mockQueryResult({
      data: [{ id: "1", name: "Dairy", categoryId: "c1", description: "Dairy products" }],
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

    renderWithProviders(<Subcategories />);
    expect(screen.getByText(/try fewer keywords/i)).toBeInTheDocument();
  });

  it("opens edit dialog when Edit button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "1", name: "Dairy", categoryId: "c1", categoryName: "Food", description: "Dairy products" },
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

    renderWithProviders(<Subcategories />);
    await user.click(screen.getByRole("button", { name: /edit/i }));

    expect(
      screen.getByRole("heading", { name: /edit subcategory/i }),
    ).toBeInTheDocument();
  });

  it("opens create dialog on shortcut:new-item event", async () => {
    const { act } = await import("@testing-library/react");
    renderWithProviders(<Subcategories />);

    act(() => {
      window.dispatchEvent(new Event("shortcut:new-item"));
    });

    await screen.findByRole("heading", { name: /create subcategory/i });
    expect(
      screen.getByRole("heading", { name: /create subcategory/i }),
    ).toBeInTheDocument();
  });
});

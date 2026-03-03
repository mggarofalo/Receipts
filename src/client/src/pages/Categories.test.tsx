import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult, mockMutationResult } from "@/test/mock-hooks";
import Categories from "./Categories";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useCategories", () => ({
  useCategories: vi.fn(() => ({ data: [], isLoading: false })),
  useCreateCategory: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useUpdateCategory: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useDeleteCategories: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
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

vi.mock("@/hooks/usePagination", () => ({
  usePagination: vi.fn(() => ({
    paginatedItems: [],
    currentPage: 1,
    pageSize: 10,
    totalItems: 0,
    totalPages: 0,
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

describe("Categories", () => {
  it("renders the page heading", () => {
    renderWithProviders(<Categories />);
    expect(
      screen.getByRole("heading", { name: /categories/i }),
    ).toBeInTheDocument();
  });

  it("renders loading skeleton when data is loading", async () => {
    const { useCategories } = await import("@/hooks/useCategories");
    vi.mocked(useCategories).mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: true,
    }));

    const { container } = renderWithProviders(<Categories />);
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
  });

  it("renders empty state when no categories exist", async () => {
    const { useCategories } = await import("@/hooks/useCategories");
    vi.mocked(useCategories).mockReturnValue(mockQueryResult({
      data: [],
      isLoading: false,
    }));

    renderWithProviders(<Categories />);
    expect(
      screen.getByText(/no categories yet/i),
    ).toBeInTheDocument();
  });

  it("renders the New Category button", () => {
    renderWithProviders(<Categories />);
    expect(
      screen.getByRole("button", { name: /new category/i }),
    ).toBeInTheDocument();
  });

  it("renders the search input", () => {
    renderWithProviders(<Categories />);
    expect(
      screen.getByPlaceholderText(/search categories/i),
    ).toBeInTheDocument();
  });

  it("renders table with categories when data exists", async () => {
    const items = [
      { id: "1", name: "Food", description: "Food expenses" },
      { id: "2", name: "Travel", description: null },
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

    renderWithProviders(<Categories />);
    expect(screen.getByText("Food")).toBeInTheDocument();
    expect(screen.getByText("Travel")).toBeInTheDocument();
    expect(screen.getByText("Food expenses")).toBeInTheDocument();
  });

  it("opens create dialog when New Category button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<Categories />);

    await user.click(screen.getByRole("button", { name: /new category/i }));

    expect(
      screen.getByRole("heading", { name: /create category/i }),
    ).toBeInTheDocument();
  });

  it("opens edit dialog when Edit button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "1", name: "Food", description: "Food expenses" },
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

    renderWithProviders(<Categories />);
    await user.click(screen.getByRole("button", { name: /edit/i }));

    expect(
      screen.getByRole("heading", { name: /edit category/i }),
    ).toBeInTheDocument();
  });

  it("toggles checkbox selection and shows delete button", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "1", name: "Food", description: "Food expenses" },
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

    renderWithProviders(<Categories />);
    await user.click(screen.getByLabelText("Select Food"));

    expect(
      screen.getByRole("button", { name: /delete/i }),
    ).toBeInTheDocument();
  });

  it("opens delete dialog and confirms deletion", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const mockMutate = vi.fn();
    const { useDeleteCategories } = await import("@/hooks/useCategories");
    vi.mocked(useDeleteCategories).mockReturnValue(mockMutationResult({
      mutate: mockMutate,
      isPending: false,
    }));

    const items = [
      { id: "1", name: "Food", description: "Food expenses" },
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

    renderWithProviders(<Categories />);
    await user.click(screen.getByLabelText("Select Food"));
    await user.click(screen.getByRole("button", { name: /delete/i }));

    expect(
      screen.getByRole("heading", { name: /delete categories/i }),
    ).toBeInTheDocument();

    const dialogDeleteBtn = screen
      .getAllByRole("button", { name: /delete/i })
      .find((btn) => btn.closest("[role='dialog']") !== null);
    if (dialogDeleteBtn) {
      await user.click(dialogDeleteBtn);
      expect(mockMutate).toHaveBeenCalledWith(["1"]);
    }
  });

  it("toggles select all checkbox", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "1", name: "Food", description: "Food expenses" },
      { id: "2", name: "Travel", description: null },
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

    renderWithProviders(<Categories />);
    await user.click(screen.getByLabelText("Select all rows"));

    expect(
      screen.getByRole("button", { name: /delete \(2\)/i }),
    ).toBeInTheDocument();
  });

  it("opens create dialog on shortcut:new-item event", async () => {
    const { act } = await import("@testing-library/react");
    renderWithProviders(<Categories />);

    act(() => {
      window.dispatchEvent(new Event("shortcut:new-item"));
    });

    await screen.findByRole("heading", { name: /create category/i });
    expect(
      screen.getByRole("heading", { name: /create category/i }),
    ).toBeInTheDocument();
  });
});

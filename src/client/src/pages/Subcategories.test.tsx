import { screen, within } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult } from "@/test/mock-hooks";
import Subcategories from "./Subcategories";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useSubcategories", () => ({
  useSubcategories: vi.fn(() => ({
    data: { data: [], total: 0, offset: 0, limit: 50 },
    isLoading: false,
  })),
  useSubcategoriesByCategoryId: vi.fn(() => ({
    data: { data: [], total: 0, offset: 0, limit: 200 },
    isLoading: false,
  })),
  useCreateSubcategory: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useUpdateSubcategory: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
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

const ITEMS = [
  { id: "1", name: "Dairy", categoryId: "c1", description: "Dairy products" },
  { id: "2", name: "Bakery", categoryId: "c1", description: "Baked goods" },
  { id: "3", name: "Electronics", categoryId: "c2", description: null },
];

const CATEGORIES = [
  { id: "c1", name: "Food" },
  { id: "c2", name: "Goods" },
];

async function setupWithData(items = ITEMS, categories = CATEGORIES) {
  const { useFuzzySearch } = await import("@/hooks/useFuzzySearch");
  vi.mocked(useFuzzySearch).mockReturnValue(
    mockQueryResult({
      search: "",
      setSearch: vi.fn(),
      results: items.map((item) => ({
        item,
        matches: [],
        score: 0,
        refIndex: 0,
      })),
      totalCount: items.length,
      isSearching: false,
      clearSearch: vi.fn(),
    }),
  );

  const { useSubcategories } = await import("@/hooks/useSubcategories");
  vi.mocked(useSubcategories).mockReturnValue(
    mockQueryResult({
      data: { data: items, total: items.length, offset: 0, limit: 50 },
      isLoading: false,
    }),
  );

  const { useCategories } = await import("@/hooks/useCategories");
  // Use Object.assign to create an array with a .data property so both
  // the page (accesses .data) and SubcategoryForm (iterates directly) work.
  const catData = Object.assign([...categories], { data: categories });
  vi.mocked(useCategories).mockReturnValue(
    mockQueryResult({
      data: catData,
      isLoading: false,
    }),
  );
}

describe("Subcategories", () => {
  it("renders the page heading", () => {
    renderWithProviders(<Subcategories />);
    expect(
      screen.getByRole("heading", { name: /subcategories/i }),
    ).toBeInTheDocument();
  });

  it("renders loading skeleton when data is loading", async () => {
    const { useSubcategories } = await import("@/hooks/useSubcategories");
    vi.mocked(useSubcategories).mockReturnValue(
      mockQueryResult({
        data: undefined,
        isLoading: true,
      }),
    );

    const { container } = renderWithProviders(<Subcategories />);
    expect(
      container.querySelector("[data-slot='skeleton']"),
    ).toBeInTheDocument();
  });

  it("renders empty state when no subcategories exist", async () => {
    const { useSubcategories } = await import("@/hooks/useSubcategories");
    vi.mocked(useSubcategories).mockReturnValue(
      mockQueryResult({
        data: { data: [], total: 0, offset: 0, limit: 50 },
        isLoading: false,
      }),
    );

    renderWithProviders(<Subcategories />);
    expect(screen.getByText(/no subcategories yet/i)).toBeInTheDocument();
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

  it("renders category group headers when data exists", async () => {
    await setupWithData();
    renderWithProviders(<Subcategories />);

    expect(screen.getByText("Food")).toBeInTheDocument();
    expect(screen.getByText("Goods")).toBeInTheDocument();
  });

  it("groups are collapsed by default — subcategory rows not visible", async () => {
    await setupWithData();
    renderWithProviders(<Subcategories />);

    expect(screen.queryByText("Dairy")).not.toBeInTheDocument();
    expect(screen.queryByText("Electronics")).not.toBeInTheDocument();
  });

  it("clicking a category header expands its group", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    await setupWithData();
    renderWithProviders(<Subcategories />);

    await user.click(screen.getByTestId("category-header-c1"));

    expect(screen.getByText("Dairy")).toBeInTheDocument();
    expect(screen.getByText("Bakery")).toBeInTheDocument();
    expect(screen.queryByText("Electronics")).not.toBeInTheDocument();
  });

  it("clicking an expanded category header collapses it", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    await setupWithData();
    renderWithProviders(<Subcategories />);

    const foodHeader = screen.getByTestId("category-header-c1");
    await user.click(foodHeader);
    expect(screen.getByText("Dairy")).toBeInTheDocument();

    await user.click(foodHeader);
    expect(screen.queryByText("Dairy")).not.toBeInTheDocument();
  });

  it("Expand All button expands all groups", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    await setupWithData();
    renderWithProviders(<Subcategories />);

    await user.click(screen.getByRole("button", { name: /expand all/i }));

    expect(screen.getByText("Dairy")).toBeInTheDocument();
    expect(screen.getByText("Bakery")).toBeInTheDocument();
    expect(screen.getByText("Electronics")).toBeInTheDocument();
  });

  it("Collapse All button collapses all groups", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    await setupWithData();
    renderWithProviders(<Subcategories />);

    await user.click(screen.getByRole("button", { name: /expand all/i }));
    expect(screen.getByText("Dairy")).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: /collapse all/i }));
    expect(screen.queryByText("Dairy")).not.toBeInTheDocument();
    expect(screen.queryByText("Electronics")).not.toBeInTheDocument();
  });

  it("category header shows item count", async () => {
    await setupWithData();
    renderWithProviders(<Subcategories />);

    const foodHeader = screen.getByTestId("category-header-c1");
    expect(within(foodHeader).getByText("(2)")).toBeInTheDocument();

    const goodsHeader = screen.getByTestId("category-header-c2");
    expect(within(goodsHeader).getByText("(1)")).toBeInTheDocument();
  });

  it("opens create dialog when New Subcategory button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<Subcategories />);

    await user.click(screen.getByRole("button", { name: /new subcategory/i }));

    expect(
      screen.getByRole("heading", { name: /create subcategory/i }),
    ).toBeInTheDocument();
  });

  it("closes edit dialog when dismissed", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    await setupWithData();
    renderWithProviders(<Subcategories />);

    await user.click(screen.getByTestId("category-header-c1"));
    await user.click(screen.getAllByRole("button", { name: /edit/i })[0]);
    expect(
      screen.getByRole("heading", { name: /edit subcategory/i }),
    ).toBeInTheDocument();

    await user.keyboard("{Escape}");
    await vi.waitFor(() => {
      expect(
        screen.queryByRole("heading", { name: /edit subcategory/i }),
      ).not.toBeInTheDocument();
    });
  });

  it("closes create dialog when Cancel is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<Subcategories />);
    await user.click(screen.getByRole("button", { name: /new subcategory/i }));
    expect(
      screen.getByRole("heading", { name: /create subcategory/i }),
    ).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: /cancel/i }));
    await vi.waitFor(() => {
      expect(
        screen.queryByRole("heading", { name: /create subcategory/i }),
      ).not.toBeInTheDocument();
    });
  });

  it("renders NoResults when search returns no matches", async () => {
    const { useSubcategories } = await import("@/hooks/useSubcategories");
    vi.mocked(useSubcategories).mockReturnValue(
      mockQueryResult({
        data: [
          {
            id: "1",
            name: "Dairy",
            categoryId: "c1",
            description: "Dairy products",
          },
        ],
        isLoading: false,
      }),
    );

    const { useFuzzySearch } = await import("@/hooks/useFuzzySearch");
    vi.mocked(useFuzzySearch).mockReturnValue(
      mockQueryResult({
        search: "xyz",
        setSearch: vi.fn(),
        results: [],
        totalCount: 0,
        isSearching: false,
        clearSearch: vi.fn(),
      }),
    );

    renderWithProviders(<Subcategories />);
    expect(screen.getByText(/try fewer keywords/i)).toBeInTheDocument();
  });

  it("opens edit dialog when Edit button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    await setupWithData();
    renderWithProviders(<Subcategories />);

    await user.click(screen.getByTestId("category-header-c1"));
    await user.click(screen.getAllByRole("button", { name: /edit/i })[0]);

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

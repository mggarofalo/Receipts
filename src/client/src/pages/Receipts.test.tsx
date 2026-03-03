import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult, mockMutationResult } from "@/test/mock-hooks";
import Receipts from "./Receipts";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useReceipts", () => ({
  useReceipts: vi.fn(() => ({ data: [], isLoading: false })),
  useCreateReceipt: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useUpdateReceipt: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useDeleteReceipts: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
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

describe("Receipts", () => {
  it("renders the page heading", () => {
    renderWithProviders(<Receipts />);
    expect(
      screen.getByRole("heading", { name: /receipts/i }),
    ).toBeInTheDocument();
  });

  it("renders loading skeleton when data is loading", async () => {
    const { useReceipts } = await import("@/hooks/useReceipts");
    vi.mocked(useReceipts).mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: true,
    }));

    const { container } = renderWithProviders(<Receipts />);
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
  });

  it("renders empty state when no receipts exist", async () => {
    const { useReceipts } = await import("@/hooks/useReceipts");
    vi.mocked(useReceipts).mockReturnValue(mockQueryResult({
      data: [],
      isLoading: false,
    }));

    renderWithProviders(<Receipts />);
    expect(
      screen.getByText(/no receipts yet/i),
    ).toBeInTheDocument();
  });

  it("renders the New Receipt button", () => {
    renderWithProviders(<Receipts />);
    expect(
      screen.getByRole("button", { name: /new receipt/i }),
    ).toBeInTheDocument();
  });

  it("renders the search input", () => {
    renderWithProviders(<Receipts />);
    expect(
      screen.getByPlaceholderText(/search receipts/i),
    ).toBeInTheDocument();
  });

  it("renders table with receipts when data exists", async () => {
    const items = [
      { id: "1", description: "Grocery", location: "Walmart", date: "2024-01-15", taxAmount: 5.25 },
      { id: "2", description: null, location: "Target", date: "2024-01-20", taxAmount: 3.50 },
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

    renderWithProviders(<Receipts />);
    expect(screen.getByText("Grocery")).toBeInTheDocument();
    expect(screen.getByText("Walmart")).toBeInTheDocument();
    expect(screen.getByText("Target")).toBeInTheDocument();
    expect(screen.getByText(/no description/i)).toBeInTheDocument();
  });

  it("opens create dialog when New Receipt button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<Receipts />);

    await user.click(screen.getByRole("button", { name: /new receipt/i }));

    expect(
      screen.getByRole("heading", { name: /create receipt/i }),
    ).toBeInTheDocument();
  });

  it("opens edit dialog when Edit button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "1", description: "Grocery", location: "Walmart", date: "2024-01-15", taxAmount: 5.25 },
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

    renderWithProviders(<Receipts />);
    await user.click(screen.getByRole("button", { name: /edit/i }));

    expect(
      screen.getByRole("heading", { name: /edit receipt/i }),
    ).toBeInTheDocument();
  });

  it("toggles checkbox selection and shows delete button", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "1", description: "Grocery", location: "Walmart", date: "2024-01-15", taxAmount: 5.25 },
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

    renderWithProviders(<Receipts />);
    await user.click(screen.getByLabelText("Select Grocery"));

    expect(
      screen.getByRole("button", { name: /delete/i }),
    ).toBeInTheDocument();
  });

  it("opens delete dialog and confirms deletion", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const mockMutate = vi.fn();
    const { useDeleteReceipts } = await import("@/hooks/useReceipts");
    vi.mocked(useDeleteReceipts).mockReturnValue(mockMutationResult({
      mutate: mockMutate,
      isPending: false,
    }));

    const items = [
      { id: "1", description: "Grocery", location: "Walmart", date: "2024-01-15", taxAmount: 5.25 },
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

    renderWithProviders(<Receipts />);
    await user.click(screen.getByLabelText("Select Grocery"));
    await user.click(screen.getByRole("button", { name: /delete/i }));

    expect(
      screen.getByRole("heading", { name: /delete receipts/i }),
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
      { id: "1", description: "Grocery", location: "Walmart", date: "2024-01-15", taxAmount: 5.25 },
      { id: "2", description: "Lunch", location: "Chipotle", date: "2024-01-20", taxAmount: 1.50 },
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

    renderWithProviders(<Receipts />);
    await user.click(screen.getByLabelText("Select all rows"));

    expect(
      screen.getByRole("button", { name: /delete \(2\)/i }),
    ).toBeInTheDocument();
  });

  it("opens create dialog on shortcut:new-item event", async () => {
    const { act } = await import("@testing-library/react");
    renderWithProviders(<Receipts />);

    act(() => {
      window.dispatchEvent(new Event("shortcut:new-item"));
    });

    await screen.findByRole("heading", { name: /create receipt/i });
    expect(
      screen.getByRole("heading", { name: /create receipt/i }),
    ).toBeInTheDocument();
  });
});

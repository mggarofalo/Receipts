import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult, mockMutationResult } from "@/test/mock-hooks";
import Transactions from "./Transactions";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useTransactions", () => ({
  useTransactions: vi.fn(() => ({ data: [], isLoading: false })),
  useCreateTransaction: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useUpdateTransaction: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useDeleteTransactions: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
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

// Mocks needed by TransactionForm (rendered inside dialogs)
vi.mock("@/hooks/useReceipts", () => ({
  useReceipts: vi.fn(() => ({ data: [], isLoading: false })),
}));

vi.mock("@/hooks/useAccounts", () => ({
  useAccounts: vi.fn(() => ({ data: [], isLoading: false })),
}));

describe("Transactions", () => {
  it("renders the page heading", () => {
    renderWithProviders(<Transactions />);
    expect(
      screen.getByRole("heading", { name: /transactions/i }),
    ).toBeInTheDocument();
  });

  it("renders loading skeleton when data is loading", async () => {
    const { useTransactions } = await import("@/hooks/useTransactions");
    vi.mocked(useTransactions).mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: true,
    }));

    const { container } = renderWithProviders(<Transactions />);
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
  });

  it("renders empty state when no transactions exist", async () => {
    const { useTransactions } = await import("@/hooks/useTransactions");
    vi.mocked(useTransactions).mockReturnValue(mockQueryResult({
      data: [],
      isLoading: false,
    }));

    renderWithProviders(<Transactions />);
    expect(
      screen.getByText(/no transactions yet/i),
    ).toBeInTheDocument();
  });

  it("renders the New Transaction button", () => {
    renderWithProviders(<Transactions />);
    expect(
      screen.getByRole("button", { name: /new transaction/i }),
    ).toBeInTheDocument();
  });

  it("renders the search input", () => {
    renderWithProviders(<Transactions />);
    expect(
      screen.getByPlaceholderText(/search transactions/i),
    ).toBeInTheDocument();
  });

  it("renders table with transactions when data exists", async () => {
    const items = [
      { id: "t1", receiptId: "r1", accountId: "a1", amount: 25.50, date: "2024-01-15" },
      { id: "t2", receiptId: "r2", accountId: "a2", amount: 100.00, date: "2024-01-20" },
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

    renderWithProviders(<Transactions />);
    expect(screen.getByText("2024-01-15")).toBeInTheDocument();
    expect(screen.getByText("2024-01-20")).toBeInTheDocument();
  });

  it("opens create dialog when New Transaction button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<Transactions />);

    await user.click(screen.getByRole("button", { name: /new transaction/i }));

    expect(
      screen.getByRole("heading", { name: /create transaction/i }),
    ).toBeInTheDocument();
  });

  it("toggles checkbox selection and shows delete button", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "t1", receiptId: "r1", accountId: "a1", amount: 25.50, date: "2024-01-15" },
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

    renderWithProviders(<Transactions />);
    await user.click(screen.getByLabelText("Select transaction t1"));

    expect(
      screen.getByRole("button", { name: /delete/i }),
    ).toBeInTheDocument();
  });

  it("opens delete dialog and confirms deletion", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const mockMutate = vi.fn();
    const { useDeleteTransactions } = await import("@/hooks/useTransactions");
    vi.mocked(useDeleteTransactions).mockReturnValue(mockMutationResult({
      mutate: mockMutate,
      isPending: false,
    }));

    const items = [
      { id: "t1", receiptId: "r1", accountId: "a1", amount: 25.50, date: "2024-01-15" },
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

    renderWithProviders(<Transactions />);
    await user.click(screen.getByLabelText("Select transaction t1"));
    await user.click(screen.getByRole("button", { name: /delete/i }));

    expect(
      screen.getByRole("heading", { name: /delete transactions/i }),
    ).toBeInTheDocument();

    const dialogDeleteBtn = screen
      .getAllByRole("button", { name: /delete/i })
      .find((btn) => btn.closest("[role='dialog']") !== null);
    if (dialogDeleteBtn) {
      await user.click(dialogDeleteBtn);
      expect(mockMutate).toHaveBeenCalledWith(["t1"]);
    }
  });

  it("toggles select all checkbox", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      { id: "t1", receiptId: "r1", accountId: "a1", amount: 25.50, date: "2024-01-15" },
      { id: "t2", receiptId: "r2", accountId: "a2", amount: 100.00, date: "2024-01-20" },
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

    renderWithProviders(<Transactions />);
    await user.click(screen.getByLabelText("Select all rows"));

    expect(
      screen.getByRole("button", { name: /delete \(2\)/i }),
    ).toBeInTheDocument();
  });

  it("opens create dialog on shortcut:new-item event", async () => {
    const { act } = await import("@testing-library/react");
    renderWithProviders(<Transactions />);

    act(() => {
      window.dispatchEvent(new Event("shortcut:new-item"));
    });

    await screen.findByRole("heading", { name: /create transaction/i });
    expect(
      screen.getByRole("heading", { name: /create transaction/i }),
    ).toBeInTheDocument();
  });
});

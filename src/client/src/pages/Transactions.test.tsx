import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult, mockMutationResult } from "@/test/mock-hooks";
import { mockTransactionResponse } from "@/test/mock-api";
import Transactions from "./Transactions";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useTransactions", () => ({
  useTransactions: vi.fn(() => ({ data: [], total: 0, isLoading: false })),
  useTransactionsByReceiptId: vi.fn(() => ({ data: [], total: 0, isLoading: false })),
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
    sortBy: "date",
    sortDirection: "desc",
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

// Mocks needed by TransactionForm (rendered inside dialogs) and receipt/account lookups
vi.mock("@/hooks/useReceipts", () => ({
  useReceipts: vi.fn(() => ({
    data: [
      { id: "r1", location: "Whole Foods", date: "2024-01-14" },
      { id: "r2", location: "Target Store", date: "2024-01-19" },
    ],
    total: 2,
    isLoading: false,
  })),
}));

vi.mock("@/hooks/useAccounts", () => ({
  useAccounts: vi.fn(() => ({
    data: [
      { id: "a1", name: "Chase Checking" },
      { id: "a2", name: "Amex Gold" },
    ],
    total: 2,
    isLoading: false,
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
      total: 0,
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
      mockTransactionResponse({ id: "t1", receiptId: "r1", accountId: "a1", amount: 25.50, date: "2024-01-15" }),
      mockTransactionResponse({ id: "t2", receiptId: "r2", accountId: "a2", amount: 100.00, date: "2024-01-20" }),
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

    const { useTransactions } = await import("@/hooks/useTransactions");
    vi.mocked(useTransactions).mockReturnValue(mockQueryResult({
      data: items,
      total: items.length,
      isLoading: false,
    }));

    renderWithProviders(<Transactions />);
    expect(screen.getByText("2024-01-15")).toBeInTheDocument();
    expect(screen.getByText("2024-01-20")).toBeInTheDocument();
    // Verify account names are resolved
    expect(screen.getByText("Chase Checking")).toBeInTheDocument();
    expect(screen.getByText("Amex Gold")).toBeInTheDocument();
    // Verify receipt locations are shown (in both Receipt link and Location columns)
    expect(screen.getAllByText("Whole Foods")).toHaveLength(2);
    expect(screen.getAllByText("Target Store")).toHaveLength(2);
  });

  it("renders receipt location and receipt date columns", async () => {
    const items = [
      mockTransactionResponse({ id: "t1", receiptId: "r1", accountId: "a1", amount: 25.50, date: "2024-01-15" }),
      mockTransactionResponse({ id: "t2", receiptId: "r2", accountId: "a2", amount: 100.00, date: "2024-01-20" }),
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

    const { useTransactions } = await import("@/hooks/useTransactions");
    vi.mocked(useTransactions).mockReturnValue(mockQueryResult({
      data: items,
      total: items.length,
      isLoading: false,
    }));

    renderWithProviders(<Transactions />);
    // Location appears in both Receipt link and Location columns
    expect(screen.getAllByText("Whole Foods")).toHaveLength(2);
    expect(screen.getAllByText("Target Store")).toHaveLength(2);
    // Verify receipt date column values
    expect(screen.getByText("2024-01-14")).toBeInTheDocument();
    expect(screen.getByText("2024-01-19")).toBeInTheDocument();
    // Verify column headers
    expect(screen.getByText("Location")).toBeInTheDocument();
    expect(screen.getByText("Receipt Date")).toBeInTheDocument();
    expect(screen.getByText("Transaction Date")).toBeInTheDocument();
  });

  it("opens create dialog when New Transaction button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<Transactions />);

    await user.click(screen.getByRole("button", { name: /new transaction/i }));

    expect(
      screen.getByRole("heading", { name: /create transaction/i }),
    ).toBeInTheDocument();
  });

  it("closes edit dialog when dismissed", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      mockTransactionResponse({ id: "t1", receiptId: "r1", accountId: "a1", amount: 25.50, date: "2024-01-15" }),
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
    await user.click(screen.getByRole("button", { name: /edit/i }));
    expect(screen.getByRole("heading", { name: /edit transaction/i })).toBeInTheDocument();

    await user.keyboard("{Escape}");
    await vi.waitFor(() => {
      expect(screen.queryByRole("heading", { name: /edit transaction/i })).not.toBeInTheDocument();
    });
  });

  it("closes create dialog when Cancel is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<Transactions />);
    await user.click(screen.getByRole("button", { name: /new transaction/i }));
    expect(screen.getByRole("heading", { name: /create transaction/i })).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: /cancel/i }));
    await vi.waitFor(() => {
      expect(screen.queryByRole("heading", { name: /create transaction/i })).not.toBeInTheDocument();
    });
  });

  it("renders NoResults when search returns no matches", async () => {
    const { useTransactions } = await import("@/hooks/useTransactions");
    vi.mocked(useTransactions).mockReturnValue(mockQueryResult({
      data: [mockTransactionResponse({ id: "t1", receiptId: "r1", accountId: "a1", amount: 25.50, date: "2024-01-15" })],
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

    renderWithProviders(<Transactions />);
    expect(screen.getByText(/try fewer keywords/i)).toBeInTheDocument();
  });

  it("opens edit dialog when Edit button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      mockTransactionResponse({ id: "t1", receiptId: "r1", accountId: "a1", amount: 25.50, date: "2024-01-15" }),
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
    await user.click(screen.getByRole("button", { name: /edit/i }));

    expect(
      screen.getByRole("heading", { name: /edit transaction/i }),
    ).toBeInTheDocument();
  });

  it("toggles checkbox selection and shows delete button", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const items = [
      mockTransactionResponse({ id: "t1", receiptId: "r1", accountId: "a1", amount: 25.50, date: "2024-01-15" }),
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

    const { useTransactions } = await import("@/hooks/useTransactions");
    vi.mocked(useTransactions).mockReturnValue(mockQueryResult({
      data: items,
      total: items.length,
      isLoading: false,
    }));

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
      mockTransactionResponse({ id: "t1", receiptId: "r1", accountId: "a1", amount: 25.50, date: "2024-01-15" }),
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

    const { useTransactions } = await import("@/hooks/useTransactions");
    vi.mocked(useTransactions).mockReturnValue(mockQueryResult({
      data: items,
      total: items.length,
      isLoading: false,
    }));

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
      mockTransactionResponse({ id: "t1", receiptId: "r1", accountId: "a1", amount: 25.50, date: "2024-01-15" }),
      mockTransactionResponse({ id: "t2", receiptId: "r2", accountId: "a2", amount: 100.00, date: "2024-01-20" }),
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

    const { useTransactions } = await import("@/hooks/useTransactions");
    vi.mocked(useTransactions).mockReturnValue(mockQueryResult({
      data: items,
      total: items.length,
      isLoading: false,
    }));

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

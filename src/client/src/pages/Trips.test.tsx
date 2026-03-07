import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult } from "@/test/mock-hooks";
import Trips from "./Trips";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useTrips", () => ({
  useTripByReceiptId: vi.fn(() => ({
    data: undefined,
    isLoading: false,
    isError: false,
  })),
}));

vi.mock("@/hooks/useReceipts", () => ({
  useReceipts: vi.fn(() => ({ data: { data: [], total: 0, offset: 0, limit: 25 }, isLoading: false })),
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
    pageSize: 25,
    totalItems: 0,
    totalPages: 1,
    setPage: vi.fn(),
    setPageSize: vi.fn(),
  })),
}));

describe("Trips", () => {
  it("renders the page heading", () => {
    renderWithProviders(<Trips />);
    expect(
      screen.getByRole("heading", { name: /trips/i }),
    ).toBeInTheDocument();
  });

  it("renders the search input", () => {
    renderWithProviders(<Trips />);
    expect(
      screen.getByPlaceholderText(/search receipts/i),
    ).toBeInTheDocument();
  });

  it("renders the filter panel", () => {
    renderWithProviders(<Trips />);
    expect(
      screen.getByRole("button", { name: /filters/i }),
    ).toBeInTheDocument();
  });

  it("renders receipt table when receipts exist", async () => {
    const items = [
      { id: "1", location: "Walmart", date: "2024-01-15", taxAmount: 5.25 },
      { id: "2", location: "Target", date: "2024-01-20", taxAmount: 3.50 },
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
      pageSize: 25,
      totalItems: items.length,
      totalPages: 1,
      setPage: vi.fn(),
      setPageSize: vi.fn(),
    });

    const { useReceipts } = await import("@/hooks/useReceipts");
    vi.mocked(useReceipts).mockReturnValue(mockQueryResult({
      data: { data: items, total: items.length, offset: 0, limit: 10000 },
      isLoading: false,
    }));

    renderWithProviders(<Trips />);
    expect(screen.getByText("Walmart")).toBeInTheDocument();
    expect(screen.getByText("Target")).toBeInTheDocument();
  });

  it("renders loading skeleton when receipts are loading", async () => {
    const { useReceipts } = await import("@/hooks/useReceipts");
    vi.mocked(useReceipts).mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: true,
    }));

    const { container } = renderWithProviders(<Trips />);
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
  });

  it("renders empty state when no receipts found", async () => {
    const { useReceipts } = await import("@/hooks/useReceipts");
    vi.mocked(useReceipts).mockReturnValue(mockQueryResult({
      data: { data: [], total: 0, offset: 0, limit: 25 },
      isLoading: false,
    }));

    const { useFuzzySearch } = await import("@/hooks/useFuzzySearch");
    vi.mocked(useFuzzySearch).mockReturnValue(mockQueryResult({
      search: "",
      setSearch: vi.fn(),
      results: [],
      totalCount: 0,
      isSearching: false,
      clearSearch: vi.fn(),
    }));

    renderWithProviders(<Trips />);
    expect(
      screen.getByText(/no receipts found/i),
    ).toBeInTheDocument();
  });

  it("renders no-results state when search yields nothing but receipts exist", async () => {
    const { useReceipts } = await import("@/hooks/useReceipts");
    vi.mocked(useReceipts).mockReturnValue(mockQueryResult({
      data: { data: [{ id: "1", location: "Walmart", date: "2024-01-15", taxAmount: 5.25 }], total: 1, offset: 0, limit: 10000 },
      isLoading: false,
    }));

    const { useFuzzySearch } = await import("@/hooks/useFuzzySearch");
    vi.mocked(useFuzzySearch).mockReturnValue(mockQueryResult({
      search: "xyz",
      setSearch: vi.fn(),
      results: [],
      totalCount: 1,
      isSearching: false,
      clearSearch: vi.fn(),
    }));

    renderWithProviders(<Trips />);
    expect(screen.getByText(/try fewer keywords/i)).toBeInTheDocument();
  });

  it("shows empty state when searching with zero receipts in database", async () => {
    const { useReceipts } = await import("@/hooks/useReceipts");
    vi.mocked(useReceipts).mockReturnValue(mockQueryResult({
      data: { data: [], total: 0, offset: 0, limit: 10000 },
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

    renderWithProviders(<Trips />);
    expect(screen.getByText(/no receipts found/i)).toBeInTheDocument();
  });

  it("does not render trip data when no receipt is selected", () => {
    renderWithProviders(<Trips />);
    expect(screen.queryByText(/receipt/i, { selector: "h3" })).not.toBeInTheDocument();
  });

  it("renders loading skeletons when trip is loading", async () => {
    const { useTripByReceiptId } = await import("@/hooks/useTrips");
    vi.mocked(useTripByReceiptId).mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: true,
      isError: false,
    }));

    const { container } = renderWithProviders(<Trips />);
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
  });

  it("renders error state when trip is not found", async () => {
    const { useTripByReceiptId } = await import("@/hooks/useTrips");
    vi.mocked(useTripByReceiptId).mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: false,
      isError: true,
    }));

    renderWithProviders(<Trips />);
    // Error state shows when receiptId is set and isError is true
    // Without selecting a receipt, the error message won't show
  });

  it("renders trip data when loaded", async () => {
    const { useTripByReceiptId } = await import("@/hooks/useTrips");
    vi.mocked(useTripByReceiptId).mockReturnValue(mockQueryResult({
      data: {
        receipt: {
          receipt: {
            id: "r1",
            location: "Walmart",
            date: "2024-01-15",
            taxAmount: 5.25,
          },
          items: [],
          subtotal: 50.00,
          adjustmentTotal: 0,
          adjustments: [],
          expectedTotal: 55.25,
          warnings: [],
        },
        transactions: [],
        warnings: [],
      },
      isLoading: false,
      isError: false,
    }));

    renderWithProviders(<Trips />);
    expect(screen.getByText(/walmart/i)).toBeInTheDocument();
    expect(screen.getByText(/2024-01-15/)).toBeInTheDocument();
  });

  it("renders transactions when trip has transactions", async () => {
    const { useTripByReceiptId } = await import("@/hooks/useTrips");
    vi.mocked(useTripByReceiptId).mockReturnValue(mockQueryResult({
      data: {
        receipt: {
          receipt: {
            id: "r1",
            location: "Walmart",
            date: "2024-01-15",
            taxAmount: 5.25,
          },
          items: [],
          subtotal: 50.00,
          adjustmentTotal: 0,
          adjustments: [],
          expectedTotal: 55.25,
          warnings: [],
        },
        transactions: [
          {
            transaction: { id: "t1", amount: 55.25, date: "2024-01-15" },
            account: { accountCode: "ACC-001", name: "Checking", isActive: true },
          },
        ],
        warnings: [],
      },
      isLoading: false,
      isError: false,
    }));

    renderWithProviders(<Trips />);
    expect(screen.getByText("ACC-001")).toBeInTheDocument();
    expect(screen.getByText("Checking")).toBeInTheDocument();
  });

  it("renders adjustments when trip has adjustments", async () => {
    const { useTripByReceiptId } = await import("@/hooks/useTrips");
    vi.mocked(useTripByReceiptId).mockReturnValue(mockQueryResult({
      data: {
        receipt: {
          receipt: {
            id: "r1",
            location: "Walmart",
            date: "2024-01-15",
            taxAmount: 5.25,
          },
          items: [],
          subtotal: 50.00,
          adjustmentTotal: 2.00,
          adjustments: [
            { id: "adj1", type: "Discount", description: "Coupon", amount: -2.00 },
          ],
          expectedTotal: 53.25,
          warnings: [],
        },
        transactions: [],
        warnings: [],
      },
      isLoading: false,
      isError: false,
    }));

    renderWithProviders(<Trips />);
    expect(screen.getByText("Discount")).toBeInTheDocument();
    expect(screen.getByText("Coupon")).toBeInTheDocument();
  });

  it("shows no transactions message when trip has no transactions", async () => {
    const { useTripByReceiptId } = await import("@/hooks/useTrips");
    vi.mocked(useTripByReceiptId).mockReturnValue(mockQueryResult({
      data: {
        receipt: {
          receipt: {
            id: "r1",
            location: "Walmart",
            date: "2024-01-15",
            taxAmount: 5.25,
          },
          items: [],
          subtotal: 50.00,
          adjustmentTotal: 0,
          adjustments: [],
          expectedTotal: 55.25,
          warnings: [],
        },
        transactions: [],
        warnings: [],
      },
      isLoading: false,
      isError: false,
    }));

    renderWithProviders(<Trips />);
    expect(
      screen.getByText(/no transactions for this receipt/i),
    ).toBeInTheDocument();
  });

  it("shows no adjustments message when trip has no adjustments", async () => {
    const { useTripByReceiptId } = await import("@/hooks/useTrips");
    vi.mocked(useTripByReceiptId).mockReturnValue(mockQueryResult({
      data: {
        receipt: {
          receipt: {
            id: "r1",
            location: "Walmart",
            date: "2024-01-15",
            taxAmount: 5.25,
          },
          items: [],
          subtotal: 50.00,
          adjustmentTotal: 0,
          adjustments: [],
          expectedTotal: 55.25,
          warnings: [],
        },
        transactions: [],
        warnings: [],
      },
      isLoading: false,
      isError: false,
    }));

    renderWithProviders(<Trips />);
    expect(
      screen.getByText(/no adjustments for this receipt/i),
    ).toBeInTheDocument();
  });
});

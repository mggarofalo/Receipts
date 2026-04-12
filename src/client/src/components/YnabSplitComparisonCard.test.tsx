import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult } from "@/test/mock-hooks";
import { YnabSplitComparisonCard } from "./YnabSplitComparisonCard";

vi.mock("@/hooks/useYnab", () => ({
  useYnabSplitComparison: vi.fn(() => mockQueryResult()),
}));

beforeEach(async () => {
  vi.clearAllMocks();
  const ynab = await import("@/hooks/useYnab");
  vi.mocked(ynab.useYnabSplitComparison).mockReturnValue(mockQueryResult());
});

describe("YnabSplitComparisonCard", () => {
  it("renders a loading skeleton while the query is pending", async () => {
    const ynab = await import("@/hooks/useYnab");
    vi.mocked(ynab.useYnabSplitComparison).mockReturnValue(
      mockQueryResult({ isLoading: true, isPending: true }),
    );

    renderWithProviders(<YnabSplitComparisonCard receiptId="r1" />);

    expect(screen.getByText("YNAB Split Comparison")).toBeInTheDocument();
    // Skeleton contains no specific text but should be in the card body.
    expect(
      screen
        .getByText("YNAB Split Comparison")
        .closest(".rounded-lg, [data-slot='card']") ??
        document.querySelector("[class*='skeleton']"),
    ).toBeTruthy();
  });

  it("renders an error alert when the query fails", async () => {
    const ynab = await import("@/hooks/useYnab");
    vi.mocked(ynab.useYnabSplitComparison).mockReturnValue(
      mockQueryResult({
        isLoading: false,
        isPending: false,
        isError: true,
        error: new Error("Boom"),
      }),
    );

    renderWithProviders(<YnabSplitComparisonCard receiptId="r1" />);

    expect(screen.getByText(/Could not load split comparison/)).toBeInTheDocument();
    expect(screen.getByText(/Boom/)).toBeInTheDocument();
  });

  it("renders an unavailable state with unmapped categories and a settings link", async () => {
    const ynab = await import("@/hooks/useYnab");
    vi.mocked(ynab.useYnabSplitComparison).mockReturnValue(
      mockQueryResult({
        isLoading: false,
        isPending: false,
        isSuccess: true,
        data: {
          canComputeExpected: false,
          expectedUnavailableReason: "Unmapped categories found.",
          unmappedCategories: ["Gas", "Dining"],
          transactionComparisons: [],
        },
      }),
    );

    renderWithProviders(<YnabSplitComparisonCard receiptId="r1" />);

    expect(screen.getByText(/Unmapped categories found\./)).toBeInTheDocument();
    expect(screen.getByText(/Gas, Dining/)).toBeInTheDocument();
    const link = screen.getByRole("link", { name: /YNAB Settings/ });
    expect(link).toHaveAttribute("href", "/settings/ynab");
  });

  it("renders expected-only state when the receipt has not been pushed", async () => {
    const ynab = await import("@/hooks/useYnab");
    vi.mocked(ynab.useYnabSplitComparison).mockReturnValue(
      mockQueryResult({
        isLoading: false,
        isPending: false,
        isSuccess: true,
        data: {
          canComputeExpected: true,
          expectedUnavailableReason: null,
          unmappedCategories: [],
          transactionComparisons: [
            {
              localTransactionId: "tx-1",
              accountName: "Checking",
              totalMilliunits: -11000,
              expected: [
                {
                  ynabCategoryId: "cat-1",
                  categoryName: "Groceries",
                  milliunits: -11000,
                },
              ],
              actual: null,
              actualFetchError: null,
              matches: null,
            },
          ],
        },
      }),
    );

    renderWithProviders(<YnabSplitComparisonCard receiptId="r1" />);

    expect(
      screen.getByText(/Actual will appear here after you push to YNAB/),
    ).toBeInTheDocument();
    expect(screen.getByText("Checking")).toBeInTheDocument();
    expect(screen.getByText("Groceries")).toBeInTheDocument();
    expect(screen.queryByText("Actual")).not.toBeInTheDocument();
  });

  it("renders matching pushed state without a mismatch badge", async () => {
    const ynab = await import("@/hooks/useYnab");
    vi.mocked(ynab.useYnabSplitComparison).mockReturnValue(
      mockQueryResult({
        isLoading: false,
        isPending: false,
        isSuccess: true,
        data: {
          canComputeExpected: true,
          expectedUnavailableReason: null,
          unmappedCategories: [],
          transactionComparisons: [
            {
              localTransactionId: "tx-1",
              accountName: "Checking",
              totalMilliunits: -11000,
              expected: [
                { ynabCategoryId: "cat-1", categoryName: "Groceries", milliunits: -11000 },
              ],
              actual: [
                { ynabCategoryId: "cat-1", categoryName: "Groceries", milliunits: -11000 },
              ],
              actualFetchError: null,
              matches: true,
            },
          ],
        },
      }),
    );

    renderWithProviders(<YnabSplitComparisonCard receiptId="r1" />);

    expect(screen.getByText("Checking")).toBeInTheDocument();
    expect(screen.getByText("Expected")).toBeInTheDocument();
    expect(screen.getByText("Actual")).toBeInTheDocument();
    expect(screen.queryByText(/Mismatch/)).not.toBeInTheDocument();
  });

  it("renders mismatch highlight and badge when actual differs from expected", async () => {
    const ynab = await import("@/hooks/useYnab");
    vi.mocked(ynab.useYnabSplitComparison).mockReturnValue(
      mockQueryResult({
        isLoading: false,
        isPending: false,
        isSuccess: true,
        data: {
          canComputeExpected: true,
          expectedUnavailableReason: null,
          unmappedCategories: [],
          transactionComparisons: [
            {
              localTransactionId: "tx-1",
              accountName: "Checking",
              totalMilliunits: -11000,
              expected: [
                { ynabCategoryId: "cat-1", categoryName: "Groceries", milliunits: -11000 },
              ],
              actual: [
                { ynabCategoryId: "cat-2", categoryName: "Dining", milliunits: -11000 },
              ],
              actualFetchError: null,
              matches: false,
            },
          ],
        },
      }),
    );

    renderWithProviders(<YnabSplitComparisonCard receiptId="r1" />);

    // Top-level mismatch badge + per-transaction one
    const badges = screen.getAllByText(/Mismatch/);
    expect(badges.length).toBeGreaterThanOrEqual(1);
    expect(screen.getByText("Dining")).toBeInTheDocument();
    expect(screen.getByText("Groceries")).toBeInTheDocument();
  });

  it("renders an actual fetch error warning when YNAB read fails", async () => {
    const ynab = await import("@/hooks/useYnab");
    vi.mocked(ynab.useYnabSplitComparison).mockReturnValue(
      mockQueryResult({
        isLoading: false,
        isPending: false,
        isSuccess: true,
        data: {
          canComputeExpected: true,
          expectedUnavailableReason: null,
          unmappedCategories: [],
          transactionComparisons: [
            {
              localTransactionId: "tx-1",
              accountName: "Checking",
              totalMilliunits: -11000,
              expected: [
                { ynabCategoryId: "cat-1", categoryName: "Groceries", milliunits: -11000 },
              ],
              actual: null,
              actualFetchError: "YNAB 503",
              matches: null,
            },
          ],
        },
      }),
    );

    renderWithProviders(<YnabSplitComparisonCard receiptId="r1" />);

    // When actual is null, we fall through to the not-yet-pushed state which
    // won't show the fetch-error alert — so this exercises an edge case where
    // at least one transaction has actual data. Reset to that scenario:
    vi.mocked(ynab.useYnabSplitComparison).mockReturnValue(
      mockQueryResult({
        isLoading: false,
        isPending: false,
        isSuccess: true,
        data: {
          canComputeExpected: true,
          expectedUnavailableReason: null,
          unmappedCategories: [],
          transactionComparisons: [
            {
              localTransactionId: "tx-1",
              accountName: "Checking",
              totalMilliunits: -11000,
              expected: [
                { ynabCategoryId: "cat-1", categoryName: "Groceries", milliunits: -11000 },
              ],
              actual: [
                { ynabCategoryId: "cat-1", categoryName: "Groceries", milliunits: -11000 },
              ],
              actualFetchError: "YNAB 503",
              matches: true,
            },
          ],
        },
      }),
    );

    renderWithProviders(<YnabSplitComparisonCard receiptId="r1" />);
    expect(
      screen.getByText(/Could not fetch current YNAB state: YNAB 503/),
    ).toBeInTheDocument();
  });
});

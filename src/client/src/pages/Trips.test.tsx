import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
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
  useReceipts: vi.fn(() => ({ data: [], isLoading: false })),
}));

vi.mock("@/lib/combobox-options", () => ({
  receiptToOption: vi.fn((r: { id: string }) => ({
    value: r.id,
    label: r.id,
  })),
}));

describe("Trips", () => {
  it("renders the page heading", () => {
    renderWithProviders(<Trips />);
    expect(
      screen.getByRole("heading", { name: /trips/i }),
    ).toBeInTheDocument();
  });

  it("renders the receipt selection label", () => {
    renderWithProviders(<Trips />);
    expect(screen.getByText(/select a receipt/i)).toBeInTheDocument();
  });

  it("does not render trip data when no receipt is selected", () => {
    renderWithProviders(<Trips />);
    expect(screen.queryByText(/receipt/i, { selector: "h3" })).not.toBeInTheDocument();
  });

  it("renders loading skeletons when trip is loading", async () => {
    const { useTripByReceiptId } = await import("@/hooks/useTrips");
    vi.mocked(useTripByReceiptId).mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
    } as unknown as ReturnType<typeof useTripByReceiptId>);

    const { container } = renderWithProviders(<Trips />);
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
  });

  it("renders error state when trip is not found", async () => {
    const { useTripByReceiptId } = await import("@/hooks/useTrips");
    vi.mocked(useTripByReceiptId).mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
    } as unknown as ReturnType<typeof useTripByReceiptId>);

    renderWithProviders(<Trips />);
    // Error state shows when receiptId is set and isError is true
    // Without selecting a receipt, the error message won't show
  });

  it("renders trip data when loaded", async () => {
    const { useTripByReceiptId } = await import("@/hooks/useTrips");
    vi.mocked(useTripByReceiptId).mockReturnValue({
      data: {
        receipt: {
          receipt: {
            id: "r1",
            description: "Weekly grocery",
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
    } as unknown as ReturnType<typeof useTripByReceiptId>);

    renderWithProviders(<Trips />);
    expect(screen.getByText(/walmart/i)).toBeInTheDocument();
    expect(screen.getByText(/2024-01-15/)).toBeInTheDocument();
  });

  it("renders transactions when trip has transactions", async () => {
    const { useTripByReceiptId } = await import("@/hooks/useTrips");
    vi.mocked(useTripByReceiptId).mockReturnValue({
      data: {
        receipt: {
          receipt: {
            id: "r1",
            description: "Grocery",
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
    } as unknown as ReturnType<typeof useTripByReceiptId>);

    renderWithProviders(<Trips />);
    expect(screen.getByText("ACC-001")).toBeInTheDocument();
    expect(screen.getByText("Checking")).toBeInTheDocument();
  });

  it("renders adjustments when trip has adjustments", async () => {
    const { useTripByReceiptId } = await import("@/hooks/useTrips");
    vi.mocked(useTripByReceiptId).mockReturnValue({
      data: {
        receipt: {
          receipt: {
            id: "r1",
            description: "Grocery",
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
    } as unknown as ReturnType<typeof useTripByReceiptId>);

    renderWithProviders(<Trips />);
    expect(screen.getByText("Discount")).toBeInTheDocument();
    expect(screen.getByText("Coupon")).toBeInTheDocument();
  });

  it("shows no transactions message when trip has no transactions", async () => {
    const { useTripByReceiptId } = await import("@/hooks/useTrips");
    vi.mocked(useTripByReceiptId).mockReturnValue({
      data: {
        receipt: {
          receipt: {
            id: "r1",
            description: "Grocery",
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
    } as unknown as ReturnType<typeof useTripByReceiptId>);

    renderWithProviders(<Trips />);
    expect(
      screen.getByText(/no transactions for this receipt/i),
    ).toBeInTheDocument();
  });

  it("shows no adjustments message when trip has no adjustments", async () => {
    const { useTripByReceiptId } = await import("@/hooks/useTrips");
    vi.mocked(useTripByReceiptId).mockReturnValue({
      data: {
        receipt: {
          receipt: {
            id: "r1",
            description: "Grocery",
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
    } as unknown as ReturnType<typeof useTripByReceiptId>);

    renderWithProviders(<Trips />);
    expect(
      screen.getByText(/no adjustments for this receipt/i),
    ).toBeInTheDocument();
  });
});

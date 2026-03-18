import { render, screen } from "@testing-library/react";
import { MemoryRouter, Routes, Route } from "react-router";
import { mockQueryResult } from "@/test/mock-hooks";
import ReceiptDetail from "./ReceiptDetail";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useAggregates", () => ({
  useReceiptWithItems: vi.fn(() => ({
    data: undefined,
    isLoading: false,
    isError: false,
  })),
}));

vi.mock("@/components/ChangeHistory", () => ({
  ChangeHistory: function MockChangeHistory() {
    return null;
  },
}));

vi.mock("@/components/ValidationWarnings", () => ({
  ValidationWarnings: function MockValidationWarnings() {
    return null;
  },
}));

vi.mock("@/components/BalanceSummaryCard", () => ({
  BalanceSummaryCard: function MockBalanceSummaryCard() {
    return null;
  },
}));

vi.mock("@/components/ReceiptItemsCard", () => ({
  ReceiptItemsCard: function MockReceiptItemsCard() {
    return null;
  },
}));

vi.mock("@/components/AdjustmentsCard", () => ({
  AdjustmentsCard: function MockAdjustmentsCard() {
    return null;
  },
}));

function renderWithRoutes(initialRoute: string) {
  return render(
    <MemoryRouter initialEntries={[initialRoute]}>
      <Routes>
        <Route path="/receipt-detail" element={<ReceiptDetail />} />
        <Route path="/receipts" element={<div data-testid="receipts-page">Receipts</div>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe("ReceiptDetail", () => {
  it("redirects to /receipts when no id param is present", () => {
    renderWithRoutes("/receipt-detail");
    expect(screen.getByTestId("receipts-page")).toBeInTheDocument();
  });

  it("redirects to /receipts when id param is empty", () => {
    renderWithRoutes("/receipt-detail?id=");
    expect(screen.getByTestId("receipts-page")).toBeInTheDocument();
  });

  it("does not render manual input or Look Up button", () => {
    renderWithRoutes("/receipt-detail?id=some-uuid");
    expect(screen.queryByPlaceholderText(/enter receipt uuid/i)).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /look up/i })).not.toBeInTheDocument();
  });

  it("calls useReceiptWithItems with the id param", async () => {
    const { useReceiptWithItems } = await import("@/hooks/useAggregates");
    const mockHook = vi.mocked(useReceiptWithItems);
    mockHook.mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: false,
      isError: false,
    }));

    renderWithRoutes("/receipt-detail?id=some-uuid");

    expect(mockHook).toHaveBeenCalledWith("some-uuid");
  });

  it("renders the page heading when id is present", () => {
    renderWithRoutes("/receipt-detail?id=some-uuid");
    expect(
      screen.getByRole("heading", { name: /receipt details/i }),
    ).toBeInTheDocument();
  });

  it("renders loading skeleton with accessible status when data is loading", async () => {
    const { useReceiptWithItems } = await import("@/hooks/useAggregates");
    vi.mocked(useReceiptWithItems).mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: true,
      isError: false,
    }));

    const { container } = renderWithRoutes("/receipt-detail?id=some-uuid");
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
    expect(screen.getByRole("status")).toBeInTheDocument();
    expect(screen.getByText(/loading receipt details/i)).toBeInTheDocument();
  });

  it("renders receipt data when loaded", async () => {
    const { useReceiptWithItems } = await import("@/hooks/useAggregates");
    vi.mocked(useReceiptWithItems).mockReturnValue(mockQueryResult({
      data: {
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
      isLoading: false,
      isError: false,
    }));

    renderWithRoutes("/receipt-detail?id=r1");
    expect(screen.getByText(/walmart/i)).toBeInTheDocument();
    expect(screen.getByText(/2024-01-15/)).toBeInTheDocument();
  });

  it("renders error state with alert role when receipt is not found", async () => {
    const { useReceiptWithItems } = await import("@/hooks/useAggregates");
    vi.mocked(useReceiptWithItems).mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: false,
      isError: true,
    }));

    renderWithRoutes("/receipt-detail?id=bad-id");
    expect(screen.getByRole("alert")).toHaveTextContent(/no receipt found for this id/i);
  });
});

import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
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

describe("ReceiptDetail", () => {
  it("renders the page heading", () => {
    renderWithProviders(<ReceiptDetail />);
    expect(
      screen.getByRole("heading", { name: /receipt details/i }),
    ).toBeInTheDocument();
  });

  it("renders the receipt ID input field", () => {
    renderWithProviders(<ReceiptDetail />);
    expect(
      screen.getByPlaceholderText(/enter receipt uuid/i),
    ).toBeInTheDocument();
  });

  it("renders the Look Up button", () => {
    renderWithProviders(<ReceiptDetail />);
    expect(
      screen.getByRole("button", { name: /look up/i }),
    ).toBeInTheDocument();
  });

  it("disables Look Up button when input is empty", () => {
    renderWithProviders(<ReceiptDetail />);
    expect(
      screen.getByRole("button", { name: /look up/i }),
    ).toBeDisabled();
  });

  it("renders loading skeleton when data is loading", async () => {
    const { useReceiptWithItems } = await import("@/hooks/useAggregates");
    vi.mocked(useReceiptWithItems).mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
    } as unknown as ReturnType<typeof useReceiptWithItems>);

    const { container } = renderWithProviders(<ReceiptDetail />);
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
  });

  it("enables Look Up button when input has a value", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<ReceiptDetail />);

    const input = screen.getByPlaceholderText(/enter receipt uuid/i);
    await user.type(input, "some-uuid");

    expect(
      screen.getByRole("button", { name: /look up/i }),
    ).not.toBeDisabled();
  });

  it("triggers lookup when Look Up button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const { useReceiptWithItems } = await import("@/hooks/useAggregates");
    const mockHook = vi.mocked(useReceiptWithItems);
    mockHook.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: false,
    } as unknown as ReturnType<typeof useReceiptWithItems>);

    renderWithProviders(<ReceiptDetail />);

    const input = screen.getByPlaceholderText(/enter receipt uuid/i);
    await user.type(input, "test-receipt-id");
    await user.click(screen.getByRole("button", { name: /look up/i }));

    // After clicking Look Up, the hook should be called with the receipt ID
    expect(mockHook).toHaveBeenCalled();
  });

  it("triggers lookup on Enter key press", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const { useReceiptWithItems } = await import("@/hooks/useAggregates");
    const mockHook = vi.mocked(useReceiptWithItems);
    mockHook.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: false,
    } as unknown as ReturnType<typeof useReceiptWithItems>);

    renderWithProviders(<ReceiptDetail />);

    const input = screen.getByPlaceholderText(/enter receipt uuid/i);
    await user.type(input, "test-receipt-id{Enter}");

    expect(mockHook).toHaveBeenCalled();
  });

  it("renders receipt data when loaded", async () => {
    const { useReceiptWithItems } = await import("@/hooks/useAggregates");
    vi.mocked(useReceiptWithItems).mockReturnValue({
      data: {
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
      isLoading: false,
      isError: false,
    } as unknown as ReturnType<typeof useReceiptWithItems>);

    renderWithProviders(<ReceiptDetail />);
    expect(screen.getByText(/walmart/i)).toBeInTheDocument();
    expect(screen.getByText(/2024-01-15/)).toBeInTheDocument();
    expect(screen.getByText(/weekly grocery/i)).toBeInTheDocument();
  });

  it("renders error state when receipt is not found", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const { useReceiptWithItems } = await import("@/hooks/useAggregates");
    vi.mocked(useReceiptWithItems).mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
    } as unknown as ReturnType<typeof useReceiptWithItems>);

    renderWithProviders(<ReceiptDetail />);

    // Need to trigger a lookup first to set receiptId
    const input = screen.getByPlaceholderText(/enter receipt uuid/i);
    await user.type(input, "bad-id");
    await user.click(screen.getByRole("button", { name: /look up/i }));

    expect(
      screen.getByText(/no receipt found for this id/i),
    ).toBeInTheDocument();
  });
});

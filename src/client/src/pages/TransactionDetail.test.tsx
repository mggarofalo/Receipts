import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import TransactionDetail from "./TransactionDetail";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useAggregates", () => ({
  useTransactionAccount: vi.fn(() => ({
    data: undefined,
    isLoading: false,
    isError: false,
  })),
  useTransactionAccountsByReceiptId: vi.fn(() => ({
    data: undefined,
    isLoading: false,
    isError: false,
  })),
}));

vi.mock("@/hooks/useListKeyboardNav", () => ({
  useListKeyboardNav: vi.fn(() => ({
    focusedId: null,
    setFocusedIndex: vi.fn(),
    tableRef: { current: null },
  })),
}));

vi.mock("@/components/ChangeHistory", () => ({
  ChangeHistory: function MockChangeHistory() {
    return null;
  },
}));

describe("TransactionDetail", () => {
  it("renders the page heading", () => {
    renderWithProviders(<TransactionDetail />);
    expect(
      screen.getByRole("heading", { name: /transaction details/i }),
    ).toBeInTheDocument();
  });

  it("renders lookup mode buttons", () => {
    renderWithProviders(<TransactionDetail />);
    expect(
      screen.getByRole("button", { name: /receipt id/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /transaction id/i }),
    ).toBeInTheDocument();
  });

  it("renders the Look Up button", () => {
    renderWithProviders(<TransactionDetail />);
    expect(
      screen.getByRole("button", { name: /look up/i }),
    ).toBeInTheDocument();
  });

  it("disables Look Up button when input is empty", () => {
    renderWithProviders(<TransactionDetail />);
    expect(
      screen.getByRole("button", { name: /look up/i }),
    ).toBeDisabled();
  });

  it("renders the input field for UUID entry", () => {
    renderWithProviders(<TransactionDetail />);
    expect(
      screen.getByPlaceholderText(/enter receipt uuid/i),
    ).toBeInTheDocument();
  });

  it("enables Look Up button when input has a value", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<TransactionDetail />);

    const input = screen.getByPlaceholderText(/enter receipt uuid/i);
    await user.type(input, "some-uuid");

    expect(
      screen.getByRole("button", { name: /look up/i }),
    ).not.toBeDisabled();
  });

  it("switches lookup mode when Transaction ID button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<TransactionDetail />);

    await user.click(screen.getByRole("button", { name: /transaction id/i }));

    // Now the placeholder should change to transaction UUID
    expect(
      screen.getByPlaceholderText(/enter transaction uuid/i),
    ).toBeInTheDocument();
  });

  it("switches back to receipt mode", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<TransactionDetail />);

    // Switch to transaction mode first
    await user.click(screen.getByRole("button", { name: /transaction id/i }));
    expect(
      screen.getByPlaceholderText(/enter transaction uuid/i),
    ).toBeInTheDocument();

    // Switch back to receipt mode
    await user.click(screen.getByRole("button", { name: /receipt id/i }));
    expect(
      screen.getByPlaceholderText(/enter receipt uuid/i),
    ).toBeInTheDocument();
  });

  it("triggers lookup when Look Up button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const { useTransactionAccountsByReceiptId } = await import("@/hooks/useAggregates");
    const mockHook = vi.mocked(useTransactionAccountsByReceiptId);

    renderWithProviders(<TransactionDetail />);

    const input = screen.getByPlaceholderText(/enter receipt uuid/i);
    await user.type(input, "test-receipt-id");
    await user.click(screen.getByRole("button", { name: /look up/i }));

    expect(mockHook).toHaveBeenCalled();
  });

  it("triggers lookup on Enter key press", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();

    renderWithProviders(<TransactionDetail />);

    const input = screen.getByPlaceholderText(/enter receipt uuid/i);
    await user.type(input, "test-receipt-id{Enter}");

    // The component should have set receiptId
    const { useTransactionAccountsByReceiptId } = await import("@/hooks/useAggregates");
    expect(vi.mocked(useTransactionAccountsByReceiptId)).toHaveBeenCalled();
  });

  it("renders transaction data when loaded via receipt lookup", async () => {
    const { useTransactionAccountsByReceiptId } = await import("@/hooks/useAggregates");
    vi.mocked(useTransactionAccountsByReceiptId).mockReturnValue({
      data: [
        {
          transaction: { id: "t1", amount: 50.00, date: "2024-01-15" },
          account: { id: "a1", accountCode: "ACC-001", name: "Checking", isActive: true },
        },
      ],
      isLoading: false,
      isError: false,
    } as unknown as ReturnType<typeof useTransactionAccountsByReceiptId>);

    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<TransactionDetail />);

    const input = screen.getByPlaceholderText(/enter receipt uuid/i);
    await user.type(input, "r1");
    await user.click(screen.getByRole("button", { name: /look up/i }));

    expect(screen.getByText("ACC-001")).toBeInTheDocument();
    expect(screen.getByText("Checking")).toBeInTheDocument();
    expect(screen.getByText("2024-01-15")).toBeInTheDocument();
  });

  it("renders error state when no data found", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const { useTransactionAccountsByReceiptId } = await import("@/hooks/useAggregates");
    vi.mocked(useTransactionAccountsByReceiptId).mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
    } as unknown as ReturnType<typeof useTransactionAccountsByReceiptId>);

    renderWithProviders(<TransactionDetail />);

    const input = screen.getByPlaceholderText(/enter receipt uuid/i);
    await user.type(input, "bad-id");
    await user.click(screen.getByRole("button", { name: /look up/i }));

    expect(
      screen.getByText(/no transaction-account data found/i),
    ).toBeInTheDocument();
  });
});

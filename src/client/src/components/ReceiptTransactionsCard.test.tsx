import { describe, it, expect, vi } from "vitest";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ReceiptTransactionsCard } from "./ReceiptTransactionsCard";
import { renderWithQueryClient } from "@/test/test-utils";
import { mockMutationResult } from "@/test/mock-hooks";

vi.mock("@/hooks/useAccounts", () => ({
  useAccounts: vi.fn(() => ({
    data: [
      { id: "acc-1", name: "Checking", accountCode: "1001" },
      { id: "acc-2", name: "Savings", accountCode: "2001" },
    ],
    isLoading: false,
  })),
}));

vi.mock("@/hooks/useTransactions", () => ({
  useCreateTransaction: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useUpdateTransaction: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useDeleteTransactions: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

const mockTransactions = [
  {
    transaction: { id: "txn-1", amount: 50.0, date: "2024-01-15" },
    account: {
      id: "acc-1",
      accountCode: "1001",
      name: "Checking",
      isActive: true,
    },
  },
  {
    transaction: { id: "txn-2", amount: 25.5, date: "2024-01-16" },
    account: {
      id: "acc-2",
      accountCode: "2001",
      name: "Savings",
      isActive: false,
    },
  },
];

describe("ReceiptTransactionsCard", () => {
  it("renders empty state when there are no transactions", () => {
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={[]}
        transactionsTotal={0}
      />,
    );
    expect(
      screen.getByText("No transactions for this receipt."),
    ).toBeInTheDocument();
    expect(screen.getByText("Transactions (0)")).toBeInTheDocument();
  });

  it("renders transaction rows with data", () => {
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    expect(screen.getByText("Transactions (2)")).toBeInTheDocument();
    expect(screen.getByText("Checking")).toBeInTheDocument();
    expect(screen.getByText("Savings")).toBeInTheDocument();
    expect(screen.getByText("1001")).toBeInTheDocument();
    expect(screen.getByText("2001")).toBeInTheDocument();
  });

  it("renders table headers when transactions exist", () => {
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    expect(screen.getByText("Amount")).toBeInTheDocument();
    expect(screen.getByText("Date")).toBeInTheDocument();
    expect(screen.getByText("Account Code")).toBeInTheDocument();
    expect(screen.getByText("Account Name")).toBeInTheDocument();
    expect(screen.getByText("Status")).toBeInTheDocument();
    expect(screen.getByText("Actions")).toBeInTheDocument();
  });

  it("renders the transaction total in the footer", () => {
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    expect(screen.getByText("Transaction Total")).toBeInTheDocument();
    expect(screen.getByText("$75.50")).toBeInTheDocument();
  });

  it("renders Add Transaction button", () => {
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    expect(
      screen.getByRole("button", { name: /add transaction/i }),
    ).toBeInTheDocument();
  });

  it("renders edit buttons for each transaction row", () => {
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    expect(editButtons).toHaveLength(2);
  });

  it("renders active/inactive badges for accounts", () => {
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    expect(screen.getByText("Active")).toBeInTheDocument();
    expect(screen.getByText("Inactive")).toBeInTheDocument();
  });

  it("toggles individual row selection checkbox", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    const checkingCheckbox = screen.getByLabelText(
      "Select Checking transaction",
    );
    expect(checkingCheckbox).not.toBeChecked();
    await user.click(checkingCheckbox);
    expect(checkingCheckbox).toBeChecked();
    await user.click(checkingCheckbox);
    expect(checkingCheckbox).not.toBeChecked();
  });

  it("select-all checkbox selects and deselects all transactions", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    const selectAll = screen.getByLabelText("Select all transactions");
    expect(selectAll).not.toBeChecked();
    await user.click(selectAll);
    expect(selectAll).toBeChecked();
    expect(
      screen.getByLabelText("Select Checking transaction"),
    ).toBeChecked();
    expect(
      screen.getByLabelText("Select Savings transaction"),
    ).toBeChecked();
    await user.click(selectAll);
    expect(selectAll).not.toBeChecked();
    expect(
      screen.getByLabelText("Select Checking transaction"),
    ).not.toBeChecked();
    expect(
      screen.getByLabelText("Select Savings transaction"),
    ).not.toBeChecked();
  });

  it("shows Delete button with count when items are selected", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    expect(
      screen.queryByRole("button", { name: /delete/i }),
    ).not.toBeInTheDocument();
    await user.click(
      screen.getByLabelText("Select Checking transaction"),
    );
    expect(
      screen.getByRole("button", { name: /delete \(1\)/i }),
    ).toBeInTheDocument();
  });

  it("opens delete confirmation dialog and calls deleteTransactions.mutate on confirm", async () => {
    const { useDeleteTransactions } = await import(
      "@/hooks/useTransactions"
    );
    const mockDeleteMutate = vi.fn();
    vi.mocked(useDeleteTransactions).mockReturnValue(
      mockMutationResult({
        mutate: mockDeleteMutate,
        isPending: false,
      }),
    );

    const user = userEvent.setup();
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    await user.click(
      screen.getByLabelText("Select Checking transaction"),
    );
    await user.click(
      screen.getByRole("button", { name: /delete \(1\)/i }),
    );
    expect(screen.getByText("Delete Transactions")).toBeInTheDocument();
    expect(
      screen.getByText(
        /are you sure you want to delete 1 transaction/i,
      ),
    ).toBeInTheDocument();
    const confirmDelete = screen.getByRole("button", { name: "Delete" });
    await user.click(confirmDelete);
    expect(mockDeleteMutate).toHaveBeenCalledWith(["txn-1"]);
  });

  it("opens create dialog when Add Transaction is clicked", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    await user.click(
      screen.getByRole("button", { name: /add transaction/i }),
    );
    expect(
      screen.getByText("Add Transaction", { selector: "[id]" }),
    ).toBeInTheDocument();
  });

  it("pre-fills the date field with receiptDate in the create dialog", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        receiptDate="2024-01-15"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    await user.click(
      screen.getByRole("button", { name: /add transaction/i }),
    );
    const dateInput = screen.getByPlaceholderText("MM/DD/YYYY");
    expect(dateInput).toHaveValue("01/15/2024");
  });

  it("opens edit dialog when Edit button is clicked", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    await user.click(editButtons[0]);
    expect(screen.getByText("Edit Transaction")).toBeInTheDocument();
  });

  it("renders create form with submit button in create dialog", async () => {
    const { useCreateTransaction } = await import(
      "@/hooks/useTransactions"
    );
    const mockCreateMutate = vi.fn();
    vi.mocked(useCreateTransaction).mockReturnValue(
      mockMutationResult({
        mutate: mockCreateMutate,
        isPending: false,
      }),
    );

    const user = userEvent.setup();
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    await user.click(
      screen.getByRole("button", { name: /add transaction/i }),
    );
    const submitButton = screen.getByRole("button", {
      name: "Add Transaction",
    });
    expect(submitButton).toBeInTheDocument();
  });

  it("renders edit form with update button in edit dialog", async () => {
    const { useUpdateTransaction } = await import(
      "@/hooks/useTransactions"
    );
    const mockUpdateMutate = vi.fn();
    vi.mocked(useUpdateTransaction).mockReturnValue(
      mockMutationResult({
        mutate: mockUpdateMutate,
        isPending: false,
      }),
    );

    const user = userEvent.setup();
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    await user.click(editButtons[0]);
    const submitButton = screen.getByRole("button", {
      name: "Update Transaction",
    });
    expect(submitButton).toBeInTheDocument();
  });

  it("cancels delete dialog without deleting", async () => {
    const { useDeleteTransactions } = await import(
      "@/hooks/useTransactions"
    );
    const mockDeleteMutate = vi.fn();
    vi.mocked(useDeleteTransactions).mockReturnValue(
      mockMutationResult({
        mutate: mockDeleteMutate,
        isPending: false,
      }),
    );

    const user = userEvent.setup();
    renderWithQueryClient(
      <ReceiptTransactionsCard
        receiptId="receipt-1"
        transactions={mockTransactions}
        transactionsTotal={75.5}
      />,
    );
    await user.click(
      screen.getByLabelText("Select Checking transaction"),
    );
    await user.click(
      screen.getByRole("button", { name: /delete \(1\)/i }),
    );
    await user.click(screen.getByRole("button", { name: "Cancel" }));
    expect(mockDeleteMutate).not.toHaveBeenCalled();
  });
});

import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { TransactionForm } from "./TransactionForm";

vi.mock("@/hooks/useFormShortcuts", () => ({
  useFormShortcuts: vi.fn(),
}));

vi.mock("@/hooks/useReceipts", () => ({
  useReceipts: vi.fn(() => ({
    data: [
      { id: "r-1", description: "Walmart Trip", location: "Walmart", date: "2024-01-15" },
    ],
    isLoading: false,
  })),
}));

vi.mock("@/hooks/useAccounts", () => ({
  useAccounts: vi.fn(() => ({
    data: [
      { id: "a-1", name: "Checking", accountCode: "CHK-001" },
    ],
    isLoading: false,
  })),
}));

vi.mock("@/lib/combobox-options", () => ({
  receiptToOption: vi.fn((r: { id: string; description?: string | null; location: string; date: string }) => ({
    value: r.id,
    label: r.description || r.location,
    sublabel: `${r.location} — ${r.date}`,
  })),
  accountToOption: vi.fn((a: { id: string; name: string; accountCode: string }) => ({
    value: a.id,
    label: a.name,
    sublabel: a.accountCode,
  })),
}));

describe("TransactionForm", () => {
  const defaultProps = {
    mode: "create" as const,
    onSubmit: vi.fn(),
    onCancel: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders in create mode with correct submit button text", () => {
    render(<TransactionForm {...defaultProps} />);

    expect(screen.getByRole("button", { name: /create transaction/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancel/i })).toBeInTheDocument();
    expect(screen.getByText("Receipt")).toBeInTheDocument();
    expect(screen.getByText("Account")).toBeInTheDocument();
    expect(screen.getByText("Amount")).toBeInTheDocument();
    expect(screen.getByText("Date")).toBeInTheDocument();
  });

  it("renders in edit mode with pre-populated fields and correct submit button text", () => {
    render(
      <TransactionForm
        {...defaultProps}
        mode="edit"
        defaultValues={{
          receiptId: "r-1",
          accountId: "a-1",
          amount: 42.50,
          date: "2024-01-15",
        }}
      />,
    );

    expect(screen.getByRole("button", { name: /update transaction/i })).toBeInTheDocument();
  });

  it("shows validation errors when required fields are empty", async () => {
    const user = userEvent.setup();
    render(<TransactionForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /create transaction/i }));

    await waitFor(() => {
      expect(screen.getByText("Receipt is required")).toBeInTheDocument();
      expect(screen.getByText("Account is required")).toBeInTheDocument();
      expect(screen.getByText("Date is required")).toBeInTheDocument();
    });
    expect(defaultProps.onSubmit).not.toHaveBeenCalled();
  });

  it("calls onCancel when cancel button is clicked", async () => {
    const user = userEvent.setup();
    render(<TransactionForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /cancel/i }));

    expect(defaultProps.onCancel).toHaveBeenCalledTimes(1);
  });

  it("disables submit button and shows spinner when isSubmitting is true", () => {
    render(<TransactionForm {...defaultProps} isSubmitting={true} />);

    const submitButton = screen.getByRole("button", { name: /saving/i });
    expect(submitButton).toBeDisabled();
  });

  it("renders receipt and account comboboxes", () => {
    render(<TransactionForm {...defaultProps} />);

    const comboboxes = screen.getAllByRole("combobox");
    expect(comboboxes.length).toBeGreaterThanOrEqual(2);
  });

  it("calls onSubmit with correct data when form is valid", async () => {
    const user = userEvent.setup();
    render(
      <TransactionForm
        {...defaultProps}
        defaultValues={{
          receiptId: "r-1",
          accountId: "a-1",
          amount: 25.00,
          date: "2024-02-01",
        }}
      />,
    );

    await user.click(screen.getByRole("button", { name: /create transaction/i }));

    await waitFor(() => {
      expect(defaultProps.onSubmit).toHaveBeenCalledWith(
        expect.objectContaining({
          receiptId: "r-1",
          accountId: "a-1",
          amount: 25.00,
          date: "2024-02-01",
        }),
        expect.anything(),
      );
    });
  });
});

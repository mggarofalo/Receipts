import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult } from "@/test/mock-hooks";
import "@/test/setup-combobox-polyfills";
import { Step2Transactions } from "./Step2Transactions";

vi.mock("@/hooks/useFormShortcuts", () => ({
  useFormShortcuts: vi.fn(),
}));

vi.mock("@/hooks/useAccounts", () => ({
  useAccounts: vi.fn(() =>
    mockQueryResult({
      data: [
        { id: "acct-1", name: "Checking", accountCode: "CHK" },
        { id: "acct-2", name: "Credit Card", accountCode: "CC" },
      ],
      total: 2,
      isLoading: false,
      isSuccess: true,
    }),
  ),
}));

describe("Step2Transactions", () => {
  const defaultProps = {
    data: [],
    receiptDate: "2024-01-15",
    taxAmount: 5.25,
    onNext: vi.fn(),
    onBack: vi.fn(),
  };

  it("renders the card title", () => {
    renderWithProviders(<Step2Transactions {...defaultProps} />);
    expect(screen.getByText("Transactions")).toBeInTheDocument();
  });

  it("renders the form fields", () => {
    renderWithProviders(<Step2Transactions {...defaultProps} />);
    expect(screen.getByLabelText(/amount/i)).toBeInTheDocument();
    expect(screen.getByText(/^date$/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText("MM/DD/YYYY")).toBeInTheDocument();
  });

  it("renders Back and Next buttons", () => {
    renderWithProviders(<Step2Transactions {...defaultProps} />);
    expect(screen.getByRole("button", { name: /back/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /next/i })).toBeInTheDocument();
  });

  it("Next button is disabled when there are no transactions", () => {
    renderWithProviders(<Step2Transactions {...defaultProps} />);
    expect(screen.getByRole("button", { name: /next/i })).toBeDisabled();
  });

  it("renders the Add Transaction button", () => {
    renderWithProviders(<Step2Transactions {...defaultProps} />);
    expect(
      screen.getByRole("button", { name: /add transaction/i }),
    ).toBeInTheDocument();
  });

  it("calls onBack when Back is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const onBack = vi.fn();
    renderWithProviders(
      <Step2Transactions {...defaultProps} onBack={onBack} />,
    );
    await user.click(screen.getByRole("button", { name: /back/i }));
    expect(onBack).toHaveBeenCalled();
  });

  it("displays tax badge when taxAmount > 0", () => {
    renderWithProviders(<Step2Transactions {...defaultProps} />);
    expect(screen.getByText(/tax from step 1/i)).toBeInTheDocument();
  });

  it("renders existing transactions", () => {
    const data = [
      { id: "1", accountId: "acct-1", amount: 25.5, date: "2024-01-15" },
    ];
    renderWithProviders(<Step2Transactions {...defaultProps} data={data} />);
    expect(screen.getByText("$25.50")).toBeInTheDocument();
  });

  it("submits the form when Enter is pressed in the amount field", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<Step2Transactions {...defaultProps} />);

    // Select account via combobox
    const combobox = screen.getByRole("combobox");
    await user.click(combobox);
    const checkingOption = await screen.findByText("Checking");
    await user.click(checkingOption);

    // Type amount
    const amountInput = screen.getByLabelText(/amount/i);
    await user.click(amountInput);
    await user.type(amountInput, "42.50");

    // Press Enter in the amount field to submit
    await user.keyboard("{Enter}");

    // Verify a new transaction row appears with the account name and formatted amount
    expect(await screen.findByText("Checking")).toBeInTheDocument();
    expect(await screen.findByText("$42.50")).toBeInTheDocument();

    // Verify the amount field was reset (not retaining stale value)
    expect(amountInput).toHaveValue("");
  });

  it("shows zero-total warning when transactions sum to zero with tax > 0", () => {
    const data = [
      { id: "1", accountId: "acct-1", amount: 0, date: "2024-01-15" },
    ];
    renderWithProviders(
      <Step2Transactions {...defaultProps} data={data} taxAmount={5.25} />,
    );
    expect(
      screen.getByText(/transaction total is \$0\.00 but tax is/i),
    ).toBeInTheDocument();
  });

  it("does not show zero-total warning when tax is zero", () => {
    const data = [
      { id: "1", accountId: "acct-1", amount: 0, date: "2024-01-15" },
    ];
    renderWithProviders(
      <Step2Transactions {...defaultProps} data={data} taxAmount={0} />,
    );
    expect(
      screen.queryByText(/transaction total is \$0\.00 but tax is/i),
    ).not.toBeInTheDocument();
  });

  it("focuses the account combobox after adding a transaction", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<Step2Transactions {...defaultProps} />);

    // Select account via combobox
    const combobox = screen.getByRole("combobox");
    await user.click(combobox);
    const checkingOption = await screen.findByText("Checking");
    await user.click(checkingOption);

    // Type amount
    const amountInput = screen.getByLabelText(/amount/i);
    await user.click(amountInput);
    await user.type(amountInput, "10");

    // Press Enter to submit
    await user.keyboard("{Enter}");

    // Wait for the transaction to appear (useEffect fires after re-render)
    expect(await screen.findByText("$10.00")).toBeInTheDocument();

    // The useEffect should have focused the combobox trigger
    const comboboxAfter = screen.getByRole("combobox");
    expect(document.activeElement).toBe(comboboxAfter);
  });
});

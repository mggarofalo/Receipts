import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult } from "@/test/mock-hooks";
import { Step2Transactions } from "./Step2Transactions";

vi.mock("@/hooks/useAccounts", () => ({
  useAccounts: vi.fn(() =>
    mockQueryResult({
      data: {
        data: [
          { id: "acct-1", name: "Checking", accountCode: "CHK" },
          { id: "acct-2", name: "Credit Card", accountCode: "CC" },
        ],
      },
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
});

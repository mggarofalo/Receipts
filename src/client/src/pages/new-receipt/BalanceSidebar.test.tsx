import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderWithProviders } from "@/test/test-utils";
import { BalanceSidebar } from "./BalanceSidebar";

describe("BalanceSidebar", () => {
  const defaultProps = {
    subtotal: 50,
    taxAmount: 5,
    transactionTotal: 55,
    isSubmitting: false,
    onSubmit: vi.fn(),
    onCancel: vi.fn(),
  };

  it("renders balance summary values", () => {
    renderWithProviders(<BalanceSidebar {...defaultProps} />);
    expect(screen.getByText("Balance")).toBeInTheDocument();
    expect(screen.getByText("$50.00")).toBeInTheDocument(); // subtotal
    expect(screen.getByText("$5.00")).toBeInTheDocument(); // tax
  });

  it("shows Balanced badge when balanced", () => {
    renderWithProviders(<BalanceSidebar {...defaultProps} />);
    expect(screen.getByText("Balanced")).toBeInTheDocument();
  });

  it("shows Remaining badge when not balanced", () => {
    renderWithProviders(
      <BalanceSidebar {...defaultProps} transactionTotal={40} />,
    );
    expect(screen.getByText("Remaining: $15.00")).toBeInTheDocument();
  });

  it("enables submit button when balanced", () => {
    renderWithProviders(<BalanceSidebar {...defaultProps} />);
    expect(
      screen.getByRole("button", { name: /submit receipt/i }),
    ).toBeEnabled();
  });

  it("disables submit button when unbalanced", () => {
    renderWithProviders(
      <BalanceSidebar {...defaultProps} transactionTotal={40} />,
    );
    expect(
      screen.getByRole("button", { name: /submit receipt/i }),
    ).toBeDisabled();
  });

  it("disables submit button when submitting", () => {
    renderWithProviders(
      <BalanceSidebar {...defaultProps} isSubmitting={true} />,
    );
    expect(screen.getByText("Submitting...")).toBeDisabled();
  });

  it("calls onSubmit when submit button clicked", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    renderWithProviders(
      <BalanceSidebar {...defaultProps} onSubmit={onSubmit} />,
    );
    await user.click(screen.getByRole("button", { name: /submit receipt/i }));
    expect(onSubmit).toHaveBeenCalled();
  });

  it("calls onCancel when cancel button clicked", async () => {
    const user = userEvent.setup();
    const onCancel = vi.fn();
    renderWithProviders(
      <BalanceSidebar {...defaultProps} onCancel={onCancel} />,
    );
    await user.click(screen.getByRole("button", { name: /cancel/i }));
    expect(onCancel).toHaveBeenCalled();
  });

  it("displays expected total and transaction total", () => {
    renderWithProviders(<BalanceSidebar {...defaultProps} />);
    // Expected total = subtotal + tax = 55
    // Transaction total = 55
    const fiftyFives = screen.getAllByText("$55.00");
    expect(fiftyFives).toHaveLength(2);
  });
});

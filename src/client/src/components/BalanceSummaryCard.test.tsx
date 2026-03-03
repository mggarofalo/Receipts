import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { BalanceSummaryCard } from "./BalanceSummaryCard";

describe("BalanceSummaryCard", () => {
  const defaultProps = {
    subtotal: 100,
    taxAmount: 8.5,
    adjustmentTotal: -2,
    expectedTotal: 106.5,
  };

  it("renders the card title", () => {
    render(<BalanceSummaryCard {...defaultProps} />);
    expect(screen.getByText("Balance Summary")).toBeInTheDocument();
  });

  it("renders formatted currency values for all fields", () => {
    render(<BalanceSummaryCard {...defaultProps} />);
    expect(screen.getByText("$100.00")).toBeInTheDocument();
    expect(screen.getByText("$8.50")).toBeInTheDocument();
    expect(screen.getByText("-$2.00")).toBeInTheDocument();
    expect(screen.getByText("$106.50")).toBeInTheDocument();
  });

  it("renders section labels", () => {
    render(<BalanceSummaryCard {...defaultProps} />);
    expect(screen.getByText("Subtotal")).toBeInTheDocument();
    expect(screen.getByText("Tax")).toBeInTheDocument();
    expect(screen.getByText("Adjustments")).toBeInTheDocument();
    expect(screen.getByText("Expected Total")).toBeInTheDocument();
  });

  it("does not show balance info when showBalance is false", () => {
    render(
      <BalanceSummaryCard
        {...defaultProps}
        transactionsTotal={106.5}
      />,
    );
    expect(screen.queryByText("Balanced")).not.toBeInTheDocument();
    expect(
      screen.queryByText("Transactions Total:"),
    ).not.toBeInTheDocument();
  });

  it("shows Balanced badge when transactions total matches expected", () => {
    render(
      <BalanceSummaryCard
        {...defaultProps}
        transactionsTotal={106.5}
        showBalance
      />,
    );
    expect(screen.getByText("Transactions Total:")).toBeInTheDocument();
    expect(screen.getByText("Balanced")).toBeInTheDocument();
  });

  it("shows Unbalanced badge when transactions total does not match", () => {
    render(
      <BalanceSummaryCard
        {...defaultProps}
        transactionsTotal={90}
        showBalance
      />,
    );
    expect(screen.getByText("Unbalanced")).toBeInTheDocument();
  });
});

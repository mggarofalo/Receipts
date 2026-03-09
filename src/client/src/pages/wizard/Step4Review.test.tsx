import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult } from "@/test/mock-hooks";
import { Step4Review } from "./Step4Review";
import type { WizardState } from "./wizardReducer";

vi.mock("@/hooks/useAccounts", () => ({
  useAccounts: vi.fn(() =>
    mockQueryResult({
      data: {
        data: [
          { id: "acct-1", name: "Checking" },
          { id: "acct-2", name: "Credit Card" },
        ],
      },
      isLoading: false,
      isSuccess: true,
    }),
  ),
}));

const createState = (overrides?: Partial<WizardState>): WizardState => ({
  currentStep: 3,
  receipt: { location: "Walmart", date: "2024-01-15", taxAmount: 5 },
  transactions: [
    { id: "t1", accountId: "acct-1", amount: 50, date: "2024-01-15" },
  ],
  items: [
    {
      id: "i1",
      receiptItemCode: "MILK",
      description: "Milk",
      pricingMode: "quantity",
      quantity: 2,
      unitPrice: 3.99,
      category: "Food",
      subcategory: "Dairy",
    },
  ],
  completedSteps: new Set([0, 1, 2]),
  ...overrides,
});

describe("Step4Review", () => {
  const defaultProps = {
    state: createState(),
    onBack: vi.fn(),
    onEditStep: vi.fn(),
    onSubmit: vi.fn(),
    isSubmitting: false,
  };

  it("renders trip details section", () => {
    renderWithProviders(<Step4Review {...defaultProps} />);
    expect(screen.getByText("Walmart")).toBeInTheDocument();
    // Date appears in both trip details and transactions; just check at least one exists
    expect(screen.getAllByText("2024-01-15").length).toBeGreaterThanOrEqual(1);
  });

  it("renders transactions section", () => {
    renderWithProviders(<Step4Review {...defaultProps} />);
    expect(screen.getByText(/transactions \(1\)/i)).toBeInTheDocument();
    expect(screen.getByText("Checking")).toBeInTheDocument();
  });

  it("renders items section", () => {
    renderWithProviders(<Step4Review {...defaultProps} />);
    expect(screen.getByText(/line items \(1\)/i)).toBeInTheDocument();
    expect(screen.getByText("Milk")).toBeInTheDocument();
    expect(screen.getByText("Food / Dairy")).toBeInTheDocument();
  });

  it("renders balance section", () => {
    renderWithProviders(<Step4Review {...defaultProps} />);
    expect(screen.getByText("Balance")).toBeInTheDocument();
  });

  it("renders Submit Receipt button", () => {
    renderWithProviders(<Step4Review {...defaultProps} />);
    expect(
      screen.getByRole("button", { name: /submit receipt/i }),
    ).toBeInTheDocument();
  });

  it("shows Submitting... when isSubmitting is true", () => {
    renderWithProviders(
      <Step4Review {...defaultProps} isSubmitting={true} />,
    );
    expect(
      screen.getByRole("button", { name: /submitting/i }),
    ).toBeDisabled();
  });

  it("renders Edit buttons for each section", () => {
    renderWithProviders(<Step4Review {...defaultProps} />);
    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    expect(editButtons.length).toBe(3); // Trip Details, Transactions, Line Items
  });

  it("calls onEditStep when Edit button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const onEditStep = vi.fn();
    renderWithProviders(
      <Step4Review {...defaultProps} onEditStep={onEditStep} />,
    );

    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    await user.click(editButtons[0]);
    expect(onEditStep).toHaveBeenCalledWith(0);
  });

  it("calls onBack when Back is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const onBack = vi.fn();
    renderWithProviders(<Step4Review {...defaultProps} onBack={onBack} />);
    await user.click(screen.getByRole("button", { name: /back/i }));
    expect(onBack).toHaveBeenCalled();
  });

  it("calls onSubmit when Submit is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const onSubmit = vi.fn();
    renderWithProviders(<Step4Review {...defaultProps} onSubmit={onSubmit} />);
    await user.click(screen.getByRole("button", { name: /submit receipt/i }));
    expect(onSubmit).toHaveBeenCalled();
  });

  it("shows balanced status when amounts match", () => {
    // items subtotal = 2*3.99 = 7.98, tax = 5, expected = 12.98, txn total = 12.98
    const state = createState({
      transactions: [{ id: "t1", accountId: "acct-1", amount: 12.98, date: "2024-01-15" }],
    });
    renderWithProviders(<Step4Review {...defaultProps} state={state} />);
    expect(screen.getByText("Balanced")).toBeInTheDocument();
  });

  it("shows unbalanced status when amounts do not match", () => {
    renderWithProviders(<Step4Review {...defaultProps} />);
    expect(screen.getByText(/unbalanced/i)).toBeInTheDocument();
  });
});

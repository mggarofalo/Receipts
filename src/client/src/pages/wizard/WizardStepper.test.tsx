import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { WizardStepper } from "./WizardStepper";

describe("WizardStepper", () => {
  const defaultProps = {
    currentStep: 0,
    completedSteps: new Set<number>(),
    onStepClick: vi.fn(),
    canGoToStep: vi.fn(() => false),
  };

  it("renders all step labels", () => {
    renderWithProviders(<WizardStepper {...defaultProps} />);
    expect(screen.getByText("Trip Details")).toBeInTheDocument();
    expect(screen.getByText("Transactions")).toBeInTheDocument();
    expect(screen.getByText("Line Items")).toBeInTheDocument();
    expect(screen.getByText("Review")).toBeInTheDocument();
  });

  it("marks current step with aria-current", () => {
    renderWithProviders(<WizardStepper {...defaultProps} currentStep={1} />);
    const buttons = screen.getAllByRole("button");
    expect(buttons[1]).toHaveAttribute("aria-current", "step");
    expect(buttons[0]).not.toHaveAttribute("aria-current");
  });

  it("disables steps that cannot be navigated to", () => {
    renderWithProviders(<WizardStepper {...defaultProps} />);
    const buttons = screen.getAllByRole("button");
    buttons.forEach((btn) => expect(btn).toBeDisabled());
  });

  it("enables steps that can be navigated to", () => {
    const canGoToStep = vi.fn(() => true);
    renderWithProviders(
      <WizardStepper {...defaultProps} canGoToStep={canGoToStep} />,
    );
    const buttons = screen.getAllByRole("button");
    buttons.forEach((btn) => expect(btn).toBeEnabled());
  });

  it("calls onStepClick when a clickable step is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const onStepClick = vi.fn();
    const canGoToStep = vi.fn(() => true);
    renderWithProviders(
      <WizardStepper
        {...defaultProps}
        onStepClick={onStepClick}
        canGoToStep={canGoToStep}
      />,
    );

    await user.click(screen.getByText("Transactions"));
    expect(onStepClick).toHaveBeenCalledWith(1);
  });

  it("shows check icon for completed steps that are not current", () => {
    const { container } = renderWithProviders(
      <WizardStepper
        {...defaultProps}
        currentStep={1}
        completedSteps={new Set([0])}
      />,
    );
    // The completed-but-not-current step (step 0) should render an SVG check icon
    // The current step (step 1) should render the number "2"
    expect(screen.getByText("2")).toBeInTheDocument();
    // Check icon is an SVG from lucide-react
    const svgs = container.querySelectorAll("svg");
    expect(svgs.length).toBeGreaterThan(0);
  });

  it("renders the progress nav landmark", () => {
    renderWithProviders(<WizardStepper {...defaultProps} />);
    expect(
      screen.getByRole("navigation", { name: /receipt entry progress/i }),
    ).toBeInTheDocument();
  });
});

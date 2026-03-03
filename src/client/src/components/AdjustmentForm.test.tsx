import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AdjustmentForm } from "./AdjustmentForm";

vi.mock("@/hooks/useFormShortcuts", () => ({
  useFormShortcuts: vi.fn(),
}));

describe("AdjustmentForm", () => {
  const defaultProps = {
    mode: "create" as const,
    onSubmit: vi.fn(),
    onCancel: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders in create mode with correct submit button text and fields", () => {
    render(<AdjustmentForm {...defaultProps} />);

    expect(screen.getByText("Type")).toBeInTheDocument();
    expect(screen.getByText("Amount")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /add adjustment/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancel/i })).toBeInTheDocument();
  });

  it("renders in edit mode with pre-populated values and correct submit button text", () => {
    render(
      <AdjustmentForm
        {...defaultProps}
        mode="edit"
        defaultValues={{ type: "tip", amount: 5.00, description: "" }}
      />,
    );

    expect(screen.getByRole("button", { name: /update adjustment/i })).toBeInTheDocument();
  });

  it("shows validation error when type is not selected", async () => {
    const user = userEvent.setup();
    render(<AdjustmentForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /add adjustment/i }));

    await waitFor(() => {
      expect(screen.getByText("Type is required")).toBeInTheDocument();
    });
    expect(defaultProps.onSubmit).not.toHaveBeenCalled();
  });

  it("calls onCancel when cancel button is clicked", async () => {
    const user = userEvent.setup();
    render(<AdjustmentForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /cancel/i }));

    expect(defaultProps.onCancel).toHaveBeenCalledTimes(1);
  });

  it("disables submit button and shows spinner when isSubmitting is true", () => {
    render(<AdjustmentForm {...defaultProps} isSubmitting={true} />);

    const submitButton = screen.getByRole("button", { name: /saving/i });
    expect(submitButton).toBeDisabled();
  });

  it("displays server errors when provided", () => {
    render(
      <AdjustmentForm
        {...defaultProps}
        serverErrors={{ type: "Invalid adjustment type", amount: "Amount too large" }}
      />,
    );

    expect(screen.getByText("Invalid adjustment type")).toBeInTheDocument();
    expect(screen.getByText("Amount too large")).toBeInTheDocument();
  });

  it("calls onSubmit with correct data when form has valid non-other type", async () => {
    const user = userEvent.setup();
    render(
      <AdjustmentForm
        {...defaultProps}
        defaultValues={{ type: "tip", amount: 10.00, description: "" }}
      />,
    );

    await user.click(screen.getByRole("button", { name: /add adjustment/i }));

    await waitFor(() => {
      expect(defaultProps.onSubmit).toHaveBeenCalledWith(
        expect.objectContaining({ type: "tip", amount: 10.00 }),
        expect.anything(),
      );
    });
  });

  it("does not show description field when type is not 'other'", () => {
    render(
      <AdjustmentForm
        {...defaultProps}
        defaultValues={{ type: "tip", amount: 0, description: "" }}
      />,
    );

    expect(screen.queryByLabelText("Description")).not.toBeInTheDocument();
  });
});

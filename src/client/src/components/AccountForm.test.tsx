import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AccountForm } from "./AccountForm";

vi.mock("@/hooks/useFormShortcuts", () => ({
  useFormShortcuts: vi.fn(),
}));

describe("AccountForm", () => {
  const defaultProps = {
    mode: "create" as const,
    onSubmit: vi.fn(),
    onCancel: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders in create mode with empty fields and correct submit button text", () => {
    render(<AccountForm {...defaultProps} />);

    expect(screen.getByLabelText(/^Name/)).toHaveValue("");
    expect(screen.getByRole("button", { name: /create account/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancel/i })).toBeInTheDocument();
  });

  it("renders in edit mode with pre-populated fields and correct submit button text", () => {
    render(
      <AccountForm
        {...defaultProps}
        mode="edit"
        defaultValues={{ name: "Apple Card", isActive: false }}
      />,
    );

    expect(screen.getByLabelText(/^Name/)).toHaveValue("Apple Card");
    expect(screen.getByRole("checkbox")).not.toBeChecked();
    expect(screen.getByRole("button", { name: /update account/i })).toBeInTheDocument();
  });

  it("calls onSubmit with correct data when form is valid", async () => {
    const user = userEvent.setup();
    render(<AccountForm {...defaultProps} />);

    await user.type(screen.getByLabelText(/^Name/), "Chase Sapphire");
    await user.click(screen.getByRole("button", { name: /create account/i }));

    await waitFor(() => {
      expect(defaultProps.onSubmit).toHaveBeenCalledWith(
        { name: "Chase Sapphire", isActive: true },
        expect.anything(),
      );
    });
  });

  it("shows validation error when name is empty", async () => {
    const user = userEvent.setup();
    render(<AccountForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /create account/i }));

    await waitFor(() => {
      expect(screen.getByText("Name is required")).toBeInTheDocument();
    });
    expect(defaultProps.onSubmit).not.toHaveBeenCalled();
  });

  it("calls onCancel when cancel button is clicked", async () => {
    const user = userEvent.setup();
    render(<AccountForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /cancel/i }));

    expect(defaultProps.onCancel).toHaveBeenCalledTimes(1);
  });

  it("disables submit button and shows loading label when isSubmitting is true", () => {
    render(<AccountForm {...defaultProps} isSubmitting={true} />);

    const submitButton = screen.getByRole("button", { name: /saving/i });
    expect(submitButton).toBeDisabled();
  });

  it("renders the Active checkbox checked by default in create mode", () => {
    render(<AccountForm {...defaultProps} />);

    expect(screen.getByRole("checkbox")).toBeChecked();
  });

  it("shows delete button in edit mode for admin users only", () => {
    const { rerender } = render(
      <AccountForm
        {...defaultProps}
        mode="edit"
        defaultValues={{ name: "Apple Card", isActive: true }}
        isAdmin={false}
        onDelete={vi.fn()}
      />,
    );

    expect(
      screen.queryByRole("button", { name: /delete/i }),
    ).not.toBeInTheDocument();

    rerender(
      <AccountForm
        {...defaultProps}
        mode="edit"
        defaultValues={{ name: "Apple Card", isActive: true }}
        isAdmin={true}
        onDelete={vi.fn()}
      />,
    );

    expect(screen.getByRole("button", { name: /delete/i })).toBeInTheDocument();
  });

  it("calls onDelete when delete is confirmed", async () => {
    const onDelete = vi.fn();
    const user = userEvent.setup();
    render(
      <AccountForm
        {...defaultProps}
        mode="edit"
        defaultValues={{ name: "Apple Card", isActive: true }}
        isAdmin={true}
        onDelete={onDelete}
      />,
    );

    await user.click(screen.getByRole("button", { name: /delete/i }));

    await waitFor(() => {
      expect(screen.getByText("Delete Account?")).toBeInTheDocument();
    });

    const confirmButtons = screen.getAllByRole("button", { name: /delete/i });
    const confirmButton = confirmButtons[confirmButtons.length - 1];
    await user.click(confirmButton);

    expect(onDelete).toHaveBeenCalledTimes(1);
  });
});

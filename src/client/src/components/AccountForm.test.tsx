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

    expect(screen.getByLabelText("Account Code")).toHaveValue("");
    expect(screen.getByLabelText("Name")).toHaveValue("");
    expect(screen.getByRole("button", { name: /create account/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancel/i })).toBeInTheDocument();
  });

  it("renders in edit mode with pre-populated fields and correct submit button text", () => {
    render(
      <AccountForm
        {...defaultProps}
        mode="edit"
        defaultValues={{ accountCode: "ACC-001", name: "Checking", isActive: false }}
      />,
    );

    expect(screen.getByLabelText("Account Code")).toHaveValue("ACC-001");
    expect(screen.getByLabelText("Name")).toHaveValue("Checking");
    expect(screen.getByRole("checkbox")).not.toBeChecked();
    expect(screen.getByRole("button", { name: /update account/i })).toBeInTheDocument();
  });

  it("calls onSubmit with correct data when form is valid", async () => {
    const user = userEvent.setup();
    render(<AccountForm {...defaultProps} />);

    await user.type(screen.getByLabelText("Account Code"), "ACC-002");
    await user.type(screen.getByLabelText("Name"), "Savings");
    await user.click(screen.getByRole("button", { name: /create account/i }));

    await waitFor(() => {
      expect(defaultProps.onSubmit).toHaveBeenCalledWith(
        { accountCode: "ACC-002", name: "Savings", isActive: true },
        expect.anything(),
      );
    });
  });

  it("shows validation errors when required fields are empty", async () => {
    const user = userEvent.setup();
    render(<AccountForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /create account/i }));

    await waitFor(() => {
      expect(screen.getByText("Account code is required")).toBeInTheDocument();
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

  it("disables submit button and shows spinner when isSubmitting is true", () => {
    render(<AccountForm {...defaultProps} isSubmitting={true} />);

    const submitButton = screen.getByRole("button", { name: /saving/i });
    expect(submitButton).toBeDisabled();
  });

  it("renders the Active checkbox checked by default in create mode", () => {
    render(<AccountForm {...defaultProps} />);

    expect(screen.getByRole("checkbox")).toBeChecked();
  });
});

import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { CategoryForm } from "./CategoryForm";

vi.mock("@/hooks/useFormShortcuts", () => ({
  useFormShortcuts: vi.fn(),
}));

describe("CategoryForm", () => {
  const defaultProps = {
    mode: "create" as const,
    onSubmit: vi.fn(),
    onCancel: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders in create mode with empty fields and correct submit button text", () => {
    render(<CategoryForm {...defaultProps} />);

    expect(screen.getByLabelText("Name")).toHaveValue("");
    expect(screen.getByLabelText("Description (optional)")).toHaveValue("");
    expect(screen.getByRole("button", { name: /create category/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancel/i })).toBeInTheDocument();
  });

  it("renders in edit mode with pre-populated fields and correct submit button text", () => {
    render(
      <CategoryForm
        {...defaultProps}
        mode="edit"
        defaultValues={{ name: "Groceries", description: "Food items", isActive: true }}
      />,
    );

    expect(screen.getByLabelText("Name")).toHaveValue("Groceries");
    expect(screen.getByLabelText("Description (optional)")).toHaveValue("Food items");
    expect(screen.getByRole("button", { name: /update category/i })).toBeInTheDocument();
  });

  it("calls onSubmit with correct data when form is valid", async () => {
    const user = userEvent.setup();
    render(<CategoryForm {...defaultProps} />);

    await user.type(screen.getByLabelText("Name"), "Utilities");
    await user.type(screen.getByLabelText("Description (optional)"), "Monthly bills");
    await user.click(screen.getByRole("button", { name: /create category/i }));

    await waitFor(() => {
      expect(defaultProps.onSubmit).toHaveBeenCalledWith(
        { name: "Utilities", description: "Monthly bills", isActive: true },
        expect.anything(),
      );
    });
  });

  it("shows validation error when name is empty", async () => {
    const user = userEvent.setup();
    render(<CategoryForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /create category/i }));

    await waitFor(() => {
      expect(screen.getByText("Name is required")).toBeInTheDocument();
    });
    expect(defaultProps.onSubmit).not.toHaveBeenCalled();
  });

  it("calls onCancel when cancel button is clicked", async () => {
    const user = userEvent.setup();
    render(<CategoryForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /cancel/i }));

    expect(defaultProps.onCancel).toHaveBeenCalledTimes(1);
  });

  it("disables submit button and shows spinner when isSubmitting is true", () => {
    render(<CategoryForm {...defaultProps} isSubmitting={true} />);

    const submitButton = screen.getByRole("button", { name: /saving/i });
    expect(submitButton).toBeDisabled();
  });

  it("allows submission with only the required name field", async () => {
    const user = userEvent.setup();
    render(<CategoryForm {...defaultProps} />);

    await user.type(screen.getByLabelText("Name"), "Transport");
    await user.click(screen.getByRole("button", { name: /create category/i }));

    await waitFor(() => {
      expect(defaultProps.onSubmit).toHaveBeenCalledWith(
        expect.objectContaining({ name: "Transport" }),
        expect.anything(),
      );
    });
  });

  it("does not show delete button in create mode", () => {
    render(
      <CategoryForm
        {...defaultProps}
        isAdmin={true}
        onDelete={vi.fn()}
      />,
    );

    expect(screen.queryByRole("button", { name: /delete/i })).not.toBeInTheDocument();
  });

  it("does not show delete button in edit mode for non-admin users", () => {
    render(
      <CategoryForm
        {...defaultProps}
        mode="edit"
        defaultValues={{ name: "Groceries", description: "Food items", isActive: true }}
        isAdmin={false}
        onDelete={vi.fn()}
      />,
    );

    expect(screen.queryByRole("button", { name: /delete/i })).not.toBeInTheDocument();
  });

  it("shows delete button in edit mode for admin users", () => {
    render(
      <CategoryForm
        {...defaultProps}
        mode="edit"
        defaultValues={{ name: "Groceries", description: "Food items", isActive: true }}
        isAdmin={true}
        onDelete={vi.fn()}
      />,
    );

    expect(screen.getByRole("button", { name: /delete/i })).toBeInTheDocument();
  });

  it("shows confirmation dialog when delete button is clicked", async () => {
    const user = userEvent.setup();
    render(
      <CategoryForm
        {...defaultProps}
        mode="edit"
        defaultValues={{ name: "Groceries", description: "Food items", isActive: true }}
        isAdmin={true}
        onDelete={vi.fn()}
      />,
    );

    await user.click(screen.getByRole("button", { name: /delete/i }));

    await waitFor(() => {
      expect(screen.getByText("Delete Category?")).toBeInTheDocument();
      expect(screen.getByText(/permanently delete/i)).toBeInTheDocument();
    });
  });

  it("calls onDelete when delete is confirmed", async () => {
    const onDelete = vi.fn();
    const user = userEvent.setup();
    render(
      <CategoryForm
        {...defaultProps}
        mode="edit"
        defaultValues={{ name: "Groceries", description: "Food items", isActive: true }}
        isAdmin={true}
        onDelete={onDelete}
      />,
    );

    await user.click(screen.getByRole("button", { name: /delete/i }));

    await waitFor(() => {
      expect(screen.getByText("Delete Category?")).toBeInTheDocument();
    });

    // Click the "Delete" action in the confirmation dialog
    const confirmButtons = screen.getAllByRole("button", { name: /delete/i });
    const confirmButton = confirmButtons[confirmButtons.length - 1];
    await user.click(confirmButton);

    expect(onDelete).toHaveBeenCalledTimes(1);
  });

  it("does not show delete button when onDelete is not provided", () => {
    render(
      <CategoryForm
        {...defaultProps}
        mode="edit"
        defaultValues={{ name: "Groceries", description: "Food items", isActive: true }}
        isAdmin={true}
      />,
    );

    expect(screen.queryByRole("button", { name: /delete/i })).not.toBeInTheDocument();
  });
});

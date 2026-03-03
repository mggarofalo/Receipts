import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { SubcategoryForm } from "./SubcategoryForm";

vi.mock("@/hooks/useFormShortcuts", () => ({
  useFormShortcuts: vi.fn(),
}));

vi.mock("@/hooks/useCategories", () => ({
  useCategories: vi.fn(() => ({
    data: [
      { id: "cat-1", name: "Groceries" },
      { id: "cat-2", name: "Utilities" },
    ],
  })),
}));

describe("SubcategoryForm", () => {
  const defaultProps = {
    mode: "create" as const,
    onSubmit: vi.fn(),
    onCancel: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders in create mode with empty fields and correct submit button text", () => {
    render(<SubcategoryForm {...defaultProps} />);

    expect(screen.getByLabelText("Name")).toHaveValue("");
    expect(screen.getByLabelText("Description (optional)")).toHaveValue("");
    expect(screen.getByRole("button", { name: /create subcategory/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancel/i })).toBeInTheDocument();
  });

  it("renders in edit mode with pre-populated fields and correct submit button text", () => {
    render(
      <SubcategoryForm
        {...defaultProps}
        mode="edit"
        defaultValues={{ name: "Produce", categoryId: "cat-1", description: "Fresh produce" }}
      />,
    );

    expect(screen.getByLabelText("Name")).toHaveValue("Produce");
    expect(screen.getByLabelText("Description (optional)")).toHaveValue("Fresh produce");
    expect(screen.getByRole("button", { name: /update subcategory/i })).toBeInTheDocument();
  });

  it("shows validation errors when required fields are empty", async () => {
    const user = userEvent.setup();
    render(<SubcategoryForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /create subcategory/i }));

    await waitFor(() => {
      expect(screen.getByText("Name is required")).toBeInTheDocument();
      expect(screen.getByText("Category is required")).toBeInTheDocument();
    });
    expect(defaultProps.onSubmit).not.toHaveBeenCalled();
  });

  it("calls onCancel when cancel button is clicked", async () => {
    const user = userEvent.setup();
    render(<SubcategoryForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /cancel/i }));

    expect(defaultProps.onCancel).toHaveBeenCalledTimes(1);
  });

  it("disables submit button and shows spinner when isSubmitting is true", () => {
    render(<SubcategoryForm {...defaultProps} isSubmitting={true} />);

    const submitButton = screen.getByRole("button", { name: /saving/i });
    expect(submitButton).toBeDisabled();
  });

  it("renders the category combobox with options from useCategories", () => {
    render(<SubcategoryForm {...defaultProps} />);

    expect(screen.getByRole("combobox")).toBeInTheDocument();
    expect(screen.getByText("Select a category...")).toBeInTheDocument();
  });

  it("calls onSubmit with correct data when all fields are filled", async () => {
    const user = userEvent.setup();
    render(
      <SubcategoryForm
        {...defaultProps}
        defaultValues={{ name: "", categoryId: "cat-1", description: "" }}
      />,
    );

    await user.type(screen.getByLabelText("Name"), "Dairy");
    await user.click(screen.getByRole("button", { name: /create subcategory/i }));

    await waitFor(() => {
      expect(defaultProps.onSubmit).toHaveBeenCalledWith(
        expect.objectContaining({ name: "Dairy", categoryId: "cat-1" }),
        expect.anything(),
      );
    });
  });
});

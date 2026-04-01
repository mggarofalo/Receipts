import "@/test/setup-combobox-polyfills";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { SubcategoryForm } from "./SubcategoryForm";

vi.mock("@/hooks/useFormShortcuts", () => ({
  useFormShortcuts: vi.fn(),
}));

vi.mock("@/hooks/useCategories", () => ({
  useCategories: vi.fn(() => ({
    data: [
      { id: "cat-1", name: "Groceries", isActive: true },
      { id: "cat-2", name: "Utilities", isActive: true },
    ],
    total: 2,
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
    localStorage.clear();
  });

  it("renders in create mode with empty fields and correct submit button text", () => {
    render(<SubcategoryForm {...defaultProps} />);

    // Name is now a Combobox (button), not an Input
    expect(screen.getByLabelText(/^Name/)).toHaveTextContent(
      "Select or type a name...",
    );
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

    // Name is a Combobox; it shows the value as text content
    expect(screen.getByLabelText(/^Name/)).toHaveTextContent("Produce");
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

    // Name and Category are both comboboxes now
    const comboboxes = screen.getAllByRole("combobox");
    expect(comboboxes.length).toBeGreaterThanOrEqual(2);
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

    // Name is a Combobox with allowCustom; type a custom value
    const nameCombobox = screen.getByLabelText(/^Name/);
    await user.click(nameCombobox);
    const searchInput = screen.getByPlaceholderText("Search names...");
    await user.type(searchInput, "Dairy");
    await user.click(screen.getByText(/Use "Dairy"/));

    await user.click(screen.getByRole("button", { name: /create subcategory/i }));

    await waitFor(() => {
      expect(defaultProps.onSubmit).toHaveBeenCalledWith(
        expect.objectContaining({ name: "Dairy", categoryId: "cat-1" }),
      );
    });
  });

  it("shows saved subcategory names in the Name Combobox dropdown", async () => {
    const user = userEvent.setup();
    localStorage.setItem(
      "receipts:subcategory-name-history",
      JSON.stringify(["Dairy", "Produce"]),
    );

    render(<SubcategoryForm {...defaultProps} />);

    const nameCombobox = screen.getByLabelText(/^Name/);
    await user.click(nameCombobox);

    await waitFor(() => {
      expect(screen.getByText("Dairy")).toBeInTheDocument();
      expect(screen.getByText("Produce")).toBeInTheDocument();
    });
  });

  it("selects a history name and populates the field", async () => {
    const user = userEvent.setup();
    localStorage.setItem(
      "receipts:subcategory-name-history",
      JSON.stringify(["Dairy"]),
    );

    render(<SubcategoryForm {...defaultProps} />);

    const nameCombobox = screen.getByLabelText(/^Name/);
    await user.click(nameCombobox);

    await waitFor(() => {
      expect(screen.getByText("Dairy")).toBeInTheDocument();
    });

    await user.click(screen.getByText("Dairy"));

    expect(nameCombobox).toHaveTextContent("Dairy");
  });

  it("persists subcategory name to history on submit", async () => {
    const user = userEvent.setup();
    render(
      <SubcategoryForm
        {...defaultProps}
        defaultValues={{ name: "", categoryId: "cat-1", description: "" }}
      />,
    );

    // Type a custom name
    const nameCombobox = screen.getByLabelText(/^Name/);
    await user.click(nameCombobox);
    const searchInput = screen.getByPlaceholderText("Search names...");
    await user.type(searchInput, "Bakery");
    await user.click(screen.getByText(/Use "Bakery"/));

    await user.click(screen.getByRole("button", { name: /create subcategory/i }));

    await waitFor(() => {
      expect(defaultProps.onSubmit).toHaveBeenCalled();
    });

    const stored = JSON.parse(
      localStorage.getItem("receipts:subcategory-name-history") ?? "[]",
    ) as string[];
    expect(stored).toContain("Bakery");
  });

  it("does not show delete button in the form", () => {
    render(
      <SubcategoryForm
        {...defaultProps}
        mode="edit"
        defaultValues={{ name: "Produce", categoryId: "cat-1", description: "" }}
      />,
    );

    expect(screen.queryByRole("button", { name: /delete/i })).not.toBeInTheDocument();
  });
});

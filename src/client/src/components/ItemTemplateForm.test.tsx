import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ItemTemplateForm } from "./ItemTemplateForm";

vi.mock("@/hooks/useFormShortcuts", () => ({
  useFormShortcuts: vi.fn(),
}));

vi.mock("@/hooks/useEnumMetadata", () => ({
  useEnumMetadata: vi.fn(() => ({
    adjustmentTypes: [],
    authEventTypes: [],
    pricingModes: [{ value: "quantity", label: "Quantity" }, { value: "flat", label: "Flat" }],
    auditActions: [],
    entityTypes: [],
    adjustmentTypeLabels: {},
    authEventLabels: {},
    pricingModeLabels: { quantity: "Quantity", flat: "Flat" },
    auditActionLabels: {},
    entityTypeLabels: {},
    isLoading: false,
  })),
}));

vi.mock("@/hooks/useCategories", () => ({
  useCategories: vi.fn(() => ({
    data: {
      data: [
        { id: "cat-1", name: "Groceries" },
        { id: "cat-2", name: "Electronics" },
      ],
    },
  })),
}));

vi.mock("@/hooks/useSubcategories", () => ({
  useSubcategoriesByCategoryId: vi.fn(() => ({
    data: {
      data: [
        { id: "sub-1", name: "Dairy" },
        { id: "sub-2", name: "Bakery" },
      ],
    },
  })),
}));

describe("ItemTemplateForm", () => {
  const defaultProps = {
    mode: "create" as const,
    onSubmit: vi.fn(),
    onCancel: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders in create mode with empty fields and correct submit button text", () => {
    render(<ItemTemplateForm {...defaultProps} />);

    expect(screen.getByLabelText("Name")).toHaveValue("");
    expect(screen.getByLabelText("Description (optional)")).toHaveValue("");
    expect(screen.getByLabelText("Default Item Code (optional)")).toHaveValue("");
    expect(screen.getByRole("button", { name: /create template/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancel/i })).toBeInTheDocument();
  });

  it("renders in edit mode with pre-populated fields and correct submit button text", () => {
    render(
      <ItemTemplateForm
        {...defaultProps}
        mode="edit"
        defaultValues={{
          name: "Milk",
          description: "Whole milk",
          defaultCategory: "Groceries",
          defaultSubcategory: "Dairy",
          defaultUnitPrice: 3.99,
          defaultPricingMode: "quantity",
          defaultItemCode: "MLK-001",
        }}
      />,
    );

    expect(screen.getByLabelText("Name")).toHaveValue("Milk");
    expect(screen.getByLabelText("Description (optional)")).toHaveValue("Whole milk");
    expect(screen.getByLabelText("Default Item Code (optional)")).toHaveValue("MLK-001");
    expect(screen.getByRole("button", { name: /update template/i })).toBeInTheDocument();
  });

  it("calls onSubmit with correct data when form is valid", async () => {
    const user = userEvent.setup();
    render(<ItemTemplateForm {...defaultProps} />);

    await user.type(screen.getByLabelText("Name"), "Bread");
    await user.click(screen.getByRole("button", { name: /create template/i }));

    await waitFor(() => {
      expect(defaultProps.onSubmit).toHaveBeenCalledWith(
        expect.objectContaining({ name: "Bread" }),
        expect.anything(),
      );
    });
  });

  it("shows validation error when name is empty", async () => {
    const user = userEvent.setup();
    render(<ItemTemplateForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /create template/i }));

    await waitFor(() => {
      expect(screen.getByText("Name is required")).toBeInTheDocument();
    });
    expect(defaultProps.onSubmit).not.toHaveBeenCalled();
  });

  it("calls onCancel when cancel button is clicked", async () => {
    const user = userEvent.setup();
    render(<ItemTemplateForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /cancel/i }));

    expect(defaultProps.onCancel).toHaveBeenCalledTimes(1);
  });

  it("disables submit button and shows spinner when isSubmitting is true", () => {
    render(<ItemTemplateForm {...defaultProps} isSubmitting={true} />);

    const submitButton = screen.getByRole("button", { name: /saving/i });
    expect(submitButton).toBeDisabled();
  });

  it("renders all optional fields", () => {
    render(<ItemTemplateForm {...defaultProps} />);

    expect(screen.getByText("Default Category (optional)")).toBeInTheDocument();
    expect(screen.getByText("Default Subcategory (optional)")).toBeInTheDocument();
    expect(screen.getByText("Default Unit Price (optional)")).toBeInTheDocument();
    expect(screen.getByText("Default Pricing Mode (optional)")).toBeInTheDocument();
    expect(screen.getByText("Default Item Code (optional)")).toBeInTheDocument();
  });
});

import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ReceiptForm } from "./ReceiptForm";

vi.mock("@/hooks/useFormShortcuts", () => ({
  useFormShortcuts: vi.fn(),
}));

describe("ReceiptForm", () => {
  const defaultProps = {
    mode: "create" as const,
    onSubmit: vi.fn(),
    onCancel: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders in create mode with empty fields and correct submit button text", () => {
    render(<ReceiptForm {...defaultProps} />);

    expect(screen.getByLabelText("Location")).toHaveValue("");
    expect(screen.getByRole("button", { name: /create receipt/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancel/i })).toBeInTheDocument();
  });

  it("renders in edit mode with pre-populated fields and correct submit button text", () => {
    render(
      <ReceiptForm
        {...defaultProps}
        mode="edit"
        defaultValues={{
          location: "Walmart",
          date: "2024-01-15",
          taxAmount: 5.25,
        }}
      />,
    );

    expect(screen.getByLabelText("Location")).toHaveValue("Walmart");
    expect(screen.getByRole("button", { name: /update receipt/i })).toBeInTheDocument();
  });

  it("calls onSubmit with correct data when form is valid", async () => {
    const user = userEvent.setup();
    render(<ReceiptForm {...defaultProps} />);

    await user.type(screen.getByLabelText("Location"), "Target");
    await user.type(screen.getByLabelText("Date"), "2024-03-01");
    await user.click(screen.getByRole("button", { name: /create receipt/i }));

    await waitFor(() => {
      expect(defaultProps.onSubmit).toHaveBeenCalledWith(
        expect.objectContaining({
          location: "Target",
          date: "2024-03-01",
          taxAmount: 0,
        }),
        expect.anything(),
      );
    });
  });

  it("shows validation errors when required fields are empty", async () => {
    const user = userEvent.setup();
    render(<ReceiptForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /create receipt/i }));

    await waitFor(() => {
      expect(screen.getByText("Location is required")).toBeInTheDocument();
      expect(screen.getByText("Date is required")).toBeInTheDocument();
    });
    expect(defaultProps.onSubmit).not.toHaveBeenCalled();
  });

  it("calls onCancel when cancel button is clicked", async () => {
    const user = userEvent.setup();
    render(<ReceiptForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /cancel/i }));

    expect(defaultProps.onCancel).toHaveBeenCalledTimes(1);
  });

  it("disables submit button and shows spinner when isSubmitting is true", () => {
    render(<ReceiptForm {...defaultProps} isSubmitting={true} />);

    const submitButton = screen.getByRole("button", { name: /saving/i });
    expect(submitButton).toBeDisabled();
  });

  it("renders tax amount field", () => {
    render(<ReceiptForm {...defaultProps} />);

    expect(screen.getByText("Tax Amount")).toBeInTheDocument();
  });

  it("renders date input field", () => {
    render(<ReceiptForm {...defaultProps} />);

    expect(screen.getByLabelText("Date")).toBeInTheDocument();
  });
});

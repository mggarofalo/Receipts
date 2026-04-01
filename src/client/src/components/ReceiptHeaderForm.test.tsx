import "@/test/setup-combobox-polyfills";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ReceiptHeaderForm } from "./ReceiptHeaderForm";

vi.mock("@/hooks/useFormShortcuts", () => ({
  useFormShortcuts: vi.fn(),
}));

vi.mock("@/hooks/useLocationHistory", () => ({
  useLocationHistory: vi.fn(() => ({
    locations: [],
    options: [],
    add: vi.fn(),
    clear: vi.fn(),
  })),
}));

describe("ReceiptHeaderForm", () => {
  const defaultProps = {
    onSubmit: vi.fn(),
    onCancel: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders all form fields", () => {
    render(<ReceiptHeaderForm {...defaultProps} />);
    expect(screen.getByRole("combobox")).toBeInTheDocument();
    expect(screen.getByRole("textbox", { name: /^date/i })).toBeInTheDocument();
    expect(screen.getByText(/tax amount/i)).toBeInTheDocument();
  });

  it("renders with default values", () => {
    render(
      <ReceiptHeaderForm
        {...defaultProps}
        defaultValues={{
          location: "Walmart",
          date: "2024-01-15",
          taxAmount: 5.25,
        }}
      />,
    );
    expect(screen.getByRole("combobox")).toHaveTextContent("Walmart");
  });

  it("calls onCancel when cancel button is clicked", async () => {
    const user = userEvent.setup();
    render(<ReceiptHeaderForm {...defaultProps} />);
    await user.click(screen.getByRole("button", { name: /cancel/i }));
    expect(defaultProps.onCancel).toHaveBeenCalledTimes(1);
  });

  it("shows validation errors for empty required fields on submit", async () => {
    const user = userEvent.setup();
    render(<ReceiptHeaderForm {...defaultProps} />);
    // Find submit button -- exclude the combobox trigger
    const buttons = screen.getAllByRole("button");
    const submitButton = buttons.find((b) => /update receipt/i.test(b.textContent ?? ""));
    await user.click(submitButton!);
    expect(await screen.findByText(/location is required/i)).toBeInTheDocument();
  });

  it("shows saving state when isSubmitting is true", () => {
    render(<ReceiptHeaderForm {...defaultProps} isSubmitting />);
    expect(screen.getByRole("button", { name: /saving/i })).toBeDisabled();
  });

  it("renders server errors when provided", () => {
    render(
      <ReceiptHeaderForm
        {...defaultProps}
        serverErrors={{ location: "Location already exists" }}
      />,
    );
    expect(screen.getByText("Location already exists")).toBeInTheDocument();
  });

  it("renders server errors for date field", () => {
    render(
      <ReceiptHeaderForm
        {...defaultProps}
        serverErrors={{ date: "Invalid date format" }}
      />,
    );
    expect(screen.getByText("Invalid date format")).toBeInTheDocument();
  });

  it("renders server errors for taxAmount field", () => {
    render(
      <ReceiptHeaderForm
        {...defaultProps}
        serverErrors={{ taxAmount: "Tax amount cannot be negative" }}
      />,
    );
    expect(
      screen.getByText("Tax amount cannot be negative"),
    ).toBeInTheDocument();
  });
});

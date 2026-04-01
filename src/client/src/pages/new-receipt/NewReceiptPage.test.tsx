import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderWithProviders } from "@/test/test-utils";
import { mockMutationResult } from "@/test/mock-hooks";
import "@/test/setup-combobox-polyfills";
import NewReceiptPage from "./NewReceiptPage";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

const mockCreateCompleteReceiptAsync = vi.fn();

vi.mock("@/hooks/useReceipts", () => ({
  useCreateCompleteReceipt: vi.fn(() =>
    mockMutationResult({ mutateAsync: mockCreateCompleteReceiptAsync }),
  ),
}));

vi.mock("@/hooks/useLocationHistory", () => ({
  useLocationHistory: vi.fn(() => ({
    locations: [],
    options: [{ value: "Walmart", label: "Walmart" }],
    add: vi.fn(),
    clear: vi.fn(),
  })),
}));

const mockNavigate = vi.fn();
vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof import("react-router")>();
  return {
    ...actual,
    useNavigate: vi.fn(() => mockNavigate),
  };
});

vi.mock("sonner", () => ({
  toast: { success: vi.fn(), error: vi.fn(), info: vi.fn() },
}));

// Mock child sections to isolate page logic
vi.mock("./TransactionsSection", () => ({
  TransactionsSection: ({
    onChange,
  }: {
    transactions: unknown[];
    receiptDate: string;
    onChange: (data: unknown[]) => void;
  }) => (
    <div data-testid="transactions-section">
      <button
        onClick={() =>
          onChange([
            { id: "t1", accountId: "acct-1", amount: 55, date: "2024-01-15" },
          ])
        }
      >
        Add Transaction
      </button>
    </div>
  ),
}));

vi.mock("./LineItemsSection", () => ({
  LineItemsSection: ({
    onChange,
  }: {
    items: unknown[];
    onChange: (data: unknown[]) => void;
  }) => (
    <div data-testid="line-items-section">
      <button
        onClick={() =>
          onChange([
            {
              id: "i1",
              receiptItemCode: "",
              description: "Milk",
              pricingMode: "quantity",
              quantity: 1,
              unitPrice: 50,
              category: "Food",
              subcategory: "",
            },
          ])
        }
      >
        Add Item
      </button>
    </div>
  ),
}));

vi.mock("./BalanceSidebar", () => ({
  BalanceSidebar: ({
    onSubmit,
    onCancel,
    isSubmitting,
  }: {
    subtotal: number;
    taxAmount: number;
    transactionTotal: number;
    isSubmitting: boolean;
    onSubmit: () => void;
    onCancel: () => void;
  }) => (
    <div data-testid="balance-sidebar">
      <button onClick={onSubmit} disabled={isSubmitting}>
        {isSubmitting ? "Submitting..." : "Submit Receipt"}
      </button>
      <button onClick={onCancel}>Cancel</button>
    </div>
  ),
}));

describe("NewReceiptPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockCreateCompleteReceiptAsync.mockResolvedValue({
      receipt: { id: "receipt-123" },
      transactions: [],
      items: [],
    });
  });

  it("renders the page heading", () => {
    renderWithProviders(<NewReceiptPage />);
    expect(
      screen.getByRole("heading", { name: /new receipt/i }),
    ).toBeInTheDocument();
  });

  it("renders all three sections", () => {
    renderWithProviders(<NewReceiptPage />);
    expect(screen.getByText(/^Location/)).toBeInTheDocument();
    expect(screen.getByTestId("transactions-section")).toBeInTheDocument();
    expect(screen.getByTestId("line-items-section")).toBeInTheDocument();
  });

  it("renders balance sidebar", () => {
    renderWithProviders(<NewReceiptPage />);
    expect(screen.getAllByTestId("balance-sidebar").length).toBeGreaterThan(0);
  });

  it("navigates directly to /receipts when cancel clicked with no data", async () => {
    const user = userEvent.setup();
    renderWithProviders(<NewReceiptPage />);

    // Click the first cancel button (there are two — desktop and mobile)
    const cancelButtons = screen.getAllByText("Cancel");
    await user.click(cancelButtons[0]);
    expect(mockNavigate).toHaveBeenCalledWith("/receipts");
  });

  it("shows discard dialog when cancel clicked after entering data", async () => {
    const user = userEvent.setup();
    renderWithProviders(<NewReceiptPage />);

    // Add a transaction to make the form dirty
    await user.click(screen.getAllByText("Add Transaction")[0]);

    // Click cancel
    const cancelButtons = screen.getAllByText("Cancel");
    await user.click(cancelButtons[0]);

    expect(screen.getByText("Discard receipt?")).toBeInTheDocument();
  });

  it("discards and navigates when Discard is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<NewReceiptPage />);

    // Add data
    await user.click(screen.getAllByText("Add Transaction")[0]);

    // Open discard dialog
    const cancelButtons = screen.getAllByText("Cancel");
    await user.click(cancelButtons[0]);

    // Click Discard
    await user.click(screen.getByText("Discard"));
    expect(mockNavigate).toHaveBeenCalledWith("/receipts");
  });

  it("continues editing when Continue editing is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<NewReceiptPage />);

    // Add data
    await user.click(screen.getAllByText("Add Transaction")[0]);

    // Open discard dialog
    const cancelButtons = screen.getAllByText("Cancel");
    await user.click(cancelButtons[0]);

    // Click Continue editing
    await user.click(screen.getByText("Continue editing"));
    expect(screen.queryByText("Discard receipt?")).not.toBeInTheDocument();
  });

  it("shows error toast when submitting without transactions", async () => {
    const { toast } = await import("sonner");
    const user = userEvent.setup();
    renderWithProviders(<NewReceiptPage />);

    // Fill header (location is a combobox — select Walmart)
    const combobox = screen.getByRole("combobox");
    await user.click(combobox);
    const walmart = await screen.findByText("Walmart");
    await user.click(walmart);

    // Fill date
    const dateInput = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(dateInput);
    await user.type(dateInput, "01/15/2024");

    // Add an item but no transaction
    await user.click(screen.getAllByText("Add Item")[0]);

    // Submit
    const submitButtons = screen.getAllByText("Submit Receipt");
    await user.click(submitButtons[0]);

    await vi.waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith(
        "Add at least one transaction.",
      );
    });
  });

  it("shows error toast when submitting without line items", async () => {
    const { toast } = await import("sonner");
    const user = userEvent.setup();
    renderWithProviders(<NewReceiptPage />);

    // Fill header
    const combobox = screen.getByRole("combobox");
    await user.click(combobox);
    const walmart = await screen.findByText("Walmart");
    await user.click(walmart);

    const dateInput = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(dateInput);
    await user.type(dateInput, "01/15/2024");

    // Add a transaction but no items
    await user.click(screen.getAllByText("Add Transaction")[0]);

    // Submit
    const submitButtons = screen.getAllByText("Submit Receipt");
    await user.click(submitButtons[0]);

    await vi.waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith(
        "Add at least one line item.",
      );
    });
  });

  it("submits receipt successfully with all data", async () => {
    const { toast } = await import("sonner");
    const user = userEvent.setup();
    renderWithProviders(<NewReceiptPage />);

    // Fill header
    const combobox = screen.getByRole("combobox");
    await user.click(combobox);
    const walmart = await screen.findByText("Walmart");
    await user.click(walmart);

    const dateInput = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(dateInput);
    await user.type(dateInput, "01/15/2024");

    // Add transaction and item
    await user.click(screen.getAllByText("Add Transaction")[0]);
    await user.click(screen.getAllByText("Add Item")[0]);

    // Submit
    const submitButtons = screen.getAllByText("Submit Receipt");
    await user.click(submitButtons[0]);

    await vi.waitFor(() => {
      expect(mockCreateCompleteReceiptAsync).toHaveBeenCalledWith(
        expect.objectContaining({
          receipt: expect.objectContaining({
            location: "Walmart",
          }),
          transactions: [
            expect.objectContaining({
              accountId: "acct-1",
              amount: 55,
            }),
          ],
          items: [
            expect.objectContaining({
              description: "Milk",
              category: "Food",
            }),
          ],
        }),
      );
    });

    expect(toast.success).toHaveBeenCalledWith("Receipt created successfully!");
    expect(mockNavigate).toHaveBeenCalledWith(
      "/receipts/receipt-123",
    );
  });

  it("shows error toast when submission fails", async () => {
    const { toast } = await import("sonner");
    mockCreateCompleteReceiptAsync.mockRejectedValueOnce(
      new Error("Server error"),
    );
    const user = userEvent.setup();
    renderWithProviders(<NewReceiptPage />);

    // Fill header
    const combobox = screen.getByRole("combobox");
    await user.click(combobox);
    const walmart = await screen.findByText("Walmart");
    await user.click(walmart);

    const dateInput = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(dateInput);
    await user.type(dateInput, "01/15/2024");

    // Add transaction and item
    await user.click(screen.getAllByText("Add Transaction")[0]);
    await user.click(screen.getAllByText("Add Item")[0]);

    // Submit
    const submitButtons = screen.getAllByText("Submit Receipt");
    await user.click(submitButtons[0]);

    await vi.waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith("Failed to create receipt.");
    });
  });
});

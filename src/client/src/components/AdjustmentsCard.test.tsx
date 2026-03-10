import { describe, it, expect, vi } from "vitest";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AdjustmentsCard } from "./AdjustmentsCard";
import { renderWithQueryClient } from "@/test/test-utils";
import { mockMutationResult } from "@/test/mock-hooks";

vi.mock("@/hooks/useEnumMetadata", () => ({
  useEnumMetadata: vi.fn(() => ({
    adjustmentTypes: [{ value: "Tip", label: "Tip" }, { value: "Discount", label: "Discount" }],
    authEventTypes: [],
    pricingModes: [],
    auditActions: [],
    entityTypes: [],
    adjustmentTypeLabels: { Tip: "Tip", Discount: "Discount" },
    authEventLabels: {},
    pricingModeLabels: {},
    auditActionLabels: {},
    entityTypeLabels: {},
    isLoading: false,
  })),
}));

vi.mock("@/hooks/useAdjustments", () => ({
  useCreateAdjustment: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useUpdateAdjustment: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useDeleteAdjustments: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

const mockAdjustments = [
  {
    id: "adj-1",
    receiptId: "receipt-1",
    type: "Tax",
    amount: 5.25,
    description: "Sales tax",
  },
  {
    id: "adj-2",
    receiptId: "receipt-1",
    type: "Discount",
    amount: -2.0,
    description: null,
  },
];

describe("AdjustmentsCard", () => {
  it("renders empty state when there are no adjustments", () => {
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={[]}
        adjustmentTotal={0}
      />,
    );
    expect(
      screen.getByText("No adjustments for this receipt."),
    ).toBeInTheDocument();
    expect(screen.getByText("Adjustments (0)")).toBeInTheDocument();
  });

  it("renders adjustment rows with data", () => {
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={mockAdjustments}
        adjustmentTotal={3.25}
      />,
    );
    expect(screen.getByText("Adjustments (2)")).toBeInTheDocument();
    expect(screen.getByText("Tax")).toBeInTheDocument();
    expect(screen.getByText("Discount")).toBeInTheDocument();
    expect(screen.getByText("Sales tax")).toBeInTheDocument();
  });

  it("renders table headers when adjustments exist", () => {
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={mockAdjustments}
        adjustmentTotal={3.25}
      />,
    );
    expect(screen.getByText("Type")).toBeInTheDocument();
    expect(screen.getByText("Description")).toBeInTheDocument();
    expect(screen.getByText("Amount")).toBeInTheDocument();
    expect(screen.getByText("Actions")).toBeInTheDocument();
  });

  it("renders the adjustment total in the footer", () => {
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={mockAdjustments}
        adjustmentTotal={3.25}
      />,
    );
    expect(screen.getByText("Adjustment Total")).toBeInTheDocument();
    expect(screen.getByText("$3.25")).toBeInTheDocument();
  });

  it("renders Add Adjustment button", () => {
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={mockAdjustments}
        adjustmentTotal={3.25}
      />,
    );
    expect(
      screen.getByRole("button", { name: /add adjustment/i }),
    ).toBeInTheDocument();
  });

  it("renders edit buttons for each adjustment row", () => {
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={mockAdjustments}
        adjustmentTotal={3.25}
      />,
    );
    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    expect(editButtons).toHaveLength(2);
  });

  it("toggles individual row selection checkbox", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={mockAdjustments}
        adjustmentTotal={3.25}
      />,
    );
    const taxCheckbox = screen.getByLabelText("Select Tax adjustment");
    expect(taxCheckbox).not.toBeChecked();
    await user.click(taxCheckbox);
    expect(taxCheckbox).toBeChecked();
    // Toggle off
    await user.click(taxCheckbox);
    expect(taxCheckbox).not.toBeChecked();
  });

  it("select-all checkbox selects and deselects all adjustments", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={mockAdjustments}
        adjustmentTotal={3.25}
      />,
    );
    const selectAll = screen.getByLabelText("Select all adjustments");
    expect(selectAll).not.toBeChecked();
    await user.click(selectAll);
    expect(selectAll).toBeChecked();
    expect(screen.getByLabelText("Select Tax adjustment")).toBeChecked();
    expect(screen.getByLabelText("Select Discount adjustment")).toBeChecked();
    // Deselect all
    await user.click(selectAll);
    expect(selectAll).not.toBeChecked();
    expect(screen.getByLabelText("Select Tax adjustment")).not.toBeChecked();
    expect(screen.getByLabelText("Select Discount adjustment")).not.toBeChecked();
  });

  it("shows Delete button with count when items are selected", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={mockAdjustments}
        adjustmentTotal={3.25}
      />,
    );
    // No delete button initially
    expect(screen.queryByRole("button", { name: /delete/i })).not.toBeInTheDocument();
    // Select one
    await user.click(screen.getByLabelText("Select Tax adjustment"));
    expect(screen.getByRole("button", { name: /delete \(1\)/i })).toBeInTheDocument();
  });

  it("opens delete confirmation dialog and calls deleteAdjustments.mutate on confirm", async () => {
    const { useDeleteAdjustments } = await import("@/hooks/useAdjustments");
    const mockDeleteMutate = vi.fn();
    vi.mocked(useDeleteAdjustments).mockReturnValue(mockMutationResult({
      mutate: mockDeleteMutate,
      isPending: false,
    }));

    const user = userEvent.setup();
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={mockAdjustments}
        adjustmentTotal={3.25}
      />,
    );
    // Select an adjustment
    await user.click(screen.getByLabelText("Select Tax adjustment"));
    // Click the Delete button to open dialog
    await user.click(screen.getByRole("button", { name: /delete \(1\)/i }));
    // Confirmation dialog should appear
    expect(screen.getByText("Delete Adjustments")).toBeInTheDocument();
    expect(screen.getByText(/are you sure you want to delete 1 adjustment/i)).toBeInTheDocument();
    // Click the destructive Delete button in the dialog
    const confirmDelete = screen.getByRole("button", { name: "Delete" });
    await user.click(confirmDelete);
    expect(mockDeleteMutate).toHaveBeenCalledWith(["adj-1"]);
  });

  it("opens create dialog when Add Adjustment is clicked", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={mockAdjustments}
        adjustmentTotal={3.25}
      />,
    );
    await user.click(screen.getByRole("button", { name: /add adjustment/i }));
    expect(screen.getByText("Add Adjustment", { selector: "[id]" })).toBeInTheDocument();
  });

  it("opens edit dialog when Edit button is clicked", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={mockAdjustments}
        adjustmentTotal={3.25}
      />,
    );
    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    await user.click(editButtons[0]);
    expect(screen.getByText("Edit Adjustment")).toBeInTheDocument();
  });

  it("calls createAdjustment.mutate when create form is submitted", async () => {
    const { useCreateAdjustment } = await import("@/hooks/useAdjustments");
    const mockCreateMutate = vi.fn();
    vi.mocked(useCreateAdjustment).mockReturnValue(mockMutationResult({
      mutate: mockCreateMutate,
      isPending: false,
    }));

    const user = userEvent.setup();
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={mockAdjustments}
        adjustmentTotal={3.25}
      />,
    );
    // Open create dialog
    await user.click(screen.getByRole("button", { name: /add adjustment/i }));
    // The form appears with a submit button
    const submitButton = screen.getByRole("button", { name: "Add Adjustment" });
    expect(submitButton).toBeInTheDocument();
  });

  it("calls updateAdjustment.mutate when edit form is submitted", async () => {
    const { useUpdateAdjustment } = await import("@/hooks/useAdjustments");
    const mockUpdateMutate = vi.fn();
    vi.mocked(useUpdateAdjustment).mockReturnValue(mockMutationResult({
      mutate: mockUpdateMutate,
      isPending: false,
    }));

    const user = userEvent.setup();
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={mockAdjustments}
        adjustmentTotal={3.25}
      />,
    );
    // Open edit dialog for first adjustment
    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    await user.click(editButtons[0]);
    // The form appears with an update button
    const submitButton = screen.getByRole("button", { name: "Update Adjustment" });
    expect(submitButton).toBeInTheDocument();
  });

  it("cancels delete dialog without deleting", async () => {
    const { useDeleteAdjustments } = await import("@/hooks/useAdjustments");
    const mockDeleteMutate = vi.fn();
    vi.mocked(useDeleteAdjustments).mockReturnValue(mockMutationResult({
      mutate: mockDeleteMutate,
      isPending: false,
    }));

    const user = userEvent.setup();
    renderWithQueryClient(
      <AdjustmentsCard
        receiptId="receipt-1"
        adjustments={mockAdjustments}
        adjustmentTotal={3.25}
      />,
    );
    await user.click(screen.getByLabelText("Select Tax adjustment"));
    await user.click(screen.getByRole("button", { name: /delete \(1\)/i }));
    // Cancel the delete
    await user.click(screen.getByRole("button", { name: "Cancel" }));
    expect(mockDeleteMutate).not.toHaveBeenCalled();
  });
});

import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ReconcileSheet, type ReconcileLine } from "./ReconcileSheet";

const LINES: ReconcileLine[] = [
  {
    id: "item-1",
    kind: "item",
    label: "Organic bananas",
    qty: "2 × $0.69",
    amount: 1.38,
    flagged: false,
  },
  {
    id: "item-2",
    kind: "item",
    label: "Mystery item",
    qty: "1 × $5.00",
    amount: 5,
    flagged: true,
    reason: "uncategorized",
  },
  {
    id: "item-3",
    kind: "item",
    label: "Another mystery",
    qty: "1 × $0.00",
    amount: 0,
    flagged: true,
    reason: "zero amount",
  },
];

function renderSheet(overrides: Partial<React.ComponentProps<typeof ReconcileSheet>> = {}) {
  const onClose = vi.fn();
  const onResolve = vi.fn();
  render(
    <ReconcileSheet
      open
      onClose={onClose}
      onResolve={onResolve}
      receiptId="abcdef1234"
      receiptLabel="Whole Foods"
      receiptDate="2024-01-15"
      receiptTotal={100}
      transactionsTotal={94.5}
      lines={LINES}
      {...overrides}
    />,
  );
  return { onClose, onResolve };
}

describe("ReconcileSheet", () => {
  it("does not render when closed", () => {
    const { container } = render(
      <ReconcileSheet
        open={false}
        onClose={() => {}}
        receiptId="abc"
        receiptLabel="X"
        receiptDate="2024-01-01"
        receiptTotal={0}
        transactionsTotal={0}
        lines={[]}
      />,
    );
    expect(container.firstChild).toBeNull();
  });

  it("renders dialog with title and delta totals", () => {
    renderSheet();
    expect(
      screen.getByRole("dialog", { name: /reconcile receipt/i }),
    ).toBeInTheDocument();
    expect(screen.getByText(/whole foods/i)).toBeInTheDocument();
    expect(screen.getByText("$100.00")).toBeInTheDocument();
    expect(screen.getByText("$94.50")).toBeInTheDocument();
    expect(screen.getByText(/REC-ABCDEF12/)).toBeInTheDocument();
  });

  it("shows balanced state when totals match", () => {
    renderSheet({ receiptTotal: 50, transactionsTotal: 50 });
    expect(screen.getByText("±0.00")).toBeInTheDocument();
  });

  it("lists flagged and unflagged lines", () => {
    renderSheet();
    expect(screen.getByText("Organic bananas")).toBeInTheDocument();
    expect(screen.getByText("Mystery item")).toBeInTheDocument();
    expect(screen.getByText(/2 of 2 resolved|0 of 2 resolved/i)).toBeInTheDocument();
  });

  it("disables Save in balance mode until all flags are resolved", async () => {
    const user = userEvent.setup();
    renderSheet();
    const save = screen.getByRole("button", { name: /save balanced/i });
    expect(save).toBeDisabled();

    // Accept both flagged lines via their accept buttons
    const acceptButtons = screen.getAllByRole("button", {
      name: /mark .* resolved/i,
    });
    for (const btn of acceptButtons) await user.click(btn);

    expect(screen.getByRole("button", { name: /save balanced/i })).toBeEnabled();
  });

  it("calls onClose when Escape is pressed", async () => {
    const user = userEvent.setup();
    const { onClose } = renderSheet();
    await user.keyboard("{Escape}");
    expect(onClose).toHaveBeenCalled();
  });

  it("switches to Accept receipt path and toggles aria-pressed", async () => {
    const user = userEvent.setup();
    renderSheet();
    const pathButtons = screen.getAllByRole("button", {
      name: /accept receipt total/i,
    });
    // First is the path card (has aria-pressed); after clicking, save label updates too.
    const pathCard = pathButtons.find((b) => b.hasAttribute("aria-pressed"))!;
    await user.click(pathCard);
    expect(pathCard).toHaveAttribute("aria-pressed", "true");
  });

  it("invokes onResolve with selected path", async () => {
    const user = userEvent.setup();
    const { onResolve, onClose } = renderSheet();
    const pathButtons = screen.getAllByRole("button", {
      name: /accept transactions/i,
    });
    const pathCard = pathButtons.find((b) => b.hasAttribute("aria-pressed"))!;
    await user.click(pathCard);
    const saveButtons = screen.getAllByRole("button", {
      name: /accept transactions/i,
    });
    const save = saveButtons.find((b) => !b.hasAttribute("aria-pressed"))!;
    await user.click(save);
    expect(onResolve).toHaveBeenCalledWith(
      expect.objectContaining({ path: "transactions" }),
    );
    expect(onClose).toHaveBeenCalled();
  });

  it("renders empty state when there are no lines", () => {
    renderSheet({ lines: [] });
    expect(screen.getByText(/nothing to reconcile/i)).toBeInTheDocument();
  });
});

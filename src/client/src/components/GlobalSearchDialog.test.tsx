import { describe, it, expect, vi, beforeAll } from "vitest";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { GlobalSearchDialog } from "./GlobalSearchDialog";
import { renderWithQueryClient } from "@/test/test-utils";

// cmdk uses ResizeObserver and scrollIntoView which are not available in jsdom
beforeAll(() => {
  globalThis.ResizeObserver = class ResizeObserver {
    observe() {}
    unobserve() {}
    disconnect() {}
  };
  Element.prototype.scrollIntoView = vi.fn();
});

vi.mock("@/hooks/useKeyboardShortcut", () => ({
  useKeyboardShortcut: vi.fn(),
}));

vi.mock("@/hooks/useAccounts", () => ({
  useAccounts: vi.fn(() => ({ data: undefined })),
}));

vi.mock("@/hooks/useReceipts", () => ({
  useReceipts: vi.fn(() => ({ data: undefined })),
}));

vi.mock("@/hooks/useReceiptItems", () => ({
  useReceiptItems: vi.fn(() => ({ data: undefined })),
}));

vi.mock("@/hooks/useTransactions", () => ({
  useTransactions: vi.fn(() => ({ data: undefined })),
}));

describe("GlobalSearchDialog", () => {
  it("renders the command dialog when open", () => {
    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={vi.fn()} />,
    );
    expect(
      screen.getByPlaceholderText("Type to search..."),
    ).toBeInTheDocument();
  });

  it("does not render the search input when closed", () => {
    renderWithQueryClient(
      <GlobalSearchDialog open={false} onOpenChange={vi.fn()} />,
    );
    expect(
      screen.queryByPlaceholderText("Type to search..."),
    ).not.toBeInTheDocument();
  });

  it("renders Navigation group with nav items when open", () => {
    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={vi.fn()} />,
    );
    expect(screen.getByText("Navigation")).toBeInTheDocument();
    expect(screen.getByText("Home")).toBeInTheDocument();
    expect(screen.getByText("Accounts")).toBeInTheDocument();
    expect(screen.getByText("Receipts")).toBeInTheDocument();
    expect(screen.getByText("Transactions")).toBeInTheDocument();
    expect(screen.getByText("Trips")).toBeInTheDocument();
  });

  it("renders account items when accounts data is available", async () => {
    const { useAccounts } = await import("@/hooks/useAccounts");
    vi.mocked(useAccounts).mockReturnValue({
      data: [
        { id: "acc-1", accountCode: "ACC001", name: "Checking" },
        { id: "acc-2", accountCode: "ACC002", name: "Savings" },
      ],
    } as unknown as ReturnType<typeof useAccounts>);

    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={vi.fn()} />,
    );
    expect(screen.getByText("Checking")).toBeInTheDocument();
    expect(screen.getByText("ACC001")).toBeInTheDocument();
    expect(screen.getByText("Savings")).toBeInTheDocument();
  });

  it("calls onOpenChange when toggling dialog", () => {
    const onOpenChange = vi.fn();
    renderWithQueryClient(
      <GlobalSearchDialog open={false} onOpenChange={onOpenChange} />,
    );
    // Dialog is closed, onOpenChange was passed correctly
    expect(screen.queryByPlaceholderText("Type to search...")).not.toBeInTheDocument();
  });

  it("registers keyboard shortcut for toggle via useKeyboardShortcut", async () => {
    const { useKeyboardShortcut } = await import("@/hooks/useKeyboardShortcut");
    const onOpenChange = vi.fn();
    renderWithQueryClient(
      <GlobalSearchDialog open={false} onOpenChange={onOpenChange} />,
    );
    // useKeyboardShortcut should have been called with key "k" and a handler
    expect(useKeyboardShortcut).toHaveBeenCalledWith(
      expect.objectContaining({ key: "k", handler: expect.any(Function) }),
    );
  });

  it("toggleOpen calls onOpenChange with negated open state", async () => {
    const { useKeyboardShortcut } = await import("@/hooks/useKeyboardShortcut");
    vi.mocked(useKeyboardShortcut).mockClear();
    const onOpenChange = vi.fn();
    renderWithQueryClient(
      <GlobalSearchDialog open={false} onOpenChange={onOpenChange} />,
    );
    // Get the handler that was passed to useKeyboardShortcut
    const call = vi.mocked(useKeyboardShortcut).mock.calls[0][0];
    call.handler();
    expect(onOpenChange).toHaveBeenCalledWith(true);
  });

  it("toggleOpen calls onOpenChange(false) when dialog is open", async () => {
    const { useKeyboardShortcut } = await import("@/hooks/useKeyboardShortcut");
    vi.mocked(useKeyboardShortcut).mockClear();
    const onOpenChange = vi.fn();
    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={onOpenChange} />,
    );
    const call = vi.mocked(useKeyboardShortcut).mock.calls[0][0];
    call.handler();
    expect(onOpenChange).toHaveBeenCalledWith(false);
  });

  it("selecting a nav item calls onOpenChange(false) and navigates", async () => {
    const user = userEvent.setup();
    const onOpenChange = vi.fn();
    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={onOpenChange} />,
    );
    // Click on the Home nav item — cmdk triggers onSelect
    const homeItem = screen.getByText("Home");
    await user.click(homeItem);
    expect(onOpenChange).toHaveBeenCalledWith(false);
  });

  it("renders receipt items when receiptItems data is available", async () => {
    const { useReceiptItems } = await import("@/hooks/useReceiptItems");
    vi.mocked(useReceiptItems).mockReturnValue({
      data: [
        {
          id: "ri-1",
          receiptItemCode: "RI001",
          description: "Test Item",
          category: "Food",
        },
      ],
    } as unknown as ReturnType<typeof useReceiptItems>);

    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={vi.fn()} />,
    );
    // "Receipt Items" appears as both a nav item and a group heading
    expect(screen.getAllByText("Receipt Items").length).toBeGreaterThanOrEqual(2);
    expect(screen.getByText("Test Item")).toBeInTheDocument();
    expect(screen.getByText("RI001")).toBeInTheDocument();
  });

  it("renders transaction items when transactions data is available", async () => {
    const { useTransactions } = await import("@/hooks/useTransactions");
    vi.mocked(useTransactions).mockReturnValue({
      data: [
        { id: "txn-1", amount: 42.5, date: "2024-01-15" },
      ],
    } as unknown as ReturnType<typeof useTransactions>);

    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={vi.fn()} />,
    );
    // "Transactions" appears as both a nav item and a group heading
    expect(screen.getAllByText("Transactions").length).toBeGreaterThanOrEqual(2);
    expect(screen.getByText("$42.50")).toBeInTheDocument();
    expect(screen.getByText("2024-01-15")).toBeInTheDocument();
  });

  it("renders receipt items when receipts data is available", async () => {
    const { useReceipts } = await import("@/hooks/useReceipts");
    vi.mocked(useReceipts).mockReturnValue({
      data: [
        { id: "r-1", description: "Grocery receipt", location: "Store A" },
        { id: "r-2", description: null, location: "Store B" },
      ],
    } as unknown as ReturnType<typeof useReceipts>);

    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={vi.fn()} />,
    );
    expect(screen.getByText("Grocery receipt")).toBeInTheDocument();
    expect(screen.getByText("Store B")).toBeInTheDocument();
  });

  it("selecting an account item closes dialog", async () => {
    const { useAccounts } = await import("@/hooks/useAccounts");
    vi.mocked(useAccounts).mockReturnValue({
      data: [
        { id: "acc-1", accountCode: "ACC001", name: "Checking" },
      ],
    } as unknown as ReturnType<typeof useAccounts>);

    const user = userEvent.setup();
    const onOpenChange = vi.fn();
    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={onOpenChange} />,
    );
    await user.click(screen.getByText("Checking"));
    expect(onOpenChange).toHaveBeenCalledWith(false);
  });

  it("selecting a receipt item closes dialog", async () => {
    const { useReceipts } = await import("@/hooks/useReceipts");
    vi.mocked(useReceipts).mockReturnValue({
      data: [
        { id: "r-1", description: "Test Receipt", location: "Downtown" },
      ],
    } as unknown as ReturnType<typeof useReceipts>);

    const user = userEvent.setup();
    const onOpenChange = vi.fn();
    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={onOpenChange} />,
    );
    await user.click(screen.getByText("Test Receipt"));
    expect(onOpenChange).toHaveBeenCalledWith(false);
  });

  it("selecting a transaction item closes dialog", async () => {
    const { useTransactions } = await import("@/hooks/useTransactions");
    vi.mocked(useTransactions).mockReturnValue({
      data: [
        { id: "txn-1", amount: 100, date: "2024-06-01" },
      ],
    } as unknown as ReturnType<typeof useTransactions>);

    const user = userEvent.setup();
    const onOpenChange = vi.fn();
    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={onOpenChange} />,
    );
    await user.click(screen.getByText("$100.00"));
    expect(onOpenChange).toHaveBeenCalledWith(false);
  });
});

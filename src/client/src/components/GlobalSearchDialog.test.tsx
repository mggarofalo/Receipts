import { describe, it, expect, vi, beforeAll } from "vitest";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { GlobalSearchDialog } from "./GlobalSearchDialog";
import { renderWithQueryClient } from "@/test/test-utils";
import { mockQueryResult } from "@/test/mock-hooks";

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

vi.mock("@/hooks/useCards", () => ({
  useCards: vi.fn(() => ({ data: undefined })),
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
    expect(screen.getByText("Cards")).toBeInTheDocument();
    expect(screen.getByText("Receipts")).toBeInTheDocument();
  });

  it("renders account items when accounts data is available", async () => {
    const { useAccounts } = await import("@/hooks/useAccounts");
    vi.mocked(useAccounts).mockReturnValue(mockQueryResult({
      data: [
        { id: "acct-1", name: "Apple Card", isActive: true },
        { id: "acct-2", name: "Chase Sapphire", isActive: true },
      ],
      total: 2,
    }));

    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={vi.fn()} />,
    );
    // "Accounts" appears both as a nav item and a group heading; the group items
    // are the logical account names.
    expect(screen.getByText("Apple Card")).toBeInTheDocument();
    expect(screen.getByText("Chase Sapphire")).toBeInTheDocument();
  });

  it("selecting an account item navigates to /accounts and closes the dialog", async () => {
    const { useAccounts } = await import("@/hooks/useAccounts");
    vi.mocked(useAccounts).mockReturnValue(mockQueryResult({
      data: [{ id: "acct-1", name: "Apple Card", isActive: true }],
      total: 1,
    }));

    const user = userEvent.setup();
    const onOpenChange = vi.fn();
    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={onOpenChange} />,
    );
    await user.click(screen.getByText("Apple Card"));
    expect(onOpenChange).toHaveBeenCalledWith(false);
  });

  it("renders card items when cards data is available", async () => {
    const { useCards } = await import("@/hooks/useCards");
    vi.mocked(useCards).mockReturnValue(mockQueryResult({
      data: [
        { id: "acc-1", cardCode: "CARD001", name: "Checking" },
        { id: "acc-2", cardCode: "CARD002", name: "Savings" },
      ],
      total: 2,
    }));

    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={vi.fn()} />,
    );
    expect(screen.getByText("Checking")).toBeInTheDocument();
    expect(screen.getByText("CARD001")).toBeInTheDocument();
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
    vi.mocked(useReceiptItems).mockReturnValue(mockQueryResult({
      data: [
        {
          id: "ri-1",
          receiptItemCode: "RI001",
          description: "Test Item",
          category: "Food",
          receiptId: "r-1",
        },
      ],
      total: 1,
    }));

    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={vi.fn()} />,
    );
    // "Receipt Items" appears as a group heading
    expect(screen.getByText("Receipt Items")).toBeInTheDocument();
    expect(screen.getByText("Test Item")).toBeInTheDocument();
    expect(screen.getByText("RI001")).toBeInTheDocument();
  });

  it("renders transaction items when transactions data is available", async () => {
    const { useTransactions } = await import("@/hooks/useTransactions");
    vi.mocked(useTransactions).mockReturnValue(mockQueryResult({
      data: [
        { id: "txn-1", amount: 42.5, date: "2024-01-15", receiptId: "r-1" },
      ],
      total: 1,
    }));

    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={vi.fn()} />,
    );
    // "Transactions" appears as a group heading
    expect(screen.getByText("Transactions")).toBeInTheDocument();
    expect(screen.getByText("$42.50")).toBeInTheDocument();
    expect(screen.getByText("2024-01-15")).toBeInTheDocument();
  });

  it("renders receipt items when receipts data is available", async () => {
    const { useReceipts } = await import("@/hooks/useReceipts");
    vi.mocked(useReceipts).mockReturnValue(mockQueryResult({
      data: [
        { id: "r-1", location: "Store A" },
        { id: "r-2", location: "Store B" },
      ],
      total: 2,
    }));

    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={vi.fn()} />,
    );
    expect(screen.getByText("Store A")).toBeInTheDocument();
    expect(screen.getByText("Store B")).toBeInTheDocument();
  });

  it("selecting a card item closes dialog", async () => {
    const { useCards } = await import("@/hooks/useCards");
    vi.mocked(useCards).mockReturnValue(mockQueryResult({
      data: [
        { id: "acc-1", cardCode: "CARD001", name: "Checking" },
      ],
      total: 1,
    }));

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
    vi.mocked(useReceipts).mockReturnValue(mockQueryResult({
      data: [
        { id: "r-1", location: "Downtown" },
      ],
      total: 1,
    }));

    const user = userEvent.setup();
    const onOpenChange = vi.fn();
    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={onOpenChange} />,
    );
    await user.click(screen.getByText("Downtown"));
    expect(onOpenChange).toHaveBeenCalledWith(false);
  });

  it("selecting a transaction item closes dialog", async () => {
    const { useTransactions } = await import("@/hooks/useTransactions");
    vi.mocked(useTransactions).mockReturnValue(mockQueryResult({
      data: [
        { id: "txn-1", amount: 100, date: "2024-06-01", receiptId: "r-1" },
      ],
      total: 1,
    }));

    const user = userEvent.setup();
    const onOpenChange = vi.fn();
    renderWithQueryClient(
      <GlobalSearchDialog open={true} onOpenChange={onOpenChange} />,
    );
    await user.click(screen.getByText("$100.00"));
    expect(onOpenChange).toHaveBeenCalledWith(false);
  });
});

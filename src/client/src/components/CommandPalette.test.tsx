import { describe, it, expect, vi, beforeAll, beforeEach } from "vitest";
import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { CommandPalette } from "./CommandPalette";
import { renderWithQueryClient } from "@/test/test-utils";
import { mockQueryResult } from "@/test/mock-hooks";

// cmdk uses ResizeObserver + scrollIntoView, absent in jsdom.
beforeAll(() => {
  globalThis.ResizeObserver = class ResizeObserver {
    observe() {}
    unobserve() {}
    disconnect() {}
  };
  Element.prototype.scrollIntoView = vi.fn();
});

const navigateMock = vi.fn();

vi.mock("react-router", async () => {
  const actual = await vi.importActual<typeof import("react-router")>(
    "react-router",
  );
  return {
    ...actual,
    useNavigate: () => navigateMock,
  };
});

vi.mock("next-themes", () => ({
  useTheme: () => ({ theme: "light", setTheme: vi.fn() }),
}));

vi.mock("@/hooks/usePermission", () => ({
  usePermission: vi.fn(() => ({ isAdmin: () => false })),
}));

vi.mock("@/hooks/useAccounts", () => ({
  useAccounts: vi.fn(() => mockQueryResult()),
}));
vi.mock("@/hooks/useCards", () => ({
  useCards: vi.fn(() => mockQueryResult()),
}));
vi.mock("@/hooks/useCategories", () => ({
  useCategories: vi.fn(() => mockQueryResult()),
}));
vi.mock("@/hooks/useSubcategories", () => ({
  useSubcategories: vi.fn(() => mockQueryResult()),
}));
vi.mock("@/hooks/useItemTemplates", () => ({
  useItemTemplates: vi.fn(() => mockQueryResult()),
}));
vi.mock("@/hooks/useReceipts", () => ({
  useReceipts: vi.fn(() => mockQueryResult()),
}));
vi.mock("@/hooks/useReceiptItems", () => ({
  useReceiptItems: vi.fn(() => mockQueryResult()),
}));
vi.mock("@/hooks/useUsers", () => ({
  useUsers: vi.fn(() => mockQueryResult()),
}));

beforeEach(async () => {
  navigateMock.mockClear();
  const { usePermission } = await import("@/hooks/usePermission");
  vi.mocked(usePermission).mockReturnValue({
    roles: [],
    hasRole: () => false,
    isAdmin: () => false,
  });
});

describe("CommandPalette", () => {
  it("does not render when closed", () => {
    renderWithQueryClient(
      <CommandPalette open={false} onOpenChange={vi.fn()} />,
    );
    expect(
      screen.queryByPlaceholderText(/type a command or search/i),
    ).not.toBeInTheDocument();
  });

  it("renders all four default groups when opened with no query", () => {
    renderWithQueryClient(
      <CommandPalette open={true} onOpenChange={vi.fn()} />,
    );
    expect(screen.getByText("Create")).toBeInTheDocument();
    expect(screen.getByText("Go to")).toBeInTheDocument();
    expect(screen.getByText("Reports")).toBeInTheDocument();
    expect(screen.getByText("Preferences")).toBeInTheDocument();

    expect(screen.getByText("New Receipt")).toBeInTheDocument();
    expect(screen.getByText("Go to Dashboard")).toBeInTheDocument();
    expect(
      screen.getByText('Open "Out of Balance" Report'),
    ).toBeInTheDocument();
    expect(screen.getByText("Sign Out")).toBeInTheDocument();
  });

  it("hides admin-only commands when user is not admin", () => {
    renderWithQueryClient(
      <CommandPalette open={true} onOpenChange={vi.fn()} />,
    );
    expect(screen.queryByText("Go to User Management")).not.toBeInTheDocument();
    expect(screen.queryByText("Go to Audit Log")).not.toBeInTheDocument();
    expect(screen.queryByText("Go to Trash")).not.toBeInTheDocument();
    expect(screen.queryByText("New User")).not.toBeInTheDocument();
  });

  it("shows admin commands when user is admin", async () => {
    const { usePermission } = await import("@/hooks/usePermission");
    vi.mocked(usePermission).mockReturnValue({
      roles: ["Admin"],
      hasRole: () => true,
      isAdmin: () => true,
    });
    renderWithQueryClient(
      <CommandPalette open={true} onOpenChange={vi.fn()} />,
    );
    expect(screen.getByText("Go to User Management")).toBeInTheDocument();
    expect(screen.getByText("Go to Trash")).toBeInTheDocument();
    expect(screen.getByText("New User")).toBeInTheDocument();
  });

  it("selecting a navigate command calls navigate and closes", async () => {
    const user = userEvent.setup();
    const onOpenChange = vi.fn();
    renderWithQueryClient(
      <CommandPalette open={true} onOpenChange={onOpenChange} />,
    );
    await user.click(screen.getByText("Go to Receipts"));
    expect(navigateMock).toHaveBeenCalledWith("/receipts");
    expect(onOpenChange).toHaveBeenCalledWith(false);
  });

  it("does not render entity groups when query is empty", async () => {
    const { useAccounts } = await import("@/hooks/useAccounts");
    vi.mocked(useAccounts).mockReturnValue(
      mockQueryResult({
        data: [{ id: "a1", name: "Apple Card" }],
      }),
    );
    renderWithQueryClient(
      <CommandPalette open={true} onOpenChange={vi.fn()} />,
    );
    expect(screen.queryByText("Apple Card")).not.toBeInTheDocument();
  });

  it("renders entity results when query is typed", async () => {
    const { useAccounts } = await import("@/hooks/useAccounts");
    vi.mocked(useAccounts).mockReturnValue(
      mockQueryResult({
        data: [
          { id: "a1", name: "Apple Card" },
          { id: "a2", name: "Chase" },
        ],
      }),
    );
    const user = userEvent.setup();
    renderWithQueryClient(
      <CommandPalette open={true} onOpenChange={vi.fn()} />,
    );
    const input = screen.getByPlaceholderText(/type a command or search/i);
    await user.type(input, "apple");
    expect(screen.getByText("Apple Card")).toBeInTheDocument();
  });

  it("navigates to a receipt when a receipt item entity is selected", async () => {
    const { useReceiptItems } = await import("@/hooks/useReceiptItems");
    vi.mocked(useReceiptItems).mockReturnValue(
      mockQueryResult({
        data: [
          {
            id: "ri1",
            receiptId: "r99",
            description: "Organic bananas",
            receiptItemCode: "BAN",
            category: "Produce",
          },
        ],
      }),
    );
    const user = userEvent.setup();
    const onOpenChange = vi.fn();
    renderWithQueryClient(
      <CommandPalette open={true} onOpenChange={onOpenChange} />,
    );
    const input = screen.getByPlaceholderText(/type a command or search/i);
    await user.type(input, "bananas");
    await user.click(screen.getByText("Organic bananas"));
    expect(navigateMock).toHaveBeenCalledWith("/receipts/r99");
    expect(onOpenChange).toHaveBeenCalledWith(false);
  });

  it("shows ⇧ N shortcut hint on the create command matching the current route", () => {
    renderWithQueryClient(
      <CommandPalette open={true} onOpenChange={vi.fn()} />,
      { route: "/accounts" },
    );
    const item = screen.getByText("New Account").closest("[cmdk-item]");
    expect(item).not.toBeNull();
    expect(within(item as HTMLElement).getByText("⇧ N")).toBeInTheDocument();
    // Screen readers see spelled-out text instead of the glyph
    expect(within(item as HTMLElement).getByText("Shift+N")).toBeInTheDocument();
  });

  it("does not show ⇧ N hint on create commands that don't match current route", () => {
    renderWithQueryClient(
      <CommandPalette open={true} onOpenChange={vi.fn()} />,
      { route: "/" },
    );
    const newAccount = screen.getByText("New Account").closest("[cmdk-item]");
    expect(
      within(newAccount as HTMLElement).queryByText("⇧ N"),
    ).not.toBeInTheDocument();
  });

  it("query state resets when the palette unmounts and remounts", async () => {
    // Layout mounts <CommandPalette> conditionally on `searchOpen`, so each
    // open is a fresh instance. Simulate that here with unmount + remount.
    const user = userEvent.setup();
    const onOpenChange = vi.fn();
    const { unmount } = renderWithQueryClient(
      <CommandPalette open={true} onOpenChange={onOpenChange} />,
    );
    await user.type(
      screen.getByPlaceholderText(/type a command or search/i),
      "xyz",
    );
    await user.keyboard("{Escape}");
    expect(onOpenChange).toHaveBeenCalledWith(false);
    unmount();
    renderWithQueryClient(
      <CommandPalette open={true} onOpenChange={onOpenChange} />,
    );
    expect(
      (
        screen.getByPlaceholderText(
          /type a command or search/i,
        ) as HTMLInputElement
      ).value,
    ).toBe("");
  });

  it("collapses large entity groups to a Show N more trailer", async () => {
    const { useCategories } = await import("@/hooks/useCategories");
    const many = Array.from({ length: 12 }, (_, i) => ({
      id: `c${i}`,
      name: `Cat ${i}`,
      description: null,
    }));
    vi.mocked(useCategories).mockReturnValue(mockQueryResult({ data: many }));
    const user = userEvent.setup();
    renderWithQueryClient(
      <CommandPalette open={true} onOpenChange={vi.fn()} />,
    );
    await user.type(
      screen.getByPlaceholderText(/type a command or search/i),
      "cat",
    );
    expect(screen.getByText("Cat 0")).toBeInTheDocument();
    expect(screen.queryByText("Cat 9")).not.toBeInTheDocument();
    const moreRow = screen.getByText(/Show 4 more categories/i);
    await user.click(moreRow);
    expect(screen.getByText("Cat 11")).toBeInTheDocument();
  });
});

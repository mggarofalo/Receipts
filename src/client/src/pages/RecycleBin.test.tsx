import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult } from "@/test/mock-hooks";
import RecycleBin from "./RecycleBin";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useAccounts", () => ({
  useDeletedAccounts: vi.fn(() => ({ data: { data: [], total: 0, offset: 0, limit: 50 }, isLoading: false })),
  useRestoreAccount: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

vi.mock("@/hooks/useCategories", () => ({
  useDeletedCategories: vi.fn(() => ({ data: { data: [], total: 0, offset: 0, limit: 50 }, isLoading: false })),
  useRestoreCategory: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

vi.mock("@/hooks/useSubcategories", () => ({
  useDeletedSubcategories: vi.fn(() => ({ data: { data: [], total: 0, offset: 0, limit: 50 }, isLoading: false })),
  useRestoreSubcategory: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

vi.mock("@/hooks/useReceipts", () => ({
  useDeletedReceipts: vi.fn(() => ({ data: { data: [], total: 0, offset: 0, limit: 50 }, isLoading: false })),
  useRestoreReceipt: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

vi.mock("@/hooks/useReceiptItems", () => ({
  useDeletedReceiptItems: vi.fn(() => ({ data: { data: [], total: 0, offset: 0, limit: 50 }, isLoading: false })),
  useRestoreReceiptItem: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

vi.mock("@/hooks/useTransactions", () => ({
  useDeletedTransactions: vi.fn(() => ({ data: { data: [], total: 0, offset: 0, limit: 50 }, isLoading: false })),
  useRestoreTransaction: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

vi.mock("@/hooks/useItemTemplates", () => ({
  useDeletedItemTemplates: vi.fn(() => ({ data: { data: [], total: 0, offset: 0, limit: 50 }, isLoading: false })),
  useRestoreItemTemplate: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

vi.mock("@/hooks/useTrash", () => ({
  usePurgeTrash: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
}));

vi.mock("@/hooks/useListKeyboardNav", () => ({
  useListKeyboardNav: vi.fn(() => ({
    focusedId: null,
    setFocusedIndex: vi.fn(),
    tableRef: { current: null },
  })),
}));

describe("RecycleBin", () => {
  it("renders the page heading", () => {
    renderWithProviders(<RecycleBin />);
    expect(
      screen.getByRole("heading", { name: /recycle bin/i }),
    ).toBeInTheDocument();
  });

  it("renders loading skeleton when data is loading", async () => {
    const { useDeletedAccounts } = await import("@/hooks/useAccounts");
    vi.mocked(useDeletedAccounts).mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: true,
    }));

    const { container } = renderWithProviders(<RecycleBin />);
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
  });

  it("renders the All tab", async () => {
    const { useDeletedAccounts } = await import("@/hooks/useAccounts");
    vi.mocked(useDeletedAccounts).mockReturnValue(mockQueryResult({
      data: { data: [], total: 0, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<RecycleBin />);
    expect(
      screen.getByRole("tab", { name: /all/i }),
    ).toBeInTheDocument();
  });

  it("renders the Empty Trash button", () => {
    renderWithProviders(<RecycleBin />);
    expect(
      screen.getByRole("button", { name: /empty trash/i }),
    ).toBeInTheDocument();
  });

  it("renders empty state when no deleted items exist", () => {
    renderWithProviders(<RecycleBin />);
    expect(
      screen.getByText(/no deleted items found/i),
    ).toBeInTheDocument();
  });

  it("renders deleted items in the table when data exists", async () => {
    const { useDeletedAccounts } = await import("@/hooks/useAccounts");
    vi.mocked(useDeletedAccounts).mockReturnValue(mockQueryResult({
      data: {
        data: [{ id: "a1", name: "Old Account", accountCode: "ACC-OLD" }],
        total: 1, offset: 0, limit: 50,
      },
      isLoading: false,
    }));

    const { useDeletedCategories } = await import("@/hooks/useCategories");
    vi.mocked(useDeletedCategories).mockReturnValue(mockQueryResult({
      data: {
        data: [{ id: "c1", name: "Deleted Category" }],
        total: 1, offset: 0, limit: 50,
      },
      isLoading: false,
    }));

    renderWithProviders(<RecycleBin />);
    expect(screen.getByText("Account")).toBeInTheDocument();
    expect(screen.getByText("Category")).toBeInTheDocument();
    expect(screen.getByText(/old account/i)).toBeInTheDocument();
    expect(screen.getByText("Deleted Category")).toBeInTheDocument();
  });

  it("renders entity type tabs when deleted items exist", async () => {
    const { useDeletedAccounts } = await import("@/hooks/useAccounts");
    vi.mocked(useDeletedAccounts).mockReturnValue(mockQueryResult({
      data: {
        data: [{ id: "a1", name: "Old Account", accountCode: "ACC-OLD" }],
        total: 1, offset: 0, limit: 50,
      },
      isLoading: false,
    }));

    renderWithProviders(<RecycleBin />);
    // Should have an "All" tab and an "Account" tab
    expect(screen.getByRole("tab", { name: /all/i })).toBeInTheDocument();
    expect(screen.getByRole("tab", { name: /account/i })).toBeInTheDocument();
  });

});

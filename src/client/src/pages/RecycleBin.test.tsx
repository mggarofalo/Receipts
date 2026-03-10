import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult, mockMutationResult } from "@/test/mock-hooks";
import RecycleBin from "./RecycleBin";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
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
  beforeEach(async () => {
    const { useDeletedReceipts, useRestoreReceipt } = await import("@/hooks/useReceipts");
    vi.mocked(useDeletedReceipts).mockReturnValue(mockQueryResult({
      data: { data: [], total: 0, offset: 0, limit: 50 },
      isLoading: false,
    }));
    vi.mocked(useRestoreReceipt).mockReturnValue(mockMutationResult());
  });

  it("renders the page heading", () => {
    renderWithProviders(<RecycleBin />);
    expect(
      screen.getByRole("heading", { name: /recycle bin/i }),
    ).toBeInTheDocument();
  });

  it("renders loading skeleton when data is loading", async () => {
    const { useDeletedReceipts } = await import("@/hooks/useReceipts");
    vi.mocked(useDeletedReceipts).mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: true,
    }));

    const { container } = renderWithProviders(<RecycleBin />);
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
  });

  it("renders the All tab", () => {
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
    const { useDeletedReceipts } = await import("@/hooks/useReceipts");
    vi.mocked(useDeletedReceipts).mockReturnValue(mockQueryResult({
      data: {
        data: [{ id: "r1", location: "Store", date: "2026-01-01" }],
        total: 1, offset: 0, limit: 50,
      },
      isLoading: false,
    }));

    const { useDeletedItemTemplates } = await import("@/hooks/useItemTemplates");
    vi.mocked(useDeletedItemTemplates).mockReturnValue(mockQueryResult({
      data: {
        data: [{ id: "it1", name: "Deleted Template" }],
        total: 1, offset: 0, limit: 50,
      },
      isLoading: false,
    }));

    renderWithProviders(<RecycleBin />);
    expect(screen.getByText("Receipt")).toBeInTheDocument();
    expect(screen.getByText("Item Template")).toBeInTheDocument();
    expect(screen.getByText("Store - 2026-01-01")).toBeInTheDocument();
    expect(screen.getByText("Deleted Template")).toBeInTheDocument();
  });

  it("renders entity type tabs when deleted items exist", async () => {
    const { useDeletedReceipts } = await import("@/hooks/useReceipts");
    vi.mocked(useDeletedReceipts).mockReturnValue(mockQueryResult({
      data: {
        data: [{ id: "r1", location: "Store", date: "2026-01-01" }],
        total: 1, offset: 0, limit: 50,
      },
      isLoading: false,
    }));

    renderWithProviders(<RecycleBin />);
    // Should have an "All" tab and a "Receipt" tab
    expect(screen.getByRole("tab", { name: /all/i })).toBeInTheDocument();
    expect(screen.getByRole("tab", { name: /receipt/i })).toBeInTheDocument();
  });

});

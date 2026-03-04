import { describe, it, expect, vi, beforeEach, type Mock } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { createElement, type ReactNode } from "react";

vi.mock("@/lib/api-client", () => ({
  default: {
    GET: vi.fn(),
    POST: vi.fn(),
    PUT: vi.fn(),
    DELETE: vi.fn(),
  },
}));

vi.mock("sonner", () => ({
  toast: { success: vi.fn(), error: vi.fn(), info: vi.fn() },
}));

import client from "@/lib/api-client";
import { toast } from "sonner";
import {
  useReceiptItems,
  useReceiptItem,
  useReceiptItemsByReceiptId,
  useCreateReceiptItem,
  useUpdateReceiptItem,
  useDeleteReceiptItems,
  useDeletedReceiptItems,
  useRestoreReceiptItem,
} from "./useReceiptItems";

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false, gcTime: 0 } },
  });
  return function Wrapper({ children }: { children: ReactNode }) {
    return createElement(QueryClientProvider, { client: queryClient }, children);
  };
}

beforeEach(() => {
  vi.clearAllMocks();
});

describe("useReceiptItems", () => {
  it("list query returns data on success", async () => {
    const items = [
      {
        id: "1",
        receiptItemCode: "RI-1",
        description: "Apples",
        quantity: 3,
        unitPrice: 1.5,
        category: "Food",
        subcategory: "Produce",
        pricingMode: "quantity",
      },
    ];
    (client.GET as Mock).mockResolvedValue({ data: items, error: undefined });

    const { result } = renderHook(() => useReceiptItems(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(items);
    expect(client.GET).toHaveBeenCalledWith("/api/receipt-items", {
      params: { query: { offset: 0, limit: 50 } },
    });
  });

  it("single query is disabled when id is null", () => {
    const { result } = renderHook(() => useReceiptItem(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.data).toBeUndefined();
    expect(result.current.fetchStatus).toBe("idle");
    expect(client.GET).not.toHaveBeenCalled();
  });

  it("by-receipt query returns data when receiptId is provided", async () => {
    const items = [{ id: "1", receiptItemCode: "RI-1", description: "Apples" }];
    (client.GET as Mock).mockResolvedValue({ data: items, error: undefined });

    const { result } = renderHook(() => useReceiptItemsByReceiptId("r-1"), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(items);
    expect(client.GET).toHaveBeenCalledWith(
      "/api/receipt-items",
      { params: { query: { receiptId: "r-1", offset: 0, limit: 200 } } },
    );
  });

  it("by-receipt query is disabled when receiptId is null", () => {
    const { result } = renderHook(() => useReceiptItemsByReceiptId(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.data).toBeUndefined();
    expect(result.current.fetchStatus).toBe("idle");
  });

  it("create mutation calls POST and shows toast on success", async () => {
    const body = {
      receiptItemCode: "RI-2",
      description: "Bananas",
      quantity: 6,
      unitPrice: 0.5,
      category: "Food",
      subcategory: "Produce",
      pricingMode: "quantity" as const,
    };
    const created = { id: "2", ...body };
    (client.POST as Mock).mockResolvedValue({ data: created, error: undefined });

    const { result } = renderHook(() => useCreateReceiptItem(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync({ receiptId: "r-1", body });

    expect(client.POST).toHaveBeenCalledWith("/api/receipts/{receiptId}/receipt-items", {
      params: { path: { receiptId: "r-1" } },
      body,
    });
    expect(toast.success).toHaveBeenCalledWith("Receipt item created");
  });

  it("update mutation calls PUT and shows toast on success", async () => {
    const body = {
      id: "1",
      receiptItemCode: "RI-1",
      description: "Apples (updated)",
      quantity: 5,
      unitPrice: 1.75,
      category: "Food",
      subcategory: "Produce",
      pricingMode: "quantity" as const,
    };
    (client.PUT as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useUpdateReceiptItem(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync({ body });

    expect(client.PUT).toHaveBeenCalledWith("/api/receipt-items/{id}", {
      params: { path: { id: body.id } },
      body,
    });
    expect(toast.success).toHaveBeenCalledWith("Receipt item updated");
  });

  it("delete mutation calls DELETE", async () => {
    (client.DELETE as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useDeleteReceiptItems(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(["1"]);

    expect(client.DELETE).toHaveBeenCalledWith("/api/receipt-items", {
      body: ["1"],
    });
    expect(toast.success).toHaveBeenCalledWith("Receipt item(s) deleted");
  });

  it("deleted receipt items query returns data on success", async () => {
    const deleted = [{ id: "3", receiptItemCode: "DEL", description: "Old item" }];
    (client.GET as Mock).mockResolvedValue({ data: deleted, error: undefined });

    const { result } = renderHook(() => useDeletedReceiptItems(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(deleted);
    expect(client.GET).toHaveBeenCalledWith("/api/receipt-items/deleted", {
      params: { query: { offset: 0, limit: 50 } },
    });
  });

  it("restore mutation calls POST and shows toast on success", async () => {
    (client.POST as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useRestoreReceiptItem(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync("1");

    expect(client.POST).toHaveBeenCalledWith("/api/receipt-items/{id}/restore", {
      params: { path: { id: "1" } },
    });
    expect(toast.success).toHaveBeenCalledWith("Receipt item restored");
  });
});

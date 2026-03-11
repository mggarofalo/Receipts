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
  useReceipts,
  useReceipt,
  useCreateReceipt,
  useUpdateReceipt,
  useDeleteReceipts,
  useDeletedReceipts,
  useRestoreReceipt,
} from "./useReceipts";

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

describe("useReceipts", () => {
  it("list query returns data on success", async () => {
    const receipts = [
      { id: "1", location: "Walmart", date: "2025-01-01", taxAmount: 5.0 },
    ];
    (client.GET as Mock).mockResolvedValue({ data: { data: receipts, total: 1, offset: 0, limit: 50 }, error: undefined });

    const { result } = renderHook(() => useReceipts(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(receipts);
    expect(client.GET).toHaveBeenCalledWith("/api/receipts", {
      params: { query: { offset: 0, limit: 50 } },
    });
  });

  it("single query is disabled when id is null", () => {
    const { result } = renderHook(() => useReceipt(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.data).toBeUndefined();
    expect(result.current.fetchStatus).toBe("idle");
    expect(client.GET).not.toHaveBeenCalled();
  });

  it("single query fetches data when id is provided", async () => {
    const receipt = { id: "1", location: "Walmart", date: "2025-01-01", taxAmount: 5.0 };
    (client.GET as Mock).mockResolvedValue({ data: receipt, error: undefined });

    const { result } = renderHook(() => useReceipt("1"), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(receipt);
  });

  it("create mutation calls POST and shows toast on success", async () => {
    const newReceipt = {
      location: "Target",
      date: "2025-02-01",
      taxAmount: 3.5,
      description: null,
    };
    const created = { id: "2", ...newReceipt };
    (client.POST as Mock).mockResolvedValue({ data: created, error: undefined });

    const { result } = renderHook(() => useCreateReceipt(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(newReceipt);

    expect(client.POST).toHaveBeenCalledWith("/api/receipts", { body: newReceipt });
    expect(toast.success).toHaveBeenCalledWith("Receipt created");
  });

  it("update mutation calls PUT and shows toast on success", async () => {
    const updated = {
      id: "1",
      location: "Walmart Updated",
      date: "2025-01-02",
      taxAmount: 6.0,
    };
    (client.PUT as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useUpdateReceipt(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(updated);

    expect(client.PUT).toHaveBeenCalledWith("/api/receipts/{id}", {
      params: { path: { id: "1" } },
      body: updated,
    });
    expect(toast.success).toHaveBeenCalledWith("Receipt updated");
  });

  it("delete mutation calls DELETE", async () => {
    (client.DELETE as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useDeleteReceipts(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(["1", "2"]);

    expect(client.DELETE).toHaveBeenCalledWith("/api/receipts", {
      body: ["1", "2"],
    });
    expect(toast.success).toHaveBeenCalledWith("Receipt(s) deleted");
  });

  it("deleted receipts query returns data on success", async () => {
    const deleted = [{ id: "3", location: "Old Store", date: "2024-01-01", taxAmount: 0 }];
    (client.GET as Mock).mockResolvedValue({ data: { data: deleted, total: 1, offset: 0, limit: 50 }, error: undefined });

    const { result } = renderHook(() => useDeletedReceipts(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(deleted);
    expect(client.GET).toHaveBeenCalledWith("/api/receipts/deleted", {
      params: { query: { offset: 0, limit: 50 } },
    });
  });

  it("restore mutation calls POST and shows toast on success", async () => {
    (client.POST as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useRestoreReceipt(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync("1");

    expect(client.POST).toHaveBeenCalledWith("/api/receipts/{id}/restore", {
      params: { path: { id: "1" } },
    });
    expect(toast.success).toHaveBeenCalledWith("Receipt restored");
  });
});

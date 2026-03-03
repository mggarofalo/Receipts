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
  useAdjustments,
  useAdjustment,
  useAdjustmentsByReceiptId,
  useCreateAdjustment,
  useUpdateAdjustment,
  useDeleteAdjustments,
  useDeletedAdjustments,
  useRestoreAdjustment,
} from "./useAdjustments";

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

describe("useAdjustments", () => {
  it("list query returns data on success", async () => {
    const adjustments = [
      { id: "1", type: "discount", amount: 5.0, description: "Coupon" },
    ];
    (client.GET as Mock).mockResolvedValue({ data: adjustments, error: undefined });

    const { result } = renderHook(() => useAdjustments(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(adjustments);
    expect(client.GET).toHaveBeenCalledWith("/api/adjustments");
  });

  it("single query is disabled when id is null", () => {
    const { result } = renderHook(() => useAdjustment(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.data).toBeUndefined();
    expect(result.current.fetchStatus).toBe("idle");
    expect(client.GET).not.toHaveBeenCalled();
  });

  it("by-receipt query returns data when receiptId is provided", async () => {
    const items = [{ id: "1", type: "discount", amount: 5.0 }];
    (client.GET as Mock).mockResolvedValue({ data: items, error: undefined });

    const { result } = renderHook(() => useAdjustmentsByReceiptId("r-1"), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(items);
    expect(client.GET).toHaveBeenCalledWith(
      "/api/adjustments/by-receipt-id/{receiptId}",
      { params: { path: { receiptId: "r-1" } } },
    );
  });

  it("by-receipt query is disabled when receiptId is null", () => {
    const { result } = renderHook(() => useAdjustmentsByReceiptId(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.data).toBeUndefined();
    expect(result.current.fetchStatus).toBe("idle");
  });

  it("create mutation calls POST and shows toast on success", async () => {
    const body = { type: "surcharge", amount: 2.0, description: "Service fee" };
    const created = { id: "2", ...body };
    (client.POST as Mock).mockResolvedValue({ data: created, error: undefined });

    const { result } = renderHook(() => useCreateAdjustment(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync({ receiptId: "r-1", body });

    expect(client.POST).toHaveBeenCalledWith("/api/adjustments/{receiptId}", {
      params: { path: { receiptId: "r-1" } },
      body,
    });
    expect(toast.success).toHaveBeenCalledWith("Adjustment created");
  });

  it("update mutation calls PUT and shows toast on success", async () => {
    const body = {
      id: "1",
      type: "discount",
      amount: 10.0,
      description: "Updated coupon",
    };
    (client.PUT as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useUpdateAdjustment(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync({ receiptId: "r-1", body });

    expect(client.PUT).toHaveBeenCalledWith("/api/adjustments/{receiptId}", {
      params: { path: { receiptId: "r-1" } },
      body,
    });
    expect(toast.success).toHaveBeenCalledWith("Adjustment updated");
  });

  it("delete mutation calls DELETE", async () => {
    (client.DELETE as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useDeleteAdjustments(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(["1"]);

    expect(client.DELETE).toHaveBeenCalledWith("/api/adjustments", {
      body: ["1"],
    });
    expect(toast.success).toHaveBeenCalledWith("Adjustment(s) deleted");
  });

  it("deleted adjustments query returns data on success", async () => {
    const deleted = [{ id: "3", type: "discount", amount: 1.0 }];
    (client.GET as Mock).mockResolvedValue({ data: deleted, error: undefined });

    const { result } = renderHook(() => useDeletedAdjustments(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(deleted);
    expect(client.GET).toHaveBeenCalledWith("/api/adjustments/deleted");
  });

  it("restore mutation calls POST and shows toast on success", async () => {
    (client.POST as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useRestoreAdjustment(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync("1");

    expect(client.POST).toHaveBeenCalledWith("/api/adjustments/{id}/restore", {
      params: { path: { id: "1" } },
    });
    expect(toast.success).toHaveBeenCalledWith("Adjustment restored");
  });
});

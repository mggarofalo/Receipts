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
  useTransactions,
  useTransaction,
  useTransactionsByReceiptId,
  useCreateTransaction,
  useUpdateTransaction,
  useDeleteTransactions,
  useDeletedTransactions,
  useRestoreTransaction,
} from "./useTransactions";

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

describe("useTransactions", () => {
  it("list query returns data on success", async () => {
    const transactions = [
      { id: "1", amount: 100, date: "2025-01-01" },
    ];
    (client.GET as Mock).mockResolvedValue({ data: transactions, error: undefined });

    const { result } = renderHook(() => useTransactions(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(transactions);
    expect(client.GET).toHaveBeenCalledWith("/api/transactions", {
      params: { query: { offset: 0, limit: 50 } },
    });
  });

  it("single query is disabled when id is null", () => {
    const { result } = renderHook(() => useTransaction(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.data).toBeUndefined();
    expect(result.current.fetchStatus).toBe("idle");
    expect(client.GET).not.toHaveBeenCalled();
  });

  it("by-receipt query returns data when receiptId is provided", async () => {
    const items = [{ id: "1", amount: 50, date: "2025-01-01" }];
    (client.GET as Mock).mockResolvedValue({ data: items, error: undefined });

    const { result } = renderHook(() => useTransactionsByReceiptId("r-1"), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(items);
    expect(client.GET).toHaveBeenCalledWith(
      "/api/transactions",
      { params: { query: { receiptId: "r-1", offset: 0, limit: 200 } } },
    );
  });

  it("by-receipt query is disabled when receiptId is null", () => {
    const { result } = renderHook(() => useTransactionsByReceiptId(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.data).toBeUndefined();
    expect(result.current.fetchStatus).toBe("idle");
  });

  it("create mutation calls POST and shows toast on success", async () => {
    const body = { amount: 200, date: "2025-03-01" };
    const created = { id: "2", ...body };
    (client.POST as Mock).mockResolvedValue({ data: created, error: undefined });

    const { result } = renderHook(() => useCreateTransaction(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync({
      receiptId: "r-1",
      body: { ...body, accountId: "acc-1" },
    });

    expect(client.POST).toHaveBeenCalledWith(
      "/api/receipts/{receiptId}/transactions",
      { params: { path: { receiptId: "r-1" } }, body: { ...body, accountId: "acc-1" } },
    );
    expect(toast.success).toHaveBeenCalledWith("Transaction created");
  });

  it("update mutation calls PUT and shows toast on success", async () => {
    const body = { id: "1", amount: 250, date: "2025-03-02", accountId: "acc-1" };
    (client.PUT as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useUpdateTransaction(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync({
      body,
    });

    expect(client.PUT).toHaveBeenCalledWith(
      "/api/transactions/{id}",
      { params: { path: { id: "1" } }, body },
    );
    expect(toast.success).toHaveBeenCalledWith("Transaction updated");
  });

  it("delete mutation calls DELETE", async () => {
    (client.DELETE as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useDeleteTransactions(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(["1", "2"]);

    expect(client.DELETE).toHaveBeenCalledWith("/api/transactions", {
      body: ["1", "2"],
    });
    expect(toast.success).toHaveBeenCalledWith("Transaction(s) deleted");
  });

  it("deleted transactions query returns data on success", async () => {
    const deleted = [{ id: "3", amount: 10, date: "2024-01-01" }];
    (client.GET as Mock).mockResolvedValue({ data: deleted, error: undefined });

    const { result } = renderHook(() => useDeletedTransactions(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(deleted);
    expect(client.GET).toHaveBeenCalledWith("/api/transactions/deleted", {
      params: { query: { offset: 0, limit: 50 } },
    });
  });

  it("restore mutation calls POST and shows toast on success", async () => {
    (client.POST as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useRestoreTransaction(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync("1");

    expect(client.POST).toHaveBeenCalledWith("/api/transactions/{id}/restore", {
      params: { path: { id: "1" } },
    });
    expect(toast.success).toHaveBeenCalledWith("Transaction restored");
  });
});

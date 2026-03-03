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
import {
  useReceiptWithItems,
  useTransactionAccount,
  useTransactionAccountsByReceiptId,
} from "./useAggregates";

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

describe("useReceiptWithItems", () => {
  it("fetches receipt with items when receiptId is provided", async () => {
    const mockData = { id: "r1", items: [{ id: "i1" }] };
    (client.GET as Mock).mockResolvedValue({ data: mockData, error: null });

    const { result } = renderHook(() => useReceiptWithItems("r1"), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith(
      "/api/receipts-with-items/by-receipt-id/{receiptId}",
      { params: { path: { receiptId: "r1" } } },
    );
    expect(result.current.data).toEqual(mockData);
  });

  it("does not fetch when receiptId is null", () => {
    const { result } = renderHook(() => useReceiptWithItems(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.fetchStatus).toBe("idle");
    expect(client.GET).not.toHaveBeenCalled();
  });
});

describe("useTransactionAccount", () => {
  it("fetches transaction account when transactionId is provided", async () => {
    const mockData = { id: "ta1", accountId: "a1" };
    (client.GET as Mock).mockResolvedValue({ data: mockData, error: null });

    const { result } = renderHook(() => useTransactionAccount("t1"), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith(
      "/api/transaction-accounts/by-transaction-id/{transactionId}",
      { params: { path: { transactionId: "t1" } } },
    );
    expect(result.current.data).toEqual(mockData);
  });

  it("does not fetch when transactionId is null", () => {
    const { result } = renderHook(() => useTransactionAccount(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.fetchStatus).toBe("idle");
    expect(client.GET).not.toHaveBeenCalled();
  });
});

describe("useTransactionAccountsByReceiptId", () => {
  it("fetches transaction accounts by receiptId", async () => {
    const mockData = [{ id: "ta1" }, { id: "ta2" }];
    (client.GET as Mock).mockResolvedValue({ data: mockData, error: null });

    const { result } = renderHook(
      () => useTransactionAccountsByReceiptId("r1"),
      { wrapper: createWrapper() },
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith(
      "/api/transaction-accounts/by-receipt-id/{receiptId}",
      { params: { path: { receiptId: "r1" } } },
    );
    expect(result.current.data).toEqual(mockData);
  });

  it("does not fetch when receiptId is null", () => {
    const { result } = renderHook(
      () => useTransactionAccountsByReceiptId(null),
      { wrapper: createWrapper() },
    );

    expect(result.current.fetchStatus).toBe("idle");
    expect(client.GET).not.toHaveBeenCalled();
  });
});

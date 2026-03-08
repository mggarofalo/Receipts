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
import { useEntityAuditHistory, useRecentAuditLogs } from "./useAudit";

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

describe("useEntityAuditHistory", () => {
  it("fetches audit history when both entityType and entityId are provided", async () => {
    const mockHistory = [{ id: "a1", action: "Created" }];
    (client.GET as Mock).mockResolvedValue({
      data: mockHistory,
      error: null,
    });

    const { result } = renderHook(
      () => useEntityAuditHistory("Receipt", "r1"),
      { wrapper: createWrapper() },
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith("/api/audit", {
      params: {
        query: {
          entityType: "Receipt",
          entityId: "r1",
          offset: 0,
          limit: 50,
          sortBy: undefined,
          sortDirection: undefined,
        },
      },
    });
    expect(result.current.data).toEqual(mockHistory);
  });

  it("does not fetch when entityType is null", () => {
    const { result } = renderHook(() => useEntityAuditHistory(null, "r1"), {
      wrapper: createWrapper(),
    });

    expect(result.current.fetchStatus).toBe("idle");
    expect(client.GET).not.toHaveBeenCalled();
  });

  it("does not fetch when entityId is null", () => {
    const { result } = renderHook(
      () => useEntityAuditHistory("Receipt", null),
      { wrapper: createWrapper() },
    );

    expect(result.current.fetchStatus).toBe("idle");
    expect(client.GET).not.toHaveBeenCalled();
  });
});

describe("useRecentAuditLogs", () => {
  it("fetches recent audit logs with default params", async () => {
    const mockResponse = { data: [{ id: "a1", action: "Updated" }], total: 1, offset: 0, limit: 50 };
    (client.GET as Mock).mockResolvedValue({ data: mockResponse, error: null });

    const { result } = renderHook(() => useRecentAuditLogs(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith("/api/audit/recent", {
      params: {
        query: {
          offset: 0,
          limit: 50,
          sortBy: undefined,
          sortDirection: undefined,
          entityType: undefined,
          action: undefined,
          search: undefined,
          dateFrom: undefined,
          dateTo: undefined,
        },
      },
    });
    expect(result.current.data).toEqual(mockResponse);
  });

  it("fetches recent audit logs with custom filters", async () => {
    const mockResponse = { data: [{ id: "a1" }], total: 1, offset: 0, limit: 25 };
    (client.GET as Mock).mockResolvedValue({ data: mockResponse, error: null });

    const { result } = renderHook(
      () =>
        useRecentAuditLogs({
          offset: 10,
          limit: 25,
          sortBy: "changedAt",
          sortDirection: "desc",
          entityType: "Account",
          action: "Create",
          search: "abc",
        }),
      {
        wrapper: createWrapper(),
      },
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith("/api/audit/recent", {
      params: {
        query: {
          offset: 10,
          limit: 25,
          sortBy: "changedAt",
          sortDirection: "desc",
          entityType: "Account",
          action: "Create",
          search: "abc",
          dateFrom: undefined,
          dateTo: undefined,
        },
      },
    });
  });

  it("converts null filter values to undefined", async () => {
    const mockResponse = { data: [], total: 0, offset: 0, limit: 50 };
    (client.GET as Mock).mockResolvedValue({ data: mockResponse, error: null });

    const { result } = renderHook(
      () =>
        useRecentAuditLogs({
          entityType: null,
          action: null,
          search: null,
          dateFrom: null,
          dateTo: null,
        }),
      {
        wrapper: createWrapper(),
      },
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith("/api/audit/recent", {
      params: {
        query: expect.objectContaining({
          entityType: undefined,
          action: undefined,
          search: undefined,
          dateFrom: undefined,
          dateTo: undefined,
        }),
      },
    });
  });
});

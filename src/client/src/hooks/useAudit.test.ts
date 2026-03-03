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

    expect(client.GET).toHaveBeenCalledWith(
      "/api/audit/entity/{entityType}/{entityId}",
      { params: { path: { entityType: "Receipt", entityId: "r1" } } },
    );
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
  it("fetches recent audit logs with default count", async () => {
    const mockLogs = [{ id: "a1", action: "Updated" }];
    (client.GET as Mock).mockResolvedValue({ data: mockLogs, error: null });

    const { result } = renderHook(() => useRecentAuditLogs(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith("/api/audit/recent", {
      params: { query: { count: 50 } },
    });
    expect(result.current.data).toEqual(mockLogs);
  });

  it("fetches recent audit logs with custom count", async () => {
    const mockLogs = [{ id: "a1" }];
    (client.GET as Mock).mockResolvedValue({ data: mockLogs, error: null });

    const { result } = renderHook(() => useRecentAuditLogs(25), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith("/api/audit/recent", {
      params: { query: { count: 25 } },
    });
  });
});

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
  useMyAuthAuditLog,
  useRecentAuthAuditLogs,
  useFailedAuthAttempts,
} from "./useAuthAudit";

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

describe("useMyAuthAuditLog", () => {
  it("fetches my auth audit logs with default count", async () => {
    const mockLogs = [{ id: "log1", event: "Login" }];
    (client.GET as Mock).mockResolvedValue({ data: mockLogs, error: null });

    const { result } = renderHook(() => useMyAuthAuditLog(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith("/api/auth/audit/me", {
      params: { query: { count: 50 } },
    });
    expect(result.current.data).toEqual(mockLogs);
  });

  it("fetches my auth audit logs with custom count", async () => {
    const mockLogs = [{ id: "log1" }];
    (client.GET as Mock).mockResolvedValue({ data: mockLogs, error: null });

    const { result } = renderHook(() => useMyAuthAuditLog(10), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith("/api/auth/audit/me", {
      params: { query: { count: 10 } },
    });
  });
});

describe("useRecentAuthAuditLogs", () => {
  it("fetches recent auth audit logs with default count", async () => {
    const mockLogs = [{ id: "log2", event: "Logout" }];
    (client.GET as Mock).mockResolvedValue({ data: mockLogs, error: null });

    const { result } = renderHook(() => useRecentAuthAuditLogs(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith("/api/auth/audit/recent", {
      params: { query: { count: 50 } },
    });
    expect(result.current.data).toEqual(mockLogs);
  });

  it("fetches recent auth audit logs with custom count", async () => {
    (client.GET as Mock).mockResolvedValue({ data: [], error: null });

    const { result } = renderHook(() => useRecentAuthAuditLogs(5), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith("/api/auth/audit/recent", {
      params: { query: { count: 5 } },
    });
  });
});

describe("useFailedAuthAttempts", () => {
  it("fetches failed auth attempts with default count", async () => {
    const mockAttempts = [{ id: "f1", reason: "InvalidPassword" }];
    (client.GET as Mock).mockResolvedValue({
      data: mockAttempts,
      error: null,
    });

    const { result } = renderHook(() => useFailedAuthAttempts(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith("/api/auth/audit/failed", {
      params: { query: { count: 50 } },
    });
    expect(result.current.data).toEqual(mockAttempts);
  });

  it("fetches failed auth attempts with custom count", async () => {
    (client.GET as Mock).mockResolvedValue({ data: [], error: null });

    const { result } = renderHook(() => useFailedAuthAttempts(100), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith("/api/auth/audit/failed", {
      params: { query: { count: 100 } },
    });
  });
});

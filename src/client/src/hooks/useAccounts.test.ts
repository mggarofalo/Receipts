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
  useAccounts,
  useAccount,
  useCreateAccount,
  useUpdateAccount,
  useDeleteAccount,
} from "./useAccounts";

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

describe("useAccounts", () => {
  it("list query returns data on success", async () => {
    const accounts = [
      { id: "1", accountCode: "ACC1", name: "Checking", isActive: true },
    ];
    (client.GET as Mock).mockResolvedValue({ data: { data: accounts, total: 1, offset: 0, limit: 50 }, error: undefined });

    const { result } = renderHook(() => useAccounts(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(accounts);
    expect(client.GET).toHaveBeenCalledWith("/api/accounts", {
      params: { query: { offset: 0, limit: 50 } },
    });
  });

  it("list query passes isActive filter to API", async () => {
    const accounts = [
      { id: "1", accountCode: "ACC1", name: "Checking", isActive: true },
    ];
    (client.GET as Mock).mockResolvedValue({ data: { data: accounts, total: 1, offset: 0, limit: 50 }, error: undefined });

    const { result } = renderHook(() => useAccounts(0, 50, null, null, true), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(client.GET).toHaveBeenCalledWith("/api/accounts", {
      params: { query: { offset: 0, limit: 50, isActive: true } },
    });
  });

  it("list query omits isActive when null", async () => {
    const accounts = [
      { id: "1", accountCode: "ACC1", name: "Checking", isActive: true },
    ];
    (client.GET as Mock).mockResolvedValue({ data: { data: accounts, total: 1, offset: 0, limit: 50 }, error: undefined });

    const { result } = renderHook(() => useAccounts(0, 50, null, null, null), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(client.GET).toHaveBeenCalledWith("/api/accounts", {
      params: { query: { offset: 0, limit: 50 } },
    });
  });

  it("single query is disabled when id is null", () => {
    const { result } = renderHook(() => useAccount(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.data).toBeUndefined();
    expect(result.current.fetchStatus).toBe("idle");
    expect(client.GET).not.toHaveBeenCalled();
  });

  it("single query fetches data when id is provided", async () => {
    const account = { id: "1", accountCode: "ACC1", name: "Checking", isActive: true };
    (client.GET as Mock).mockResolvedValue({ data: account, error: undefined });

    const { result } = renderHook(() => useAccount("1"), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(account);
  });

  it("create mutation calls POST and shows toast on success", async () => {
    const newAccount = { accountCode: "ACC2", name: "Savings", isActive: true };
    const created = { id: "2", ...newAccount };
    (client.POST as Mock).mockResolvedValue({ data: created, error: undefined });

    const { result } = renderHook(() => useCreateAccount(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(newAccount);

    expect(client.POST).toHaveBeenCalledWith("/api/accounts", { body: newAccount });
    expect(toast.success).toHaveBeenCalledWith("Account created");
  });

  it("update mutation calls PUT and shows toast on success", async () => {
    const updated = { id: "1", accountCode: "ACC1", name: "Updated", isActive: false };
    (client.PUT as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useUpdateAccount(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(updated);

    expect(client.PUT).toHaveBeenCalledWith("/api/accounts/{id}", {
      params: { path: { id: "1" } },
      body: updated,
    });
    expect(toast.success).toHaveBeenCalledWith("Account updated");
  });

  it("delete mutation calls DELETE and shows toast on success", async () => {
    (client.DELETE as Mock).mockResolvedValue({ error: undefined, response: { status: 204 } });

    const { result } = renderHook(() => useDeleteAccount(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync("1");

    expect(client.DELETE).toHaveBeenCalledWith("/api/accounts/{id}", {
      params: { path: { id: "1" } },
    });
    expect(toast.success).toHaveBeenCalledWith("Account deleted");
  });

  it("delete mutation shows conflict toast on 409", async () => {
    (client.DELETE as Mock).mockResolvedValue({
      error: { message: "Cannot delete — 3 transaction(s) reference this account", transactionCount: 3 },
      response: { status: 409 },
    });

    const { result } = renderHook(() => useDeleteAccount(), {
      wrapper: createWrapper(),
    });

    await expect(result.current.mutateAsync("1")).rejects.toThrow();

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith(
        "Cannot delete — 3 transaction(s) reference this account",
      );
    });
  });

  it("delete mutation shows generic error toast on non-409 failure", async () => {
    (client.DELETE as Mock).mockResolvedValue({
      error: { message: "Server error" },
      response: { status: 500 },
    });

    const { result } = renderHook(() => useDeleteAccount(), {
      wrapper: createWrapper(),
    });

    await expect(result.current.mutateAsync("1")).rejects.toThrow();

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith("Failed to delete account");
    });
  });

});

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
  useUsers,
  useUser,
  useCreateUser,
  useUpdateUser,
  useDeleteUser,
  useResetUserPassword,
} from "./useUsers";

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

describe("useUsers", () => {
  it("fetches paginated users with page and pageSize", async () => {
    const mockUsers = [{ id: "1", email: "a@b.com" }];
    (client.GET as Mock).mockResolvedValue({ data: mockUsers, error: null });

    const { result } = renderHook(() => useUsers(0, 10), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith("/api/users", {
      params: { query: { offset: 0, limit: 10 } },
    });
    expect(result.current.data).toEqual(mockUsers);
  });

  it("throws when GET returns an error", async () => {
    (client.GET as Mock).mockResolvedValue({
      data: null,
      error: { message: "Forbidden" },
    });

    const { result } = renderHook(() => useUsers(0, 10), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
  });
});

describe("useUser", () => {
  it("fetches a single user when userId is provided", async () => {
    const mockUser = { id: "u1", email: "user@test.com" };
    (client.GET as Mock).mockResolvedValue({ data: mockUser, error: null });

    const { result } = renderHook(() => useUser("u1"), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.GET).toHaveBeenCalledWith("/api/users/{userId}", {
      params: { path: { userId: "u1" } },
    });
    expect(result.current.data).toEqual(mockUser);
  });

  it("does not fetch when userId is null", () => {
    const { result } = renderHook(() => useUser(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.fetchStatus).toBe("idle");
    expect(client.GET).not.toHaveBeenCalled();
  });
});

describe("useCreateUser", () => {
  it("posts a new user and shows success toast", async () => {
    const newUser = { id: "u2", email: "new@test.com" };
    (client.POST as Mock).mockResolvedValue({ data: newUser, error: null });

    const { result } = renderHook(() => useCreateUser(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      email: "new@test.com",
      password: "P@ss1",
      role: "User",
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.POST).toHaveBeenCalledWith("/api/users", {
      body: { email: "new@test.com", password: "P@ss1", role: "User" },
    });
    expect(toast.success).toHaveBeenCalledWith("User created");
  });
});

describe("useUpdateUser", () => {
  it("puts user update and shows success toast", async () => {
    (client.PUT as Mock).mockResolvedValue({ error: null });

    const { result } = renderHook(() => useUpdateUser(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      userId: "u1",
      body: { email: "updated@test.com", role: "Admin", isDisabled: false },
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.PUT).toHaveBeenCalledWith("/api/users/{userId}", {
      params: { path: { userId: "u1" } },
      body: { email: "updated@test.com", role: "Admin", isDisabled: false },
    });
    expect(toast.success).toHaveBeenCalledWith("User updated");
  });
});

describe("useDeleteUser", () => {
  it("deletes user and shows success toast", async () => {
    (client.DELETE as Mock).mockResolvedValue({ error: null });

    const { result } = renderHook(() => useDeleteUser(), {
      wrapper: createWrapper(),
    });

    result.current.mutate("u1");

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.DELETE).toHaveBeenCalledWith("/api/users/{userId}", {
      params: { path: { userId: "u1" } },
    });
    expect(toast.success).toHaveBeenCalledWith("User deactivated");
  });
});

describe("useResetUserPassword", () => {
  it("posts password reset and shows success toast", async () => {
    (client.POST as Mock).mockResolvedValue({ error: null });

    const { result } = renderHook(() => useResetUserPassword(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ userId: "u1", newPassword: "NewP@ss1" });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(client.POST).toHaveBeenCalledWith(
      "/api/users/{userId}/reset-password",
      {
        params: { path: { userId: "u1" } },
        body: { newPassword: "NewP@ss1" },
      },
    );
    expect(toast.success).toHaveBeenCalledWith("Password reset successfully");
  });
});

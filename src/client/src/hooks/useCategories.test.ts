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
  useCategories,
  useCategory,
  useCreateCategory,
  useUpdateCategory,
} from "./useCategories";

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

describe("useCategories", () => {
  it("list query returns data on success", async () => {
    const categories = [{ id: "1", name: "Food", description: "Groceries" }];
    (client.GET as Mock).mockResolvedValue({ data: { data: categories, total: 1, offset: 0, limit: 50 }, error: undefined });

    const { result } = renderHook(() => useCategories(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(categories);
    expect(client.GET).toHaveBeenCalledWith("/api/categories", {
      params: { query: { offset: 0, limit: 50 } },
    });
  });

  it("single query is disabled when id is null", () => {
    const { result } = renderHook(() => useCategory(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.data).toBeUndefined();
    expect(result.current.fetchStatus).toBe("idle");
    expect(client.GET).not.toHaveBeenCalled();
  });

  it("single query fetches data when id is provided", async () => {
    const category = { id: "1", name: "Food", description: "Groceries" };
    (client.GET as Mock).mockResolvedValue({ data: category, error: undefined });

    const { result } = renderHook(() => useCategory("1"), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(category);
  });

  it("create mutation calls POST and shows toast on success", async () => {
    const newCategory = { name: "Travel", description: "Business travel", isActive: true };
    const created = { id: "2", ...newCategory };
    (client.POST as Mock).mockResolvedValue({ data: created, error: undefined });

    const { result } = renderHook(() => useCreateCategory(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(newCategory);

    expect(client.POST).toHaveBeenCalledWith("/api/categories", { body: newCategory });
    expect(toast.success).toHaveBeenCalledWith("Category created");
  });

  it("update mutation calls PUT and shows toast on success", async () => {
    const updated = { id: "1", name: "Food & Drink", description: "Updated", isActive: true };
    (client.PUT as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useUpdateCategory(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(updated);

    expect(client.PUT).toHaveBeenCalledWith("/api/categories/{id}", {
      params: { path: { id: "1" } },
      body: updated,
    });
    expect(toast.success).toHaveBeenCalledWith("Category updated");
  });

});

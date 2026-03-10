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
  useSubcategories,
  useSubcategory,
  useSubcategoriesByCategoryId,
  useCreateSubcategory,
  useUpdateSubcategory,
} from "./useSubcategories";

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

describe("useSubcategories", () => {
  it("list query returns data on success", async () => {
    const subcategories = [
      { id: "1", name: "Produce", categoryId: "cat-1", description: null },
    ];
    (client.GET as Mock).mockResolvedValue({
      data: subcategories,
      error: undefined,
    });

    const { result } = renderHook(() => useSubcategories(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(subcategories);
    expect(client.GET).toHaveBeenCalledWith("/api/subcategories", {
      params: { query: { offset: 0, limit: 50 } },
    });
  });

  it("single query is disabled when id is null", () => {
    const { result } = renderHook(() => useSubcategory(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.data).toBeUndefined();
    expect(result.current.fetchStatus).toBe("idle");
    expect(client.GET).not.toHaveBeenCalled();
  });

  it("single query fetches data when id is provided", async () => {
    const subcategory = { id: "1", name: "Produce", categoryId: "cat-1" };
    (client.GET as Mock).mockResolvedValue({
      data: subcategory,
      error: undefined,
    });

    const { result } = renderHook(() => useSubcategory("1"), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(subcategory);
  });

  it("by-category query returns data when categoryId is provided", async () => {
    const items = [{ id: "1", name: "Produce", categoryId: "cat-1" }];
    (client.GET as Mock).mockResolvedValue({ data: items, error: undefined });

    const { result } = renderHook(() => useSubcategoriesByCategoryId("cat-1"), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(items);
    expect(client.GET).toHaveBeenCalledWith(
      "/api/subcategories",
      { params: { query: { categoryId: "cat-1", offset: 0, limit: 200 } } },
    );
  });

  it("by-category query is disabled when categoryId is null", () => {
    const { result } = renderHook(() => useSubcategoriesByCategoryId(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.data).toBeUndefined();
    expect(result.current.fetchStatus).toBe("idle");
  });

  it("create mutation calls POST and shows toast on success", async () => {
    const newSub = { name: "Dairy", categoryId: "cat-1", description: "Milk products" };
    const created = { id: "2", ...newSub };
    (client.POST as Mock).mockResolvedValue({ data: created, error: undefined });

    const { result } = renderHook(() => useCreateSubcategory(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(newSub);

    expect(client.POST).toHaveBeenCalledWith("/api/subcategories", { body: newSub });
    expect(toast.success).toHaveBeenCalledWith("Subcategory created");
  });

  it("update mutation calls PUT and shows toast on success", async () => {
    const updated = { id: "1", name: "Organic Produce", categoryId: "cat-1" };
    (client.PUT as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useUpdateSubcategory(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(updated);

    expect(client.PUT).toHaveBeenCalledWith("/api/subcategories/{id}", {
      params: { path: { id: "1" } },
      body: updated,
    });
    expect(toast.success).toHaveBeenCalledWith("Subcategory updated");
  });

});

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
  useItemTemplates,
  useItemTemplate,
  useCreateItemTemplate,
  useUpdateItemTemplate,
  useDeleteItemTemplates,
  useDeletedItemTemplates,
  useRestoreItemTemplate,
} from "./useItemTemplates";

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

describe("useItemTemplates", () => {
  it("list query returns data on success", async () => {
    const templates = [
      {
        id: "1",
        name: "Grocery Item",
        description: "Default grocery template",
        defaultCategory: "Food",
        defaultSubcategory: "Produce",
        defaultUnitPrice: 1.0,
        defaultPricingMode: "quantity",
        defaultItemCode: "GRO",
      },
    ];
    (client.GET as Mock).mockResolvedValue({ data: { data: templates, total: 1, offset: 0, limit: 50 }, error: undefined });

    const { result } = renderHook(() => useItemTemplates(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(templates);
    expect(client.GET).toHaveBeenCalledWith("/api/item-templates", {
      params: { query: { offset: 0, limit: 50 } },
    });
  });

  it("single query is disabled when id is null", () => {
    const { result } = renderHook(() => useItemTemplate(null), {
      wrapper: createWrapper(),
    });

    expect(result.current.data).toBeUndefined();
    expect(result.current.fetchStatus).toBe("idle");
    expect(client.GET).not.toHaveBeenCalled();
  });

  it("single query fetches data when id is provided", async () => {
    const template = { id: "1", name: "Grocery Item", description: null };
    (client.GET as Mock).mockResolvedValue({ data: template, error: undefined });

    const { result } = renderHook(() => useItemTemplate("1"), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(template);
  });

  it("create mutation calls POST and shows toast on success", async () => {
    const newTemplate = {
      name: "Gas Template",
      description: "For fuel purchases",
      defaultCategory: "Transport",
      defaultSubcategory: null,
      defaultUnitPrice: null,
      defaultPricingMode: "flat",
      defaultItemCode: "GAS",
    };
    const created = { id: "2", ...newTemplate };
    (client.POST as Mock).mockResolvedValue({ data: created, error: undefined });

    const { result } = renderHook(() => useCreateItemTemplate(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(newTemplate);

    expect(client.POST).toHaveBeenCalledWith("/api/item-templates", {
      body: newTemplate,
    });
    expect(toast.success).toHaveBeenCalledWith("Item template created");
  });

  it("update mutation calls PUT and shows toast on success", async () => {
    const updated = {
      id: "1",
      name: "Grocery Item (v2)",
      description: "Updated",
      defaultCategory: "Food",
      defaultSubcategory: "Dairy",
      defaultUnitPrice: 2.0,
      defaultPricingMode: "quantity",
      defaultItemCode: "GRO",
    };
    (client.PUT as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useUpdateItemTemplate(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(updated);

    expect(client.PUT).toHaveBeenCalledWith("/api/item-templates/{id}", {
      params: { path: { id: "1" } },
      body: updated,
    });
    expect(toast.success).toHaveBeenCalledWith("Item template updated");
  });

  it("delete mutation calls DELETE", async () => {
    (client.DELETE as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useDeleteItemTemplates(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync(["1", "2"]);

    expect(client.DELETE).toHaveBeenCalledWith("/api/item-templates", {
      body: ["1", "2"],
    });
    expect(toast.success).toHaveBeenCalledWith("Item template(s) deleted");
  });

  it("deleted item templates query returns data on success", async () => {
    const deleted = [{ id: "3", name: "Old Template", description: null }];
    (client.GET as Mock).mockResolvedValue({ data: { data: deleted, total: 1, offset: 0, limit: 50 }, error: undefined });

    const { result } = renderHook(() => useDeletedItemTemplates(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(deleted);
    expect(client.GET).toHaveBeenCalledWith("/api/item-templates/deleted", {
      params: { query: { offset: 0, limit: 50 } },
    });
  });

  it("restore mutation calls POST and shows toast on success", async () => {
    (client.POST as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useRestoreItemTemplate(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync("1");

    expect(client.POST).toHaveBeenCalledWith("/api/item-templates/{id}/restore", {
      params: { path: { id: "1" } },
    });
    expect(toast.success).toHaveBeenCalledWith("Item template restored");
  });
});

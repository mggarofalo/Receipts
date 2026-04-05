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
  useYnabBudgets,
  useSelectedYnabBudget,
  useSelectYnabBudget,
  useYnabAccounts,
  useYnabAccountMappings,
  useCreateYnabAccountMapping,
  useUpdateYnabAccountMapping,
  useDeleteYnabAccountMapping,
  useYnabCategories,
  useDistinctReceiptItemCategories,
  useYnabCategoryMappings,
  useUnmappedCategories,
  useCreateYnabCategoryMapping,
  useUpdateYnabCategoryMapping,
  useDeleteYnabCategoryMapping,
} from "./useYnab";

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

describe("useYnab", () => {
  it("useYnabBudgets returns budgets on success", async () => {
    const budgets = [
      { id: "budget-1", name: "My Budget" },
      { id: "budget-2", name: "Other Budget" },
    ];
    (client.GET as Mock).mockResolvedValue({ data: { data: budgets }, error: undefined });

    const { result } = renderHook(() => useYnabBudgets(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.budgets).toEqual(budgets);
    expect(client.GET).toHaveBeenCalledWith("/api/ynab/budgets");
  });

  it("useYnabBudgets returns empty array when data is undefined", async () => {
    (client.GET as Mock).mockResolvedValue({ data: undefined, error: "Service unavailable" });

    const { result } = renderHook(() => useYnabBudgets(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.budgets).toEqual([]);
  });

  it("useSelectedYnabBudget returns selected budget id", async () => {
    const budgetId = "budget-123";
    (client.GET as Mock).mockResolvedValue({ data: { selectedBudgetId: budgetId }, error: undefined });

    const { result } = renderHook(() => useSelectedYnabBudget(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.selectedBudgetId).toBe(budgetId);
  });

  it("useSelectedYnabBudget returns null when no budget selected", async () => {
    (client.GET as Mock).mockResolvedValue({ data: { selectedBudgetId: null }, error: undefined });

    const { result } = renderHook(() => useSelectedYnabBudget(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.selectedBudgetId).toBeNull();
  });

  it("useSelectYnabBudget calls PUT and shows toast on success", async () => {
    (client.PUT as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useSelectYnabBudget(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync("budget-123");

    expect(client.PUT).toHaveBeenCalledWith("/api/ynab/settings/budget", {
      body: { budgetId: "budget-123" },
    });
    expect(toast.success).toHaveBeenCalledWith("YNAB budget selected");
  });

  it("useSelectYnabBudget shows error toast on failure", async () => {
    (client.PUT as Mock).mockResolvedValue({ error: "Failed" });

    const { result } = renderHook(() => useSelectYnabBudget(), {
      wrapper: createWrapper(),
    });

    await expect(result.current.mutateAsync("budget-123")).rejects.toThrow();

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith("Failed to select YNAB budget");
    });
  });

  it("useYnabAccounts returns accounts on success", async () => {
    const accounts = [
      { id: "acc-1", name: "Checking", type: "checking", onBudget: true, closed: false, balance: 100000 },
      { id: "acc-2", name: "Savings", type: "savings", onBudget: true, closed: false, balance: 50000 },
    ];
    (client.GET as Mock).mockResolvedValue({ data: { data: accounts }, error: undefined });

    const { result } = renderHook(() => useYnabAccounts(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.accounts).toEqual(accounts);
    expect(client.GET).toHaveBeenCalledWith("/api/ynab/accounts");
  });

  it("useYnabAccounts returns empty array on error", async () => {
    (client.GET as Mock).mockResolvedValue({ data: undefined, error: "Service unavailable" });

    const { result } = renderHook(() => useYnabAccounts(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.accounts).toEqual([]);
  });

  it("useYnabAccountMappings returns mappings on success", async () => {
    const mappings = [
      { id: "m1", receiptsAccountId: "a1", ynabAccountId: "y1", ynabAccountName: "Checking", ynabBudgetId: "b1" },
    ];
    (client.GET as Mock).mockResolvedValue({ data: { data: mappings }, error: undefined });

    const { result } = renderHook(() => useYnabAccountMappings(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.mappings).toEqual(mappings);
    expect(client.GET).toHaveBeenCalledWith("/api/ynab/account-mappings");
  });

  it("useCreateYnabAccountMapping calls POST and shows toast", async () => {
    (client.POST as Mock).mockResolvedValue({ data: { id: "new-id" }, error: undefined });

    const { result } = renderHook(() => useCreateYnabAccountMapping(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync({
      receiptsAccountId: "a1",
      ynabAccountId: "y1",
      ynabAccountName: "Checking",
      ynabBudgetId: "b1",
    });

    expect(client.POST).toHaveBeenCalledWith("/api/ynab/account-mappings", {
      body: {
        receiptsAccountId: "a1",
        ynabAccountId: "y1",
        ynabAccountName: "Checking",
        ynabBudgetId: "b1",
      },
    });
    expect(toast.success).toHaveBeenCalledWith("Account mapping created");
  });

  it("useUpdateYnabAccountMapping calls PUT and shows toast", async () => {
    (client.PUT as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useUpdateYnabAccountMapping(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync({
      id: "m1",
      ynabAccountId: "y2",
      ynabAccountName: "Savings",
      ynabBudgetId: "b1",
    });

    expect(client.PUT).toHaveBeenCalledWith("/api/ynab/account-mappings/{id}", {
      params: { path: { id: "m1" } },
      body: {
        ynabAccountId: "y2",
        ynabAccountName: "Savings",
        ynabBudgetId: "b1",
      },
    });
    expect(toast.success).toHaveBeenCalledWith("Account mapping updated");
  });

  it("useDeleteYnabAccountMapping calls DELETE and shows toast", async () => {
    (client.DELETE as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useDeleteYnabAccountMapping(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync("m1");

    expect(client.DELETE).toHaveBeenCalledWith("/api/ynab/account-mappings/{id}", {
      params: { path: { id: "m1" } },
    });
    expect(toast.success).toHaveBeenCalledWith("Account mapping removed");
  });

  it("useCreateYnabAccountMapping shows error toast on failure", async () => {
    (client.POST as Mock).mockResolvedValue({ error: "Failed" });

    const { result } = renderHook(() => useCreateYnabAccountMapping(), {
      wrapper: createWrapper(),
    });

    await expect(
      result.current.mutateAsync({
        receiptsAccountId: "a1",
        ynabAccountId: "y1",
        ynabAccountName: "Checking",
        ynabBudgetId: "b1",
      }),
    ).rejects.toThrow();

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith("Failed to create account mapping");
    });
  });

  it("useYnabCategories returns categories on success", async () => {
    const categories = [
      { id: "cat-1", name: "Groceries", categoryGroupId: "group-1", categoryGroupName: "Needs", hidden: false },
      { id: "cat-2", name: "Rent", categoryGroupId: "group-1", categoryGroupName: "Needs", hidden: false },
    ];
    (client.GET as Mock).mockResolvedValue({ data: { data: categories }, error: undefined });

    const { result } = renderHook(() => useYnabCategories(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.categories).toEqual(categories);
    expect(client.GET).toHaveBeenCalledWith("/api/ynab/categories");
  });

  it("useYnabCategories returns empty array on error", async () => {
    (client.GET as Mock).mockResolvedValue({ data: undefined, error: "Service unavailable" });

    const { result } = renderHook(() => useYnabCategories(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.categories).toEqual([]);
  });

  it("useDistinctReceiptItemCategories returns categories on success", async () => {
    const categories = ["Electronics", "Groceries", "Pharmacy"];
    (client.GET as Mock).mockResolvedValue({ data: { categories }, error: undefined });

    const { result } = renderHook(() => useDistinctReceiptItemCategories(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.categories).toEqual(categories);
    expect(client.GET).toHaveBeenCalledWith("/api/receipt-items/distinct-categories");
  });

  it("useYnabCategoryMappings returns mappings on success", async () => {
    const mappings = [
      { id: "m-1", receiptsCategory: "Groceries", ynabCategoryId: "cat-1", ynabCategoryName: "Groceries", ynabCategoryGroupName: "Needs", ynabBudgetId: "budget-1", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
    ];
    (client.GET as Mock).mockResolvedValue({ data: { data: mappings }, error: undefined });

    const { result } = renderHook(() => useYnabCategoryMappings(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.mappings).toEqual(mappings);
    expect(client.GET).toHaveBeenCalledWith("/api/ynab/category-mappings");
  });

  it("useUnmappedCategories returns unmapped list on success", async () => {
    const unmappedCategories = ["Electronics", "Pharmacy"];
    (client.GET as Mock).mockResolvedValue({ data: { unmappedCategories }, error: undefined });

    const { result } = renderHook(() => useUnmappedCategories(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.unmappedCategories).toEqual(unmappedCategories);
    expect(client.GET).toHaveBeenCalledWith("/api/ynab/category-mappings/unmapped");
  });

  it("useCreateYnabCategoryMapping calls POST and shows toast on success", async () => {
    (client.POST as Mock).mockResolvedValue({
      data: { id: "m-1", receiptsCategory: "Groceries" },
      error: undefined,
    });

    const { result } = renderHook(() => useCreateYnabCategoryMapping(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync({
      receiptsCategory: "Groceries",
      ynabCategoryId: "cat-1",
      ynabCategoryName: "Groceries",
      ynabCategoryGroupName: "Needs",
      ynabBudgetId: "budget-1",
    });

    expect(client.POST).toHaveBeenCalledWith("/api/ynab/category-mappings", {
      body: {
        receiptsCategory: "Groceries",
        ynabCategoryId: "cat-1",
        ynabCategoryName: "Groceries",
        ynabCategoryGroupName: "Needs",
        ynabBudgetId: "budget-1",
      },
    });
    expect(toast.success).toHaveBeenCalledWith("Category mapping created");
  });

  it("useUpdateYnabCategoryMapping calls PUT and shows toast on success", async () => {
    (client.PUT as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useUpdateYnabCategoryMapping(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync({
      id: "m-1",
      ynabCategoryId: "cat-2",
      ynabCategoryName: "Rent",
      ynabCategoryGroupName: "Needs",
      ynabBudgetId: "budget-1",
    });

    expect(client.PUT).toHaveBeenCalledWith("/api/ynab/category-mappings/{id}", {
      params: { path: { id: "m-1" } },
      body: {
        ynabCategoryId: "cat-2",
        ynabCategoryName: "Rent",
        ynabCategoryGroupName: "Needs",
        ynabBudgetId: "budget-1",
      },
    });
    expect(toast.success).toHaveBeenCalledWith("Category mapping updated");
  });

  it("useDeleteYnabCategoryMapping calls DELETE and shows toast on success", async () => {
    (client.DELETE as Mock).mockResolvedValue({ error: undefined });

    const { result } = renderHook(() => useDeleteYnabCategoryMapping(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync("m-1");

    expect(client.DELETE).toHaveBeenCalledWith("/api/ynab/category-mappings/{id}", {
      params: { path: { id: "m-1" } },
    });
    expect(toast.success).toHaveBeenCalledWith("Category mapping deleted");
  });

  it("useCreateYnabCategoryMapping shows error toast on failure", async () => {
    (client.POST as Mock).mockResolvedValue({ error: "Conflict" });

    const { result } = renderHook(() => useCreateYnabCategoryMapping(), {
      wrapper: createWrapper(),
    });

    await expect(
      result.current.mutateAsync({
        receiptsCategory: "Groceries",
        ynabCategoryId: "cat-1",
        ynabCategoryName: "Groceries",
        ynabCategoryGroupName: "Needs",
        ynabBudgetId: "budget-1",
      }),
    ).rejects.toThrow();

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith("Failed to create category mapping");
    });
  });
});

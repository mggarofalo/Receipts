import { useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import client from "@/lib/api-client";
import { toast } from "sonner";

export function useYnabBudgets() {
  const query = useQuery({
    queryKey: ["ynab", "budgets"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/ynab/budgets");
      if (error) throw error;
      return data;
    },
    retry: false,
  });
  return useMemo(
    () => ({ ...query, budgets: query.data?.data ?? [] }),
    [query],
  );
}

export function useSelectedYnabBudget() {
  const query = useQuery({
    queryKey: ["ynab", "settings", "budget"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/ynab/settings/budget");
      if (error) throw error;
      return data;
    },
  });
  return useMemo(
    () => ({
      ...query,
      selectedBudgetId: query.data?.selectedBudgetId ?? null,
    }),
    [query],
  );
}

export function useSelectYnabBudget() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (budgetId: string) => {
      const { error } = await client.PUT("/api/ynab/settings/budget", {
        body: { budgetId },
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["ynab"] });
      toast.success("YNAB budget selected");
    },
    onError: () => {
      toast.error("Failed to select YNAB budget");
    },
  });
}

export function useYnabAccounts() {
  const query = useQuery({
    queryKey: ["ynab", "accounts"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/ynab/accounts");
      if (error) throw error;
      return data;
    },
    retry: false,
  });
  return useMemo(
    () => ({ ...query, accounts: query.data?.data ?? [] }),
    [query],
  );
}

export function useYnabAccountMappings() {
  const query = useQuery({
    queryKey: ["ynab", "account-mappings"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/ynab/account-mappings");
      if (error) throw error;
      return data;
    },
  });
  return useMemo(
    () => ({ ...query, mappings: query.data?.data ?? [] }),
    [query],
  );
}

export function useCreateYnabAccountMapping() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      receiptsAccountId: string;
      ynabAccountId: string;
      ynabAccountName: string;
      ynabBudgetId: string;
    }) => {
      const { data, error } = await client.POST(
        "/api/ynab/account-mappings",
        { body },
      );
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["ynab", "account-mappings"] });
      toast.success("Account mapping created");
    },
    onError: () => {
      toast.error("Failed to create account mapping");
    },
  });
}

export function useUpdateYnabAccountMapping() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (params: {
      id: string;
      ynabAccountId: string;
      ynabAccountName: string;
      ynabBudgetId: string;
    }) => {
      const { error } = await client.PUT(
        "/api/ynab/account-mappings/{id}",
        {
          params: { path: { id: params.id } },
          body: {
            ynabAccountId: params.ynabAccountId,
            ynabAccountName: params.ynabAccountName,
            ynabBudgetId: params.ynabBudgetId,
          },
        },
      );
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["ynab", "account-mappings"] });
      toast.success("Account mapping updated");
    },
    onError: () => {
      toast.error("Failed to update account mapping");
    },
  });
}

export function useDeleteYnabAccountMapping() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await client.DELETE(
        "/api/ynab/account-mappings/{id}",
        {
          params: { path: { id } },
        },
      );
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["ynab", "account-mappings"] });
      toast.success("Account mapping removed");
    },
    onError: () => {
      toast.error("Failed to remove account mapping");
    },
  });
}

export function useYnabCategories() {
  const query = useQuery({
    queryKey: ["ynab", "categories"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/ynab/categories");
      if (error) throw error;
      return data;
    },
    retry: false,
  });
  return useMemo(
    () => ({ ...query, categories: query.data?.data ?? [] }),
    [query],
  );
}

export function useDistinctReceiptItemCategories() {
  const query = useQuery({
    queryKey: ["receipt-items", "distinct-categories"],
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/receipt-items/distinct-categories",
      );
      if (error) throw error;
      return data;
    },
  });
  return useMemo(
    () => ({ ...query, categories: query.data?.categories ?? [] }),
    [query],
  );
}

export function useYnabCategoryMappings() {
  const query = useQuery({
    queryKey: ["ynab", "category-mappings"],
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/ynab/category-mappings",
      );
      if (error) throw error;
      return data;
    },
  });
  return useMemo(
    () => ({ ...query, mappings: query.data?.data ?? [] }),
    [query],
  );
}

export function useUnmappedCategories() {
  const query = useQuery({
    queryKey: ["ynab", "category-mappings", "unmapped"],
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/ynab/category-mappings/unmapped",
      );
      if (error) throw error;
      return data;
    },
  });
  return useMemo(
    () => ({
      ...query,
      unmappedCategories: query.data?.unmappedCategories ?? [],
    }),
    [query],
  );
}

export function useCreateYnabCategoryMapping() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      receiptsCategory: string;
      ynabCategoryId: string;
      ynabCategoryName: string;
      ynabCategoryGroupName: string;
      ynabBudgetId: string;
    }) => {
      const { data, error } = await client.POST(
        "/api/ynab/category-mappings",
        { body },
      );
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["ynab", "category-mappings"] });
      toast.success("Category mapping created");
    },
    onError: () => {
      toast.error("Failed to create category mapping");
    },
  });
}

export function useUpdateYnabCategoryMapping() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({
      id,
      ...body
    }: {
      id: string;
      ynabCategoryId: string;
      ynabCategoryName: string;
      ynabCategoryGroupName: string;
      ynabBudgetId: string;
    }) => {
      const { error } = await client.PUT(
        "/api/ynab/category-mappings/{id}",
        { params: { path: { id } }, body },
      );
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["ynab", "category-mappings"] });
      toast.success("Category mapping updated");
    },
    onError: () => {
      toast.error("Failed to update category mapping");
    },
  });
}

export function useDeleteYnabCategoryMapping() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await client.DELETE(
        "/api/ynab/category-mappings/{id}",
        { params: { path: { id } } },
      );
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["ynab", "category-mappings"] });
      toast.success("Category mapping deleted");
    },
    onError: () => {
      toast.error("Failed to delete category mapping");
    },
  });
}

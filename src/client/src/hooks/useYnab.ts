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

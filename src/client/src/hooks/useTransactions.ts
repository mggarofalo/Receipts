import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import client from "@/lib/api-client";
import { toast } from "sonner";

export function useTransactions() {
  return useQuery({
    queryKey: ["transactions"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/transactions");
      if (error) throw error;
      return data;
    },
  });
}

export function useTransaction(id: string | null) {
  return useQuery({
    queryKey: ["transactions", id],
    enabled: !!id,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/transactions/{id}", {
        params: { path: { id: id! } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useTransactionsByReceiptId(receiptId: string | null) {
  return useQuery({
    queryKey: ["transactions", "by-receipt", receiptId],
    enabled: !!receiptId,
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/transactions/by-receipt-id/{receiptId}",
        { params: { path: { receiptId: receiptId! } } },
      );
      if (error) throw error;
      return data;
    },
  });
}

export function useCreateTransaction() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({
      receiptId,
      accountId,
      body,
    }: {
      receiptId: string;
      accountId: string;
      body: { amount: number; date: string };
    }) => {
      const { data, error } = await client.POST(
        "/api/transactions/{receiptId}/{accountId}",
        { params: { path: { receiptId, accountId } }, body },
      );
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
      toast.success("Transaction created");
    },
    onError: () => {
      toast.error("Failed to create transaction");
    },
  });
}

export function useUpdateTransaction() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({
      receiptId,
      accountId,
      body,
    }: {
      receiptId: string;
      accountId: string;
      body: { id: string; amount: number; date: string };
    }) => {
      const { error } = await client.PUT(
        "/api/transactions/{receiptId}/{accountId}",
        { params: { path: { receiptId, accountId } }, body },
      );
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
      toast.success("Transaction updated");
    },
    onError: () => {
      toast.error("Failed to update transaction");
    },
  });
}

export function useDeleteTransactions() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (ids: string[]) => {
      const { error } = await client.DELETE("/api/transactions", {
        body: ids,
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
      toast.success("Transaction(s) deleted");
    },
    onError: () => {
      toast.error("Failed to delete transaction(s)");
    },
  });
}

export function useDeletedTransactions() {
  return useQuery({
    queryKey: ["transactions", "deleted"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/transactions/deleted");
      if (error) throw error;
      return data;
    },
  });
}

export function useRestoreTransaction() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await client.POST("/api/transactions/{id}/restore", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["transactions"] });
      toast.success("Transaction restored");
    },
    onError: () => {
      toast.error("Failed to restore transaction");
    },
  });
}

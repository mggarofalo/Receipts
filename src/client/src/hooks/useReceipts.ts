import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import client from "@/lib/api-client";
import { toast } from "sonner";

export function useReceipts() {
  return useQuery({
    queryKey: ["receipts"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/receipts");
      if (error) throw error;
      return data;
    },
  });
}

export function useReceipt(id: string | null) {
  return useQuery({
    queryKey: ["receipts", id],
    enabled: !!id,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/receipts/{id}", {
        params: { path: { id: id! } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useCreateReceipt() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      description?: string | null;
      location: string;
      date: string;
      taxAmount: number;
    }) => {
      const { data, error } = await client.POST("/api/receipts", { body });
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["receipts"] });
      toast.success("Receipt created");
    },
    onError: () => {
      toast.error("Failed to create receipt");
    },
  });
}

export function useUpdateReceipt() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      id: string;
      description?: string | null;
      location: string;
      date: string;
      taxAmount: number;
    }) => {
      const { error } = await client.PUT("/api/receipts/{id}", {
        params: { path: { id: body.id } },
        body,
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["receipts"] });
      toast.success("Receipt updated");
    },
    onError: () => {
      toast.error("Failed to update receipt");
    },
  });
}

export function useDeleteReceipts() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (ids: string[]) => {
      const { error } = await client.DELETE("/api/receipts", { body: ids });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["receipts"] });
      toast.success("Receipt(s) deleted");
    },
    onError: () => {
      toast.error("Failed to delete receipt(s)");
    },
  });
}

export function useDeletedReceipts() {
  return useQuery({
    queryKey: ["receipts", "deleted"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/receipts/deleted");
      if (error) throw error;
      return data;
    },
  });
}

export function useRestoreReceipt() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await client.POST("/api/receipts/{id}/restore", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["receipts"] });
      toast.success("Receipt restored");
    },
    onError: () => {
      toast.error("Failed to restore receipt");
    },
  });
}

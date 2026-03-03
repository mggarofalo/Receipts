import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import client from "@/lib/api-client";
import { toast } from "sonner";

export function useAdjustments() {
  return useQuery({
    queryKey: ["adjustments"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/adjustments");
      if (error) throw error;
      return data;
    },
  });
}

export function useAdjustment(id: string | null) {
  return useQuery({
    queryKey: ["adjustments", id],
    enabled: !!id,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/adjustments/{id}", {
        params: { path: { id: id! } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useAdjustmentsByReceiptId(receiptId: string | null) {
  return useQuery({
    queryKey: ["adjustments", "by-receipt", receiptId],
    enabled: !!receiptId,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/adjustments", {
        params: { query: { receiptId: receiptId! } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useCreateAdjustment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({
      receiptId,
      body,
    }: {
      receiptId: string;
      body: {
        type: string;
        amount: number;
        description?: string | null;
      };
    }) => {
      const { data, error } = await client.POST(
        "/api/receipts/{receiptId}/adjustments",
        { params: { path: { receiptId } }, body },
      );
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["adjustments"] });
      queryClient.invalidateQueries({ queryKey: ["receipts-with-items"] });
      queryClient.invalidateQueries({ queryKey: ["trips"] });
      toast.success("Adjustment created");
    },
    onError: () => {
      toast.error("Failed to create adjustment");
    },
  });
}

export function useUpdateAdjustment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({
      body,
    }: {
      body: {
        id: string;
        type: string;
        amount: number;
        description?: string | null;
      };
    }) => {
      const { error } = await client.PUT("/api/adjustments/{id}", {
        params: { path: { id: body.id } },
        body,
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["adjustments"] });
      queryClient.invalidateQueries({ queryKey: ["receipts-with-items"] });
      queryClient.invalidateQueries({ queryKey: ["trips"] });
      toast.success("Adjustment updated");
    },
    onError: () => {
      toast.error("Failed to update adjustment");
    },
  });
}

export function useDeleteAdjustments() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (ids: string[]) => {
      const { error } = await client.DELETE("/api/adjustments", {
        body: ids,
      });
      if (error) throw error;
    },
    onMutate: async (ids) => {
      await queryClient.cancelQueries({ queryKey: ["adjustments"] });
      const previous = queryClient.getQueryData(["adjustments"]);
      queryClient.setQueryData(
        ["adjustments"],
        (old: { id: string }[] | undefined) =>
          old?.filter((item) => !ids.includes(item.id)),
      );
      return { previous };
    },
    onError: (_err, _ids, context) => {
      if (context?.previous) {
        queryClient.setQueryData(["adjustments"], context.previous);
      }
      toast.error("Failed to delete adjustment(s)");
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ["adjustments"] });
      queryClient.invalidateQueries({ queryKey: ["adjustments", "deleted"] });
      queryClient.invalidateQueries({ queryKey: ["receipts-with-items"] });
      queryClient.invalidateQueries({ queryKey: ["trips"] });
    },
    onSuccess: () => {
      toast.success("Adjustment(s) deleted");
    },
  });
}

export function useDeletedAdjustments() {
  return useQuery({
    queryKey: ["adjustments", "deleted"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/adjustments/deleted");
      if (error) throw error;
      return data;
    },
  });
}

export function useRestoreAdjustment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await client.POST("/api/adjustments/{id}/restore", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["adjustments"] });
      queryClient.invalidateQueries({ queryKey: ["adjustments", "deleted"] });
      queryClient.invalidateQueries({ queryKey: ["receipts-with-items"] });
      queryClient.invalidateQueries({ queryKey: ["trips"] });
      toast.success("Adjustment restored");
    },
    onError: () => {
      toast.error("Failed to restore adjustment");
    },
  });
}

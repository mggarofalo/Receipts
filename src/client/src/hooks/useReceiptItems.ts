import {
  useQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import client from "@/lib/api-client";
import { toast } from "sonner";

export function useReceiptItems() {
  return useQuery({
    queryKey: ["receipt-items"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/receipt-items");
      if (error) throw error;
      return data;
    },
  });
}

export function useReceiptItem(id: string | null) {
  return useQuery({
    queryKey: ["receipt-items", id],
    enabled: !!id,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/receipt-items/{id}", {
        params: { path: { id: id! } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useReceiptItemsByReceiptId(receiptId: string | null) {
  return useQuery({
    queryKey: ["receipt-items", "by-receipt", receiptId],
    enabled: !!receiptId,
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/receipt-items/by-receipt-id/{receiptId}",
        { params: { path: { receiptId: receiptId! } } },
      );
      if (error) throw error;
      return data;
    },
  });
}

export function useCreateReceiptItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({
      receiptId,
      body,
    }: {
      receiptId: string;
      body: {
        receiptItemCode: string;
        description: string;
        quantity: number;
        unitPrice: number;
        category: string;
        subcategory: string;
      };
    }) => {
      const { data, error } = await client.POST("/api/receipt-items/{id}", {
        params: { path: { id: receiptId } },
        body,
      });
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["receipt-items"] });
      toast.success("Receipt item created");
    },
    onError: () => {
      toast.error("Failed to create receipt item");
    },
  });
}

export function useUpdateReceiptItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({
      receiptId,
      body,
    }: {
      receiptId: string;
      body: {
        id: string;
        receiptItemCode: string;
        description: string;
        quantity: number;
        unitPrice: number;
        category: string;
        subcategory: string;
      };
    }) => {
      const { error } = await client.PUT("/api/receipt-items/{id}", {
        params: { path: { id: receiptId } },
        body,
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["receipt-items"] });
      toast.success("Receipt item updated");
    },
    onError: () => {
      toast.error("Failed to update receipt item");
    },
  });
}

export function useDeleteReceiptItems() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (ids: string[]) => {
      const { error } = await client.DELETE("/api/receipt-items", {
        body: ids,
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["receipt-items"] });
      toast.success("Receipt item(s) deleted");
    },
    onError: () => {
      toast.error("Failed to delete receipt item(s)");
    },
  });
}

export function useRestoreReceiptItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await client.POST(
        "/api/receipt-items/{id}/restore",
        { params: { path: { id } } },
      );
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["receipt-items"] });
      toast.success("Receipt item restored");
    },
    onError: () => {
      toast.error("Failed to restore receipt item");
    },
  });
}

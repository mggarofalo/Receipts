import { useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import client from "@/lib/api-client";
import { toast } from "sonner";

// Note: Cards are hard-delete entities (no soft-delete/restore).

export function useCards(offset = 0, limit = 50, sortBy?: string | null, sortDirection?: string | null, isActive?: boolean | null) {
  const query = useQuery({
    queryKey: ["cards", "list", offset, limit, sortBy, sortDirection, isActive],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/cards", {
        params: { query: { offset, limit, sortBy: sortBy ?? undefined, sortDirection: (sortDirection ?? undefined) as "asc" | "desc" | undefined, isActive: isActive ?? undefined } },
      });
      if (error) throw error;
      return data;
    },
  });
  return useMemo(() => ({ ...query, data: query.data?.data, total: Number(query.data?.total ?? 0) }), [query]);
}

export function useCard(id: string | null) {
  return useQuery({
    queryKey: ["cards", id],
    enabled: !!id,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/cards/{id}", {
        params: { path: { id: id! } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useCreateCard() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      cardCode: string;
      name: string;
      isActive: boolean;
    }) => {
      const { data, error } = await client.POST("/api/cards", { body });
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["cards"] });
      toast.success("Card created");
    },
    onError: () => {
      toast.error("Failed to create card");
    },
  });
}

export function useUpdateCard() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      id: string;
      cardCode: string;
      name: string;
      isActive: boolean;
    }) => {
      const { error } = await client.PUT("/api/cards/{id}", {
        params: { path: { id: body.id } },
        body,
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["cards"] });
      toast.success("Card updated");
    },
    onError: () => {
      toast.error("Failed to update card");
    },
  });
}

export interface DeleteCardConflict {
  message: string;
  transactionCount: number;
}

export function useDeleteCard() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error, response } = await client.DELETE("/api/cards/{id}", {
        params: { path: { id } },
      });
      if (error) {
        if (response.status === 409) {
          const body = error as unknown as DeleteCardConflict;
          throw { conflict: true, ...body };
        }
        throw error;
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["cards"] });
      toast.success("Card deleted");
    },
    onError: (error: unknown) => {
      const err = error as { conflict?: boolean; message?: string; transactionCount?: number };
      if (err.conflict) {
        toast.error(err.message ?? "Cannot delete — transactions reference this card");
      } else {
        toast.error("Failed to delete card");
      }
    },
  });
}

import { useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import client from "@/lib/api-client";
import { toast } from "sonner";

export function useCategories(offset = 0, limit = 50, sortBy?: string | null, sortDirection?: string | null, isActive?: boolean | null) {
  const query = useQuery({
    queryKey: ["categories", "list", offset, limit, sortBy, sortDirection, isActive],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/categories", {
        params: { query: { offset, limit, sortBy: sortBy ?? undefined, sortDirection: (sortDirection ?? undefined) as "asc" | "desc" | undefined, isActive: isActive ?? undefined } },
      });
      if (error) throw error;
      return data;
    },
  });
  return useMemo(() => ({ ...query, data: query.data?.data, total: Number(query.data?.total ?? 0) }), [query]);
}

export function useCategory(id: string | null) {
  return useQuery({
    queryKey: ["categories", id],
    enabled: !!id,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/categories/{id}", {
        params: { path: { id: id! } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useCreateCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      name: string;
      description?: string | null;
      isActive: boolean;
    }) => {
      const { data, error } = await client.POST("/api/categories", { body });
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      toast.success("Category created");
    },
    onError: (err) => {
      toast.error(typeof err === "string" ? err : "Failed to create category");
    },
  });
}

export function useUpdateCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      id: string;
      name: string;
      description?: string | null;
      isActive: boolean;
    }) => {
      const { error } = await client.PUT("/api/categories/{id}", {
        params: { path: { id: body.id } },
        body,
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      toast.success("Category updated");
    },
    onError: () => {
      toast.error("Failed to update category");
    },
  });
}

export interface DeleteCategoryConflict {
  message: string;
  receiptItemCount?: number;
}

export function useDeleteCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error, response } = await client.DELETE("/api/categories/{id}", {
        params: { path: { id } },
      });
      if (error) {
        if (response.status === 409) {
          const body = error as unknown as DeleteCategoryConflict;
          throw { conflict: true, ...body };
        }
        throw error;
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      queryClient.invalidateQueries({ queryKey: ["categories", "deleted"] });
      toast.success("Category deleted");
    },
    onError: (error: unknown) => {
      const err = error as { conflict?: boolean; message?: string };
      if (err.conflict) {
        toast.error(err.message ?? "Cannot delete — dependencies reference this category");
      } else {
        toast.error("Failed to delete category");
      }
    },
  });
}

export function useDeletedCategories(offset = 0, limit = 50, sortBy?: string | null, sortDirection?: string | null) {
  const query = useQuery({
    queryKey: ["categories", "deleted", offset, limit, sortBy, sortDirection],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/categories/deleted", {
        params: { query: { offset, limit, sortBy: sortBy ?? undefined, sortDirection: (sortDirection ?? undefined) as "asc" | "desc" | undefined } },
      });
      if (error) throw error;
      return data;
    },
  });
  return useMemo(() => ({ ...query, data: query.data?.data, total: Number(query.data?.total ?? 0) }), [query]);
}

export function useRestoreCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await client.POST("/api/categories/{id}/restore", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      queryClient.invalidateQueries({ queryKey: ["categories", "deleted"] });
      toast.success("Category restored");
    },
    onError: () => {
      toast.error("Failed to restore category");
    },
  });
}

import { useMemo } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import client from "@/lib/api-client";
import { toast } from "sonner";

export function useSubcategories(offset = 0, limit = 50, sortBy?: string | null, sortDirection?: string | null) {
  const query = useQuery({
    queryKey: ["subcategories", "list", offset, limit, sortBy, sortDirection],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/subcategories", {
        params: { query: { offset, limit, sortBy: sortBy ?? undefined, sortDirection: (sortDirection ?? undefined) as "asc" | "desc" | undefined } },
      });
      if (error) throw error;
      return data;
    },
  });
  return useMemo(() => ({ ...query, data: query.data?.data, total: query.data?.total ?? 0 }), [query]);
}

export function useSubcategory(id: string | null) {
  return useQuery({
    queryKey: ["subcategories", id],
    enabled: !!id,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/subcategories/{id}", {
        params: { path: { id: id! } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useSubcategoriesByCategoryId(categoryId: string | null, offset = 0, limit = 200, sortBy?: string | null, sortDirection?: string | null) {
  const query = useQuery({
    queryKey: ["subcategories", "byCategory", categoryId, offset, limit, sortBy, sortDirection],
    enabled: !!categoryId,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/subcategories", {
        params: { query: { categoryId: categoryId!, offset, limit, sortBy: sortBy ?? undefined, sortDirection: (sortDirection ?? undefined) as "asc" | "desc" | undefined } },
      });
      if (error) throw error;
      return data;
    },
  });
  return useMemo(() => ({ ...query, data: query.data?.data, total: query.data?.total ?? 0 }), [query]);
}

export function useCreateSubcategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      name: string;
      categoryId: string;
      description?: string | null;
      isActive: boolean;
    }) => {
      const { data, error } = await client.POST("/api/subcategories", {
        body,
      });
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["subcategories"] });
      toast.success("Subcategory created");
    },
    onError: (err) => {
      toast.error(
        typeof err === "string" ? err : "Failed to create subcategory",
      );
    },
  });
}

export function useUpdateSubcategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      id: string;
      name: string;
      categoryId: string;
      description?: string | null;
      isActive: boolean;
    }) => {
      const { error } = await client.PUT("/api/subcategories/{id}", {
        params: { path: { id: body.id } },
        body,
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["subcategories"] });
      toast.success("Subcategory updated");
    },
    onError: () => {
      toast.error("Failed to update subcategory");
    },
  });
}


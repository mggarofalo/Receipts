import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import client from "@/lib/api-client";
import { toast } from "sonner";

export function useSubcategories(offset = 0, limit = 50, sortBy?: string | null, sortDirection?: string | null) {
  return useQuery({
    queryKey: ["subcategories", "list", offset, limit, sortBy, sortDirection],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/subcategories", {
        params: { query: { offset, limit, sortBy: sortBy ?? undefined, sortDirection: (sortDirection ?? undefined) as "asc" | "desc" | undefined } },
      });
      if (error) throw error;
      return data;
    },
  });
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
  return useQuery({
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
}

export function useCreateSubcategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      name: string;
      categoryId: string;
      description?: string | null;
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

export function useDeleteSubcategories() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (ids: string[]) => {
      const { error } = await client.DELETE("/api/subcategories", {
        body: ids,
      });
      if (error) throw error;
    },
    onMutate: async (ids) => {
      await queryClient.cancelQueries({ queryKey: ["subcategories"] });
      const previous = queryClient.getQueriesData<{ data: { id: string }[]; total: number }>({ queryKey: ["subcategories", "list"] });
      for (const [key] of previous) {
        queryClient.setQueryData(key, (old: { data: { id: string }[]; total: number; offset: number; limit: number } | undefined) => {
          if (!old?.data) return old;
          const filtered = old.data.filter((item) => !ids.includes(item.id));
          return { ...old, data: filtered, total: old.total - (old.data.length - filtered.length) };
        });
      }
      return { previous };
    },
    onError: (_err, _ids, context) => {
      for (const [key, data] of context?.previous ?? []) {
        queryClient.setQueryData(key, data);
      }
      toast.error("Failed to delete subcategory(ies)");
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ["subcategories"] });
      queryClient.invalidateQueries({
        queryKey: ["subcategories", "deleted"],
      });
    },
    onSuccess: () => {
      toast.success("Subcategory(ies) deleted");
    },
  });
}

export function useDeletedSubcategories(offset = 0, limit = 50, sortBy?: string | null, sortDirection?: string | null) {
  return useQuery({
    queryKey: ["subcategories", "deleted", offset, limit, sortBy, sortDirection],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/subcategories/deleted", {
        params: { query: { offset, limit, sortBy: sortBy ?? undefined, sortDirection: (sortDirection ?? undefined) as "asc" | "desc" | undefined } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useRestoreSubcategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await client.POST("/api/subcategories/{id}/restore", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["subcategories"] });
      queryClient.invalidateQueries({
        queryKey: ["subcategories", "deleted"],
      });
      toast.success("Subcategory restored");
    },
    onError: () => {
      toast.error("Failed to restore subcategory");
    },
  });
}

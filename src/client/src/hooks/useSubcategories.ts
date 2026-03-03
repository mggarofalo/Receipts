import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import client from "@/lib/api-client";
import { toast } from "sonner";

export function useSubcategories() {
  return useQuery({
    queryKey: ["subcategories"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/subcategories");
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

export function useSubcategoriesByCategoryId(categoryId: string | null) {
  return useQuery({
    queryKey: ["subcategories", "byCategory", categoryId],
    enabled: !!categoryId,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/subcategories", {
        params: { query: { categoryId: categoryId! } },
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
      const previous = queryClient.getQueryData(["subcategories"]);
      queryClient.setQueryData(
        ["subcategories"],
        (old: { id: string }[] | undefined) =>
          old?.filter((item) => !ids.includes(item.id)),
      );
      return { previous };
    },
    onError: (_err, _ids, context) => {
      if (context?.previous) {
        queryClient.setQueryData(["subcategories"], context.previous);
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

export function useDeletedSubcategories() {
  return useQuery({
    queryKey: ["subcategories", "deleted"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/subcategories/deleted");
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

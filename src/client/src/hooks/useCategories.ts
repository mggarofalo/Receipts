import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import client from "@/lib/api-client";
import { toast } from "sonner";

export function useCategories() {
  return useQuery({
    queryKey: ["categories"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/categories");
      if (error) throw error;
      return data;
    },
  });
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
    mutationFn: async (body: { name: string; description?: string | null }) => {
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

export function useDeleteCategories() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (ids: string[]) => {
      const { error } = await client.DELETE("/api/categories", { body: ids });
      if (error) throw error;
    },
    onMutate: async (ids) => {
      await queryClient.cancelQueries({ queryKey: ["categories"] });
      const previous = queryClient.getQueryData(["categories"]);
      queryClient.setQueryData(
        ["categories"],
        (old: { id: string }[] | undefined) =>
          old?.filter((item) => !ids.includes(item.id)),
      );
      return { previous };
    },
    onError: (_err, _ids, context) => {
      if (context?.previous) {
        queryClient.setQueryData(["categories"], context.previous);
      }
      toast.error("Failed to delete category(ies)");
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      queryClient.invalidateQueries({ queryKey: ["categories", "deleted"] });
    },
    onSuccess: () => {
      toast.success("Category(ies) deleted");
    },
  });
}

export function useDeletedCategories() {
  return useQuery({
    queryKey: ["categories", "deleted"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/categories/deleted");
      if (error) throw error;
      return data;
    },
  });
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

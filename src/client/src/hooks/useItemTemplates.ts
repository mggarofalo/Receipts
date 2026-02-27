import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import client from "@/lib/api-client";
import { toast } from "sonner";

export function useItemTemplates() {
  return useQuery({
    queryKey: ["itemTemplates"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/item-templates");
      if (error) throw error;
      return data;
    },
  });
}

export function useItemTemplate(id: string | null) {
  return useQuery({
    queryKey: ["itemTemplates", id],
    enabled: !!id,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/item-templates/{id}", {
        params: { path: { id: id! } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useCreateItemTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      name: string;
      description?: string | null;
      defaultCategory?: string | null;
      defaultSubcategory?: string | null;
      defaultUnitPrice?: number | null;
      defaultPricingMode?: string | null;
      defaultItemCode?: string | null;
    }) => {
      const { data, error } = await client.POST("/api/item-templates", {
        body,
      });
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["itemTemplates"] });
      toast.success("Item template created");
    },
    onError: () => {
      toast.error("Failed to create item template");
    },
  });
}

export function useUpdateItemTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      id: string;
      name: string;
      description?: string | null;
      defaultCategory?: string | null;
      defaultSubcategory?: string | null;
      defaultUnitPrice?: number | null;
      defaultPricingMode?: string | null;
      defaultItemCode?: string | null;
    }) => {
      const { error } = await client.PUT("/api/item-templates/{id}", {
        params: { path: { id: body.id } },
        body,
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["itemTemplates"] });
      toast.success("Item template updated");
    },
    onError: () => {
      toast.error("Failed to update item template");
    },
  });
}

export function useDeleteItemTemplates() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (ids: string[]) => {
      const { error } = await client.DELETE("/api/item-templates", {
        body: ids,
      });
      if (error) throw error;
    },
    onMutate: async (ids) => {
      await queryClient.cancelQueries({ queryKey: ["itemTemplates"] });
      const previous = queryClient.getQueryData(["itemTemplates"]);
      queryClient.setQueryData(
        ["itemTemplates"],
        (old: { id: string }[] | undefined) =>
          old?.filter((item) => !ids.includes(item.id)),
      );
      return { previous };
    },
    onError: (_err, _ids, context) => {
      if (context?.previous) {
        queryClient.setQueryData(["itemTemplates"], context.previous);
      }
      toast.error("Failed to delete item template(s)");
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ["itemTemplates"] });
      queryClient.invalidateQueries({
        queryKey: ["itemTemplates", "deleted"],
      });
    },
    onSuccess: () => {
      toast.success("Item template(s) deleted");
    },
  });
}

export function useDeletedItemTemplates() {
  return useQuery({
    queryKey: ["itemTemplates", "deleted"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/item-templates/deleted");
      if (error) throw error;
      return data;
    },
  });
}

export function useRestoreItemTemplate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await client.POST("/api/item-templates/{id}/restore", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["itemTemplates"] });
      queryClient.invalidateQueries({
        queryKey: ["itemTemplates", "deleted"],
      });
      toast.success("Item template restored");
    },
    onError: () => {
      toast.error("Failed to restore item template");
    },
  });
}

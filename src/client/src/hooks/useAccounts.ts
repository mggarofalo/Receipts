import {
  useQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import client from "@/lib/api-client";
import { toast } from "sonner";

export function useAccounts() {
  return useQuery({
    queryKey: ["accounts"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/accounts");
      if (error) throw error;
      return data;
    },
  });
}

export function useAccount(id: string | null) {
  return useQuery({
    queryKey: ["accounts", id],
    enabled: !!id,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/accounts/{id}", {
        params: { path: { id: id! } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useCreateAccount() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      accountCode: string;
      name: string;
      isActive: boolean;
    }) => {
      const { data, error } = await client.POST("/api/accounts", { body });
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["accounts"] });
      toast.success("Account created");
    },
    onError: () => {
      toast.error("Failed to create account");
    },
  });
}

export function useUpdateAccount() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      id: string;
      accountCode: string;
      name: string;
      isActive: boolean;
    }) => {
      const { error } = await client.PUT("/api/accounts/{id}", {
        params: { path: { id: body.id } },
        body,
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["accounts"] });
      toast.success("Account updated");
    },
    onError: () => {
      toast.error("Failed to update account");
    },
  });
}

export function useDeleteAccounts() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (ids: string[]) => {
      const { error } = await client.DELETE("/api/accounts", { body: ids });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["accounts"] });
      toast.success("Account(s) deleted");
    },
    onError: () => {
      toast.error("Failed to delete account(s)");
    },
  });
}

export function useRestoreAccount() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await client.POST("/api/accounts/{id}/restore", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["accounts"] });
      toast.success("Account restored");
    },
    onError: () => {
      toast.error("Failed to restore account");
    },
  });
}

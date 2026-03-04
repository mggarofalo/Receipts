import {
  useQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import client from "@/lib/api-client";
import { toast } from "sonner";

export function useUsers(offset = 0, limit = 50) {
  return useQuery({
    queryKey: ["users", "list", offset, limit],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/users", {
        params: { query: { offset, limit } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useUser(userId: string | null) {
  return useQuery({
    queryKey: ["users", userId],
    enabled: !!userId,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/users/{userId}", {
        params: { path: { userId: userId! } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useCreateUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: {
      email: string;
      password: string;
      role: string;
      firstName?: string | null;
      lastName?: string | null;
    }) => {
      const { data, error } = await client.POST("/api/users", { body });
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      toast.success("User created");
    },
    onError: () => {
      toast.error("Failed to create user");
    },
  });
}

export function useUpdateUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({
      userId,
      body,
    }: {
      userId: string;
      body: {
        email: string;
        role: string;
        isDisabled: boolean;
        firstName?: string | null;
        lastName?: string | null;
      };
    }) => {
      const { error } = await client.PUT("/api/users/{userId}", {
        params: { path: { userId } },
        body,
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      toast.success("User updated");
    },
    onError: () => {
      toast.error("Failed to update user");
    },
  });
}

export function useDeleteUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (userId: string) => {
      const { error } = await client.DELETE("/api/users/{userId}", {
        params: { path: { userId } },
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      toast.success("User deactivated");
    },
    onError: () => {
      toast.error("Failed to deactivate user");
    },
  });
}

export function useResetUserPassword() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({
      userId,
      newPassword,
    }: {
      userId: string;
      newPassword: string;
    }) => {
      const { error } = await client.POST(
        "/api/users/{userId}/reset-password",
        {
          params: { path: { userId } },
          body: { newPassword },
        },
      );
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      toast.success("Password reset successfully");
    },
    onError: () => {
      toast.error("Failed to reset password");
    },
  });
}

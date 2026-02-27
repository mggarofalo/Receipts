import { useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";

export function useMyAuthAuditLog(count = 50) {
  return useQuery({
    queryKey: ["auth-audit", "me", count],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/auth/audit/me", {
        params: { query: { count } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useRecentAuthAuditLogs(count = 50) {
  return useQuery({
    queryKey: ["auth-audit", "recent", count],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/auth/audit/recent", {
        params: { query: { count } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useFailedAuthAttempts(count = 50) {
  return useQuery({
    queryKey: ["auth-audit", "failed", count],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/auth/audit/failed", {
        params: { query: { count } },
      });
      if (error) throw error;
      return data;
    },
  });
}

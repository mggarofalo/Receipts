import { useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";

export function useEntityAuditHistory(
  entityType: string | null,
  entityId: string | null,
) {
  return useQuery({
    queryKey: ["audit", "entity", entityType, entityId],
    enabled: !!entityType && !!entityId,
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/audit/entity/{entityType}/{entityId}",
        { params: { path: { entityType: entityType!, entityId: entityId! } } },
      );
      if (error) throw error;
      return data;
    },
  });
}

export function useRecentAuditLogs(count = 50) {
  return useQuery({
    queryKey: ["audit", "recent", count],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/audit/recent", {
        params: { query: { count } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useAuditLogsByUser(userId: string | null) {
  return useQuery({
    queryKey: ["audit", "user", userId],
    enabled: !!userId,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/audit/user/{userId}", {
        params: { path: { userId: userId! } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useAuditLogsByApiKey(apiKeyId: string | null) {
  return useQuery({
    queryKey: ["audit", "apikey", apiKeyId],
    enabled: !!apiKeyId,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/audit/apikey/{apiKeyId}", {
        params: { path: { apiKeyId: apiKeyId! } },
      });
      if (error) throw error;
      return data;
    },
  });
}

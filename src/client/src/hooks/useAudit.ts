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
      const { data, error } = await client.GET("/api/audit", {
        params: { query: { entityType: entityType!, entityId: entityId! } },
      });
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

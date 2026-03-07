import { useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";

export function useEntityAuditHistory(
  entityType: string | null,
  entityId: string | null,
  offset = 0,
  limit = 50,
  sortBy?: string | null,
  sortDirection?: string | null,
) {
  return useQuery({
    queryKey: ["audit", "entity", entityType, entityId, offset, limit, sortBy, sortDirection],
    enabled: !!entityType && !!entityId,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/audit", {
        params: {
          query: {
            entityType: entityType!,
            entityId: entityId!,
            offset,
            limit,
            sortBy: sortBy ?? undefined,
            sortDirection: (sortDirection ?? undefined) as "asc" | "desc" | undefined,
          },
        },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useRecentAuditLogs(
  offset = 0,
  limit = 50,
  sortBy?: string | null,
  sortDirection?: string | null,
) {
  return useQuery({
    queryKey: ["audit", "recent", offset, limit, sortBy, sortDirection],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/audit/recent", {
        params: {
          query: {
            offset,
            limit,
            sortBy: sortBy ?? undefined,
            sortDirection: (sortDirection ?? undefined) as "asc" | "desc" | undefined,
          },
        },
      });
      if (error) throw error;
      return data;
    },
  });
}

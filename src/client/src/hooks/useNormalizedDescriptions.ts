import { keepPreviousData, useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";

export type NormalizedDescriptionStatusFilter = "Active" | "PendingReview";

export function useNormalizedDescriptions(
  statusFilter?: NormalizedDescriptionStatusFilter,
) {
  return useQuery({
    queryKey: ["normalized-descriptions", "list", statusFilter],
    placeholderData: keepPreviousData,
    queryFn: async () => {
      const { data, error } = await client.GET("/api/normalized-descriptions", {
        params: { query: { status: statusFilter } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useNormalizedDescription(id: string | null) {
  return useQuery({
    queryKey: ["normalized-descriptions", id],
    enabled: !!id,
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/normalized-descriptions/{id}",
        { params: { path: { id: id! } } },
      );
      if (error) throw error;
      return data;
    },
  });
}

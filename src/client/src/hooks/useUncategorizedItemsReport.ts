import { keepPreviousData, useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";

export interface UncategorizedItemsParams {
  sortBy?: "description" | "total" | "itemCode";
  sortDirection?: "asc" | "desc";
  page?: number;
  pageSize?: number;
}

export function useUncategorizedItemsReport(
  params: UncategorizedItemsParams = {},
) {
  return useQuery({
    queryKey: [
      "reports",
      "uncategorized-items",
      params.sortBy,
      params.sortDirection,
      params.page,
      params.pageSize,
    ],
    placeholderData: keepPreviousData,
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/reports/uncategorized-items",
        {
          params: {
            query: {
              sortBy: params.sortBy,
              sortDirection: params.sortDirection,
              page: params.page,
              pageSize: params.pageSize,
            },
          },
        },
      );
      if (error) throw error;
      return data;
    },
  });
}

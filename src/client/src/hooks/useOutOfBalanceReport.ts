import { keepPreviousData, useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";

export interface OutOfBalanceParams {
  sortBy?: "date" | "difference";
  sortDirection?: "asc" | "desc";
  page?: number;
  pageSize?: number;
}

export function useOutOfBalanceReport(params: OutOfBalanceParams = {}) {
  return useQuery({
    queryKey: [
      "reports",
      "out-of-balance",
      params.sortBy,
      params.sortDirection,
      params.page,
      params.pageSize,
    ],
    placeholderData: keepPreviousData,
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/reports/out-of-balance",
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

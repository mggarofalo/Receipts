import { keepPreviousData, useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";

export interface SpendingByLocationParams {
  startDate?: string;
  endDate?: string;
  sortBy?: "location" | "visits" | "total" | "averagePerVisit";
  sortDirection?: "asc" | "desc";
  page?: number;
  pageSize?: number;
}

export function useSpendingByLocationReport(
  params: SpendingByLocationParams = {},
) {
  return useQuery({
    queryKey: [
      "reports",
      "spending-by-location",
      params.startDate,
      params.endDate,
      params.sortBy,
      params.sortDirection,
      params.page,
      params.pageSize,
    ],
    placeholderData: keepPreviousData,
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/reports/spending-by-location",
        {
          params: {
            query: {
              startDate: params.startDate,
              endDate: params.endDate,
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

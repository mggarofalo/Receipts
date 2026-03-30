import { keepPreviousData, useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";

export interface CategoryTrendsParams {
  startDate?: string;
  endDate?: string;
  granularity?: "daily" | "monthly" | "quarterly" | "yearly";
  topN?: number;
}

export function useCategoryTrendsReport(params: CategoryTrendsParams) {
  return useQuery({
    queryKey: [
      "reports",
      "category-trends",
      params.startDate,
      params.endDate,
      params.granularity,
      params.topN,
    ],
    placeholderData: keepPreviousData,
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/reports/category-trends",
        {
          params: {
            query: {
              startDate: params.startDate,
              endDate: params.endDate,
              granularity: params.granularity,
              topN: params.topN,
            },
          },
        },
      );
      if (error) throw error;
      return data;
    },
  });
}

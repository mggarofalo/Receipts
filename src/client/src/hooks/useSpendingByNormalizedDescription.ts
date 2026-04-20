import { keepPreviousData, useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";

export interface SpendingByNormalizedDescriptionParams {
  from?: string;
  to?: string;
}

export function useSpendingByNormalizedDescription(
  params: SpendingByNormalizedDescriptionParams = {},
) {
  return useQuery({
    queryKey: [
      "reports",
      "spending-by-normalized-description",
      params.from,
      params.to,
    ],
    placeholderData: keepPreviousData,
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/reports/spending-by-normalized-description",
        {
          params: {
            query: {
              from: params.from,
              to: params.to,
            },
          },
        },
      );
      if (error) throw error;
      return data;
    },
  });
}

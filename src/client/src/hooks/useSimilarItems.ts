import { useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";
import { useDebouncedValue } from "./useDebouncedValue";

export function useSimilarItems(
  query: string,
  options?: { limit?: number; threshold?: number; enabled?: boolean },
) {
  const debouncedQuery = useDebouncedValue(query, 300);
  const enabled =
    (options?.enabled ?? true) && debouncedQuery.length >= 2;

  return useQuery({
    queryKey: [
      "similarItems",
      debouncedQuery,
      options?.limit,
      options?.threshold,
    ],
    enabled,
    queryFn: async ({ signal }) => {
      const { data, error } = await client.GET(
        "/api/item-templates/similar",
        {
          params: {
            query: {
              q: debouncedQuery,
              limit: options?.limit ?? 5,
              threshold: options?.threshold ?? 0.3,
            },
          },
          signal,
        },
      );
      if (error) throw error;
      return data;
    },
    staleTime: 30_000,
  });
}

export function useCategoryRecommendations(
  description: string,
  options?: { limit?: number; enabled?: boolean },
) {
  const debouncedDescription = useDebouncedValue(description, 300);
  const enabled =
    (options?.enabled ?? true) && debouncedDescription.length >= 2;

  return useQuery({
    queryKey: [
      "categoryRecommendations",
      debouncedDescription,
      options?.limit,
    ],
    enabled,
    queryFn: async ({ signal }) => {
      const { data, error } = await client.GET(
        "/api/item-templates/category-suggestions",
        {
          params: {
            query: {
              q: debouncedDescription,
              limit: options?.limit ?? 5,
            },
          },
          signal,
        },
      );
      if (error) throw error;
      return data;
    },
    staleTime: 30_000,
  });
}

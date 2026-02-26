import { useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";

export function useUsers(page = 1, pageSize = 100) {
  return useQuery({
    queryKey: ["users", page, pageSize],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/users", {
        params: { query: { page, pageSize } },
      });
      if (error) throw error;
      return data;
    },
  });
}

import { useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";

export function useAccounts() {
  return useQuery({
    queryKey: ["accounts"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/accounts");
      if (error) {
        throw error;
      }
      return data;
    },
  });
}

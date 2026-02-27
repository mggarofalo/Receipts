import { useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";

export function useTripByReceiptId(receiptId: string | null) {
  return useQuery({
    queryKey: ["trips", receiptId],
    enabled: !!receiptId,
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/trips/by-receipt-id/{receiptId}",
        { params: { path: { receiptId: receiptId! } } },
      );
      if (error) throw error;
      return data;
    },
  });
}

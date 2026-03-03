import { useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";

export function useReceiptWithItems(receiptId: string | null) {
  return useQuery({
    queryKey: ["receipts-with-items", receiptId],
    enabled: !!receiptId,
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/receipts-with-items",
        { params: { query: { receiptId: receiptId! } } },
      );
      if (error) throw error;
      return data;
    },
  });
}

export function useTransactionAccount(transactionId: string | null) {
  return useQuery({
    queryKey: ["transaction-accounts", transactionId],
    enabled: !!transactionId,
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/transaction-accounts",
        { params: { query: { transactionId: transactionId! } } },
      );
      if (error) throw error;
      return data;
    },
  });
}

export function useTransactionAccountsByReceiptId(
  receiptId: string | null,
) {
  return useQuery({
    queryKey: ["transaction-accounts", "by-receipt", receiptId],
    enabled: !!receiptId,
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/transaction-accounts",
        { params: { query: { receiptId: receiptId! } } },
      );
      if (error) throw error;
      return data;
    },
  });
}

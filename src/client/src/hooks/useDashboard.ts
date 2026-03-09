import { useQuery } from "@tanstack/react-query";
import client from "@/lib/api-client";

export interface DateRange {
  startDate?: string;
  endDate?: string;
}

export function useDashboardSummary(dateRange: DateRange) {
  return useQuery({
    queryKey: ["dashboard", "summary", dateRange.startDate, dateRange.endDate],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/dashboard/summary", {
        params: {
          query: {
            startDate: dateRange.startDate,
            endDate: dateRange.endDate,
          },
        },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useDashboardSpendingOverTime(
  dateRange: DateRange,
  granularity?: string,
) {
  return useQuery({
    queryKey: [
      "dashboard",
      "spending-over-time",
      dateRange.startDate,
      dateRange.endDate,
      granularity,
    ],
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/dashboard/spending-over-time",
        {
          params: {
            query: {
              startDate: dateRange.startDate,
              endDate: dateRange.endDate,
              granularity,
            },
          },
        },
      );
      if (error) throw error;
      return data;
    },
  });
}

export function useDashboardSpendingByCategory(dateRange: DateRange) {
  return useQuery({
    queryKey: [
      "dashboard",
      "spending-by-category",
      dateRange.startDate,
      dateRange.endDate,
    ],
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/dashboard/spending-by-category",
        {
          params: {
            query: {
              startDate: dateRange.startDate,
              endDate: dateRange.endDate,
            },
          },
        },
      );
      if (error) throw error;
      return data;
    },
  });
}

export function useDashboardSpendingByAccount(dateRange: DateRange) {
  return useQuery({
    queryKey: [
      "dashboard",
      "spending-by-account",
      dateRange.startDate,
      dateRange.endDate,
    ],
    queryFn: async () => {
      const { data, error } = await client.GET(
        "/api/dashboard/spending-by-account",
        {
          params: {
            query: {
              startDate: dateRange.startDate,
              endDate: dateRange.endDate,
            },
          },
        },
      );
      if (error) throw error;
      return data;
    },
  });
}

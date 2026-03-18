import { renderHook, waitFor } from "@testing-library/react";
import { createQueryWrapper } from "@/test/test-utils";
import {
  useDashboardSummary,
  useDashboardSpendingOverTime,
  useDashboardSpendingByCategory,
  useDashboardSpendingByAccount,
} from "./useDashboard";

vi.mock("@/lib/api-client", () => ({
  default: {
    GET: vi.fn(),
  },
}));

import client from "@/lib/api-client";
const mockClient = vi.mocked(client);

describe("useDashboard hooks", () => {
  const dateRange = { startDate: "2024-01-01", endDate: "2024-01-31" };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe("useDashboardSummary", () => {
    it("fetches summary data", async () => {
      const mockData = {
        totalReceipts: 10,
        totalSpent: 500,
        averageTripAmount: 50,
        mostUsedAccount: { name: "Visa", count: 5 },
        mostUsedCategory: { name: "Food", count: 7 },
      };
      mockClient.GET.mockResolvedValue({
        data: mockData,
        error: undefined,
        response: {} as Response,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const { result } = renderHook(() => useDashboardSummary(dateRange), {
        wrapper: createQueryWrapper(),
      });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      expect(result.current.data).toEqual(mockData);
      expect(mockClient.GET).toHaveBeenCalledWith("/api/dashboard/summary", {
        params: { query: dateRange },
      });
    });

    it("throws when API returns an error", async () => {
      const apiError = { message: "Server error" };
      mockClient.GET.mockResolvedValue({
        data: undefined,
        error: apiError,
        response: {} as Response,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const { result } = renderHook(() => useDashboardSummary(dateRange), {
        wrapper: createQueryWrapper(),
      });

      await waitFor(() => expect(result.current.isError).toBe(true));
      expect(result.current.error).toEqual(apiError);
    });

    it("passes undefined dates when dateRange has no values", async () => {
      const mockData = { totalReceipts: 0 };
      mockClient.GET.mockResolvedValue({
        data: mockData,
        error: undefined,
        response: {} as Response,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const { result } = renderHook(() => useDashboardSummary({}), {
        wrapper: createQueryWrapper(),
      });

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      expect(mockClient.GET).toHaveBeenCalledWith("/api/dashboard/summary", {
        params: { query: { startDate: undefined, endDate: undefined } },
      });
    });
  });

  describe("useDashboardSpendingOverTime", () => {
    it("fetches spending over time data", async () => {
      const mockData = {
        buckets: [
          { period: "2024-01", amount: 100 },
          { period: "2024-02", amount: 200 },
        ],
      };
      mockClient.GET.mockResolvedValue({
        data: mockData,
        error: undefined,
        response: {} as Response,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const { result } = renderHook(
        () => useDashboardSpendingOverTime(dateRange, "monthly"),
        { wrapper: createQueryWrapper() },
      );

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      expect(result.current.data).toEqual(mockData);
    });

    it("throws when API returns an error", async () => {
      const apiError = { message: "Server error" };
      mockClient.GET.mockResolvedValue({
        data: undefined,
        error: apiError,
        response: {} as Response,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const { result } = renderHook(
        () => useDashboardSpendingOverTime(dateRange, "daily"),
        { wrapper: createQueryWrapper() },
      );

      await waitFor(() => expect(result.current.isError).toBe(true));
      expect(result.current.error).toEqual(apiError);
    });

    it("fetches without granularity parameter", async () => {
      const mockData = { buckets: [] };
      mockClient.GET.mockResolvedValue({
        data: mockData,
        error: undefined,
        response: {} as Response,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const { result } = renderHook(
        () => useDashboardSpendingOverTime(dateRange),
        { wrapper: createQueryWrapper() },
      );

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      expect(mockClient.GET).toHaveBeenCalledWith(
        "/api/dashboard/spending-over-time",
        {
          params: {
            query: {
              startDate: dateRange.startDate,
              endDate: dateRange.endDate,
              granularity: undefined,
            },
          },
        },
      );
    });
  });

  describe("useDashboardSpendingByCategory", () => {
    it("fetches spending by category data", async () => {
      const mockData = {
        items: [{ categoryName: "Food", amount: 300, percentage: 60 }],
      };
      mockClient.GET.mockResolvedValue({
        data: mockData,
        error: undefined,
        response: {} as Response,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const { result } = renderHook(
        () => useDashboardSpendingByCategory(dateRange),
        { wrapper: createQueryWrapper() },
      );

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      expect(result.current.data).toEqual(mockData);
    });

    it("throws when API returns an error", async () => {
      const apiError = { message: "Server error" };
      mockClient.GET.mockResolvedValue({
        data: undefined,
        error: apiError,
        response: {} as Response,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const { result } = renderHook(
        () => useDashboardSpendingByCategory(dateRange),
        { wrapper: createQueryWrapper() },
      );

      await waitFor(() => expect(result.current.isError).toBe(true));
      expect(result.current.error).toEqual(apiError);
    });

    it("passes custom limit parameter", async () => {
      const mockData = { items: [] };
      mockClient.GET.mockResolvedValue({
        data: mockData,
        error: undefined,
        response: {} as Response,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const { result } = renderHook(
        () => useDashboardSpendingByCategory(dateRange, 5),
        { wrapper: createQueryWrapper() },
      );

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      expect(mockClient.GET).toHaveBeenCalledWith(
        "/api/dashboard/spending-by-category",
        {
          params: {
            query: {
              startDate: dateRange.startDate,
              endDate: dateRange.endDate,
              limit: 5,
            },
          },
        },
      );
    });
  });

  describe("useDashboardSpendingByAccount", () => {
    it("fetches spending by account data", async () => {
      const mockData = {
        items: [
          {
            accountId: "abc",
            accountName: "Visa",
            amount: 500,
            percentage: 100,
          },
        ],
      };
      mockClient.GET.mockResolvedValue({
        data: mockData,
        error: undefined,
        response: {} as Response,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const { result } = renderHook(
        () => useDashboardSpendingByAccount(dateRange),
        { wrapper: createQueryWrapper() },
      );

      await waitFor(() => expect(result.current.isSuccess).toBe(true));
      expect(result.current.data).toEqual(mockData);
    });

    it("throws when API returns an error", async () => {
      const apiError = { message: "Server error" };
      mockClient.GET.mockResolvedValue({
        data: undefined,
        error: apiError,
        response: {} as Response,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } as any);

      const { result } = renderHook(
        () => useDashboardSpendingByAccount(dateRange),
        { wrapper: createQueryWrapper() },
      );

      await waitFor(() => expect(result.current.isError).toBe(true));
      expect(result.current.error).toEqual(apiError);
    });
  });
});

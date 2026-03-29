import { renderHook, waitFor } from "@testing-library/react";
import { createQueryWrapper } from "@/test/test-utils";
import { useSpendingByLocationReport } from "./useSpendingByLocationReport";

vi.mock("@/lib/api-client", () => ({
  default: {
    GET: vi.fn(),
  },
}));

import client from "@/lib/api-client";
const mockClient = vi.mocked(client);

describe("useSpendingByLocationReport", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("fetches report data with default parameters", async () => {
    const mockData = {
      totalCount: 2,
      grandTotal: 150.5,
      items: [
        {
          location: "Store A",
          visits: 5,
          total: 100.5,
          averagePerVisit: 20.1,
        },
      ],
    };
    mockClient.GET.mockResolvedValue({
      data: mockData,
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useSpendingByLocationReport(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(mockData);
    expect(mockClient.GET).toHaveBeenCalledWith(
      "/api/reports/spending-by-location",
      {
        params: {
          query: {
            startDate: undefined,
            endDate: undefined,
            sortBy: undefined,
            sortDirection: undefined,
            page: undefined,
            pageSize: undefined,
          },
        },
      },
    );
  });

  it("passes custom parameters", async () => {
    const mockData = { totalCount: 0, grandTotal: 0, items: [] };
    mockClient.GET.mockResolvedValue({
      data: mockData,
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(
      () =>
        useSpendingByLocationReport({
          startDate: "2025-01-01",
          endDate: "2025-12-31",
          sortBy: "visits",
          sortDirection: "asc",
          page: 2,
          pageSize: 25,
        }),
      { wrapper: createQueryWrapper() },
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(mockClient.GET).toHaveBeenCalledWith(
      "/api/reports/spending-by-location",
      {
        params: {
          query: {
            startDate: "2025-01-01",
            endDate: "2025-12-31",
            sortBy: "visits",
            sortDirection: "asc",
            page: 2,
            pageSize: 25,
          },
        },
      },
    );
  });

  it("throws when API returns an error", async () => {
    const apiError = { message: "Server error" };
    mockClient.GET.mockResolvedValue({
      data: undefined,
      error: apiError,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useSpendingByLocationReport(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error).toEqual(apiError);
  });
});

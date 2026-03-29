import { renderHook, waitFor } from "@testing-library/react";
import { createQueryWrapper } from "@/test/test-utils";
import { useOutOfBalanceReport } from "./useOutOfBalanceReport";

vi.mock("@/lib/api-client", () => ({
  default: {
    GET: vi.fn(),
  },
}));

import client from "@/lib/api-client";
const mockClient = vi.mocked(client);

describe("useOutOfBalanceReport", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("fetches report data with default parameters", async () => {
    const mockData = {
      totalCount: 2,
      totalDiscrepancy: 10.5,
      items: [
        {
          receiptId: "abc",
          location: "Store A",
          date: "2025-03-01",
          itemSubtotal: 10,
          taxAmount: 1,
          adjustmentTotal: 0,
          expectedTotal: 11,
          transactionTotal: 15,
          difference: -4,
        },
      ],
    };
    mockClient.GET.mockResolvedValue({
      data: mockData,
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useOutOfBalanceReport(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(mockData);
    expect(mockClient.GET).toHaveBeenCalledWith(
      "/api/reports/out-of-balance",
      {
        params: {
          query: {
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
    const mockData = { totalCount: 0, totalDiscrepancy: 0, items: [] };
    mockClient.GET.mockResolvedValue({
      data: mockData,
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(
      () =>
        useOutOfBalanceReport({
          sortBy: "difference",
          sortDirection: "desc",
          page: 2,
          pageSize: 25,
        }),
      { wrapper: createQueryWrapper() },
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(mockClient.GET).toHaveBeenCalledWith(
      "/api/reports/out-of-balance",
      {
        params: {
          query: {
            sortBy: "difference",
            sortDirection: "desc",
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

    const { result } = renderHook(() => useOutOfBalanceReport(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error).toEqual(apiError);
  });
});

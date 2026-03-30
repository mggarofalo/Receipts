import { renderHook, waitFor } from "@testing-library/react";
import { createQueryWrapper } from "@/test/test-utils";
import { useDuplicateDetectionReport } from "./useDuplicateDetectionReport";

vi.mock("@/lib/api-client", () => ({
  default: {
    GET: vi.fn(),
  },
}));

import client from "@/lib/api-client";
const mockClient = vi.mocked(client);

describe("useDuplicateDetectionReport", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("fetches report data with default parameters", async () => {
    const mockData = {
      groupCount: 1,
      totalDuplicateReceipts: 2,
      groups: [
        {
          matchKey: "2025-03-01 @ Store A",
          receipts: [
            {
              receiptId: "id-1",
              location: "Store A",
              date: "2025-03-01",
              transactionTotal: 25.5,
            },
            {
              receiptId: "id-2",
              location: "Store A",
              date: "2025-03-01",
              transactionTotal: 30.0,
            },
          ],
        },
      ],
    };
    mockClient.GET.mockResolvedValue({
      data: mockData,
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useDuplicateDetectionReport(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(mockData);
    expect(mockClient.GET).toHaveBeenCalledWith("/api/reports/duplicates", {
      params: {
        query: {
          matchOn: undefined,
          locationTolerance: undefined,
          totalTolerance: undefined,
        },
      },
    });
  });

  it("passes custom parameters", async () => {
    const mockData = {
      groupCount: 0,
      totalDuplicateReceipts: 0,
      groups: [],
    };
    mockClient.GET.mockResolvedValue({
      data: mockData,
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(
      () =>
        useDuplicateDetectionReport({
          matchOn: "DateAndTotal",
          locationTolerance: "normalized",
          totalTolerance: 0.05,
        }),
      { wrapper: createQueryWrapper() },
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(mockClient.GET).toHaveBeenCalledWith("/api/reports/duplicates", {
      params: {
        query: {
          matchOn: "DateAndTotal",
          locationTolerance: "normalized",
          totalTolerance: 0.05,
        },
      },
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

    const { result } = renderHook(() => useDuplicateDetectionReport(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error).toEqual(apiError);
  });
});

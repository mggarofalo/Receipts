import { renderHook, waitFor } from "@testing-library/react";
import { createQueryWrapper } from "@/test/test-utils";
import { useSpendingByNormalizedDescription } from "./useSpendingByNormalizedDescription";

vi.mock("@/lib/api-client", () => ({
  default: {
    GET: vi.fn(),
  },
}));

import client from "@/lib/api-client";
const mockClient = vi.mocked(client);

describe("useSpendingByNormalizedDescription", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("fetches report with no params", async () => {
    const mockData = { items: [] };
    mockClient.GET.mockResolvedValue({
      data: mockData,
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useSpendingByNormalizedDescription(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(mockData);
    expect(mockClient.GET).toHaveBeenCalledWith(
      "/api/reports/spending-by-normalized-description",
      {
        params: {
          query: {
            from: undefined,
            to: undefined,
          },
        },
      },
    );
  });

  it("passes from/to params", async () => {
    mockClient.GET.mockResolvedValue({
      data: { items: [] },
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(
      () =>
        useSpendingByNormalizedDescription({
          from: "2025-01-01",
          to: "2025-12-31",
        }),
      { wrapper: createQueryWrapper() },
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(mockClient.GET).toHaveBeenCalledWith(
      "/api/reports/spending-by-normalized-description",
      {
        params: {
          query: {
            from: "2025-01-01",
            to: "2025-12-31",
          },
        },
      },
    );
  });

  it("propagates errors", async () => {
    const apiError = { message: "Server error" };
    mockClient.GET.mockResolvedValue({
      data: undefined,
      error: apiError,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useSpendingByNormalizedDescription(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error).toEqual(apiError);
  });
});

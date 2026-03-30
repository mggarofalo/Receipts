import { renderHook, waitFor } from "@testing-library/react";
import { createQueryWrapper } from "@/test/test-utils";
import {
  useItemSimilarityReport,
  useRenameItemSimilarityGroup,
} from "./useItemSimilarityReport";

vi.mock("@/lib/api-client", () => ({
  default: {
    GET: vi.fn(),
    POST: vi.fn(),
  },
}));

import client from "@/lib/api-client";
const mockClient = vi.mocked(client);

describe("useItemSimilarityReport", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("fetches report data with default parameters", async () => {
    const mockData = {
      totalCount: 1,
      groups: [
        {
          canonicalName: "Milk",
          variants: ["Milk", "MILK"],
          itemIds: ["id-1", "id-2"],
          occurrences: 2,
          maxSimilarity: 0.85,
        },
      ],
    };
    mockClient.GET.mockResolvedValue({
      data: mockData,
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useItemSimilarityReport(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(mockData);
    expect(mockClient.GET).toHaveBeenCalledWith(
      "/api/reports/item-similarity",
      {
        params: {
          query: {
            threshold: undefined,
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
    const mockData = { totalCount: 0, groups: [] };
    mockClient.GET.mockResolvedValue({
      data: mockData,
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(
      () =>
        useItemSimilarityReport({
          threshold: 0.5,
          sortBy: "canonicalName",
          sortDirection: "asc",
          page: 2,
          pageSize: 25,
        }),
      { wrapper: createQueryWrapper() },
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(mockClient.GET).toHaveBeenCalledWith(
      "/api/reports/item-similarity",
      {
        params: {
          query: {
            threshold: 0.5,
            sortBy: "canonicalName",
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

    const { result } = renderHook(() => useItemSimilarityReport(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error).toEqual(apiError);
  });
});

describe("useRenameItemSimilarityGroup", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("calls POST with correct parameters", async () => {
    mockClient.POST.mockResolvedValue({
      data: { updatedCount: 3 },
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useRenameItemSimilarityGroup(), {
      wrapper: createQueryWrapper(),
    });

    result.current.mutate({
      itemIds: ["id-1", "id-2", "id-3"],
      newDescription: "Milk",
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(mockClient.POST).toHaveBeenCalledWith(
      "/api/reports/item-similarity/rename",
      {
        body: {
          itemIds: ["id-1", "id-2", "id-3"],
          newDescription: "Milk",
        },
      },
    );
  });

  it("throws when API returns an error", async () => {
    const apiError = { message: "Server error" };
    mockClient.POST.mockResolvedValue({
      data: undefined,
      error: apiError,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useRenameItemSimilarityGroup(), {
      wrapper: createQueryWrapper(),
    });

    result.current.mutate({
      itemIds: ["id-1"],
      newDescription: "Test",
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
  });
});

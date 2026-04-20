import { renderHook, waitFor } from "@testing-library/react";
import { createQueryWrapper } from "@/test/test-utils";
import {
  useNormalizedDescriptions,
  useNormalizedDescription,
} from "./useNormalizedDescriptions";

vi.mock("@/lib/api-client", () => ({
  default: {
    GET: vi.fn(),
  },
}));

import client from "@/lib/api-client";
const mockClient = vi.mocked(client);

describe("useNormalizedDescriptions", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("fetches list with no filter", async () => {
    const mockData = {
      items: [
        {
          id: "n-1",
          canonicalName: "Apples",
          status: "active",
          createdAt: "2025-01-01T00:00:00Z",
        },
      ],
      totalCount: 1,
    };
    mockClient.GET.mockResolvedValue({
      data: mockData,
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useNormalizedDescriptions(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(mockData);
    expect(mockClient.GET).toHaveBeenCalledWith(
      "/api/normalized-descriptions",
      { params: { query: { status: undefined } } },
    );
  });

  it("fetches list with PendingReview filter", async () => {
    mockClient.GET.mockResolvedValue({
      data: { items: [], totalCount: 0 },
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(
      () => useNormalizedDescriptions("PendingReview"),
      { wrapper: createQueryWrapper() },
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(mockClient.GET).toHaveBeenCalledWith(
      "/api/normalized-descriptions",
      { params: { query: { status: "PendingReview" } } },
    );
  });

  it("propagates API errors", async () => {
    const apiError = { message: "Server error" };
    mockClient.GET.mockResolvedValue({
      data: undefined,
      error: apiError,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useNormalizedDescriptions(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error).toEqual(apiError);
  });
});

describe("useNormalizedDescription", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("does not fetch when id is null", () => {
    const { result } = renderHook(() => useNormalizedDescription(null), {
      wrapper: createQueryWrapper(),
    });
    expect(result.current.isPending).toBe(true);
    expect(mockClient.GET).not.toHaveBeenCalled();
  });

  it("fetches by id", async () => {
    const mockData = {
      id: "n-1",
      canonicalName: "Apples",
      status: "active",
      createdAt: "2025-01-01T00:00:00Z",
    };
    mockClient.GET.mockResolvedValue({
      data: mockData,
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useNormalizedDescription("n-1"), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(mockData);
    expect(mockClient.GET).toHaveBeenCalledWith(
      "/api/normalized-descriptions/{id}",
      { params: { path: { id: "n-1" } } },
    );
  });
});

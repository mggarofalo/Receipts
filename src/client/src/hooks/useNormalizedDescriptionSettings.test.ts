import { renderHook, waitFor } from "@testing-library/react";
import { createQueryWrapper } from "@/test/test-utils";
import {
  useSettings,
  useUpdateSettingsMutation,
  useTestMatchMutation,
  usePreviewImpactMutation,
} from "./useNormalizedDescriptionSettings";

vi.mock("@/lib/api-client", () => ({
  default: {
    GET: vi.fn(),
    POST: vi.fn(),
    PATCH: vi.fn(),
  },
}));

vi.mock("sonner", () => ({
  toast: { success: vi.fn(), error: vi.fn() },
}));

import client from "@/lib/api-client";
const mockClient = vi.mocked(client);

describe("useSettings", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("fetches live settings", async () => {
    const mockData = {
      id: "00000000-0000-0000-0000-000000000001",
      autoAcceptThreshold: 0.9,
      pendingReviewThreshold: 0.75,
      updatedAt: "2025-01-01T00:00:00Z",
    };
    mockClient.GET.mockResolvedValue({
      data: mockData,
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useSettings(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(mockData);
    expect(mockClient.GET).toHaveBeenCalledWith(
      "/api/normalized-descriptions/settings",
    );
  });
});

describe("useUpdateSettingsMutation", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("patches settings with thresholds", async () => {
    mockClient.PATCH.mockResolvedValue({
      data: {
        id: "n-1",
        autoAcceptThreshold: 0.95,
        pendingReviewThreshold: 0.7,
        updatedAt: "2025-01-01T00:00:00Z",
      },
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useUpdateSettingsMutation(), {
      wrapper: createQueryWrapper(),
    });

    result.current.mutate({
      autoAcceptThreshold: 0.95,
      pendingReviewThreshold: 0.7,
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(mockClient.PATCH).toHaveBeenCalledWith(
      "/api/normalized-descriptions/settings",
      {
        body: {
          autoAcceptThreshold: 0.95,
          pendingReviewThreshold: 0.7,
        },
      },
    );
  });

  it("propagates errors", async () => {
    mockClient.PATCH.mockResolvedValue({
      data: undefined,
      error: { message: "nope" },
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);
    const { result } = renderHook(() => useUpdateSettingsMutation(), {
      wrapper: createQueryWrapper(),
    });
    result.current.mutate({
      autoAcceptThreshold: 0.9,
      pendingReviewThreshold: 0.7,
    });
    await waitFor(() => expect(result.current.isError).toBe(true));
  });
});

describe("useTestMatchMutation", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("posts test description with defaults", async () => {
    mockClient.POST.mockResolvedValue({
      data: { candidates: [], simulatedOutcome: "CreateNew" },
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useTestMatchMutation(), {
      wrapper: createQueryWrapper(),
    });

    result.current.mutate({ description: "apples" });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(mockClient.POST).toHaveBeenCalledWith(
      "/api/normalized-descriptions/test",
      {
        body: {
          description: "apples",
          topN: 5,
          autoAcceptThresholdOverride: undefined,
          pendingReviewThresholdOverride: undefined,
        },
      },
    );
  });

  it("passes overrides", async () => {
    mockClient.POST.mockResolvedValue({
      data: { candidates: [], simulatedOutcome: "AutoAccept" },
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => useTestMatchMutation(), {
      wrapper: createQueryWrapper(),
    });

    result.current.mutate({
      description: "apples",
      topN: 10,
      autoAcceptThresholdOverride: 0.8,
      pendingReviewThresholdOverride: 0.6,
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(mockClient.POST).toHaveBeenCalledWith(
      "/api/normalized-descriptions/test",
      {
        body: {
          description: "apples",
          topN: 10,
          autoAcceptThresholdOverride: 0.8,
          pendingReviewThresholdOverride: 0.6,
        },
      },
    );
  });
});

describe("usePreviewImpactMutation", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("posts threshold preview", async () => {
    mockClient.POST.mockResolvedValue({
      data: {
        current: { autoAccepted: 1, pendingReview: 2, unresolved: 3 },
        proposed: { autoAccepted: 2, pendingReview: 2, unresolved: 2 },
        deltas: {
          autoToPending: 0,
          pendingToAuto: 1,
          unresolvedToAuto: 0,
          unresolvedToPending: 0,
        },
      },
      error: undefined,
      response: {} as Response,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as any);

    const { result } = renderHook(() => usePreviewImpactMutation(), {
      wrapper: createQueryWrapper(),
    });

    result.current.mutate({
      autoAcceptThreshold: 0.9,
      pendingReviewThreshold: 0.7,
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(mockClient.POST).toHaveBeenCalledWith(
      "/api/normalized-descriptions/settings/preview",
      {
        body: {
          autoAcceptThreshold: 0.9,
          pendingReviewThreshold: 0.7,
        },
      },
    );
  });
});

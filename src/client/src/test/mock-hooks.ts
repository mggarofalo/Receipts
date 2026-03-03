import type { UseQueryResult, UseMutationResult } from "@tanstack/react-query";

/**
 * Creates a mock UseQueryResult with sensible defaults.
 * Use this instead of `as unknown as ReturnType<typeof useXxx>` double-casts.
 *
 * The return is typed as `any` so it can be passed to `mockReturnValue`
 * on any query hook without needing a cast at the call site.
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function mockQueryResult(overrides: Record<string, any> = {}): any {
  return {
    data: undefined,
    error: null,
    isError: false,
    isPending: true,
    isLoading: true,
    isLoadingError: false,
    isRefetchError: false,
    isSuccess: false,
    isFetching: false,
    isFetched: false,
    isFetchedAfterMount: false,
    isRefetching: false,
    isStale: false,
    isPlaceholderData: false,
    status: "pending" as const,
    fetchStatus: "idle" as const,
    dataUpdatedAt: 0,
    errorUpdatedAt: 0,
    failureCount: 0,
    failureReason: null,
    errorUpdateCount: 0,
    refetch: vi.fn(),
    promise: Promise.resolve(undefined),
    ...overrides,
  };
}

/**
 * Creates a mock UseMutationResult with sensible defaults.
 * Use this instead of `as unknown as ReturnType<typeof useXxx>` double-casts
 * for mutation hooks.
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function mockMutationResult(overrides: Record<string, any> = {}): any {
  return {
    data: undefined,
    error: null,
    isError: false,
    isIdle: true,
    isPending: false,
    isSuccess: false,
    status: "idle" as const,
    failureCount: 0,
    failureReason: null,
    mutate: vi.fn(),
    mutateAsync: vi.fn(),
    reset: vi.fn(),
    context: undefined,
    variables: undefined,
    submittedAt: 0,
    isPaused: false,
    ...overrides,
  };
}

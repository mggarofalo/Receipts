import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, act } from "@testing-library/react";

// vi.hoisted() runs before vi.mock factories and before module-level code.
// All shared state and mocks that are referenced in vi.mock factories must
// be created here to avoid temporal dead zone errors.
const mocks = vi.hoisted(() => {
  const hubState = {
    onReconnecting: undefined as (() => void) | undefined,
    onReconnected: undefined as (() => void) | undefined,
    onClose: undefined as (() => void) | undefined,
    events: {} as Record<string, Array<(...args: unknown[]) => void>>,
  };

  const mockStart = vi.fn();
  const mockStop = vi.fn();

  // Connection object returned by builder.build()
  const mockConnection = {
    on(event: string, cb: (...args: unknown[]) => void) {
      hubState.events[event] = hubState.events[event] ?? [];
      hubState.events[event].push(cb);
    },
    start: mockStart,
    stop: mockStop,
    onreconnecting(cb: () => void) {
      hubState.onReconnecting = cb;
    },
    onreconnected(cb: () => void) {
      hubState.onReconnected = cb;
    },
    onclose(cb: () => void) {
      hubState.onClose = cb;
    },
  };

  // Builder object returned by new HubConnectionBuilder()
  const mockBuilder = {
    withUrl() {
      return mockBuilder;
    },
    withAutomaticReconnect() {
      return mockBuilder;
    },
    configureLogging() {
      return mockBuilder;
    },
    build() {
      return mockConnection;
    },
  };

  return {
    hubState,
    mockStart,
    mockStop,
    mockBuilder,
    mockInvalidateQueries: vi.fn(),
    mockToastInfo: vi.fn(),
  };
});

// HubConnectionBuilder must be a constructable function (not an arrow fn).
// vitest v4 requires regular functions for constructor mocks.
vi.mock("@microsoft/signalr", () => ({
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  HubConnectionBuilder: function MockHubConnectionBuilder(this: any) {
    return mocks.mockBuilder;
  },
  LogLevel: { Debug: 1, None: 6 },
}));

vi.mock("@tanstack/react-query", () => ({
  useQueryClient: vi.fn(() => ({
    invalidateQueries: mocks.mockInvalidateQueries,
  })),
}));

vi.mock("sonner", () => ({
  toast: { info: mocks.mockToastInfo },
}));

vi.mock("@/lib/auth", () => ({
  getAccessToken: vi.fn(() => "mock-token"),
}));

import { useSignalR } from "./useSignalR";

const { hubState, mockStart, mockStop, mockInvalidateQueries, mockToastInfo } =
  mocks;

describe("useSignalR – auto-reconnect state transitions", () => {
  beforeEach(() => {
    mockStart.mockClear();
    mockStart.mockResolvedValue(undefined);
    mockStop.mockClear();
    mockInvalidateQueries.mockClear();
    mockToastInfo.mockClear();
    hubState.onReconnecting = undefined;
    hubState.onReconnected = undefined;
    hubState.onClose = undefined;
    hubState.events = {};
  });

  it("starts in disconnected state when disabled", () => {
    const { result } = renderHook(() => useSignalR(false));
    expect(result.current.connectionState).toBe("disconnected");
  });

  it("transitions to connected state after the hub connects", async () => {
    const { result } = renderHook(() => useSignalR(true));

    await act(async () => {
      await Promise.resolve();
    });

    expect(result.current.connectionState).toBe("connected");
  });

  it("transitions to reconnecting state when the hub fires onreconnecting", async () => {
    const { result } = renderHook(() => useSignalR(true));

    await act(async () => {
      await Promise.resolve();
    });

    act(() => {
      hubState.onReconnecting?.();
    });

    expect(result.current.connectionState).toBe("reconnecting");
  });

  it("transitions back to connected state when the hub fires onreconnected", async () => {
    const { result } = renderHook(() => useSignalR(true));

    await act(async () => {
      await Promise.resolve();
    });

    act(() => {
      hubState.onReconnecting?.();
    });
    act(() => {
      hubState.onReconnected?.();
    });

    expect(result.current.connectionState).toBe("connected");
  });

  it("transitions to disconnected state when the connection closes", async () => {
    const { result } = renderHook(() => useSignalR(true));

    await act(async () => {
      await Promise.resolve();
    });

    act(() => {
      hubState.onClose?.();
    });

    expect(result.current.connectionState).toBe("disconnected");
  });
});

describe("useSignalR – TanStack Query cache invalidation on SignalR events", () => {
  beforeEach(() => {
    mockStart.mockClear();
    mockStart.mockResolvedValue(undefined);
    mockStop.mockClear();
    mockInvalidateQueries.mockClear();
    mockToastInfo.mockClear();
    hubState.onReconnecting = undefined;
    hubState.onReconnected = undefined;
    hubState.onClose = undefined;
    hubState.events = {};
  });

  it("invalidates the receipts query cache when ReceiptCreated fires", async () => {
    renderHook(() => useSignalR(true));

    await act(async () => {
      await Promise.resolve();
    });

    act(() => {
      hubState.events["ReceiptCreated"]?.[0]?.({ id: "abc" });
    });

    expect(mockInvalidateQueries).toHaveBeenCalledWith({
      queryKey: ["receipts"],
    });
  });

  it("invalidates the receipts query cache when ReceiptUpdated fires", async () => {
    renderHook(() => useSignalR(true));

    await act(async () => {
      await Promise.resolve();
    });

    act(() => {
      hubState.events["ReceiptUpdated"]?.[0]?.({ id: "abc" });
    });

    expect(mockInvalidateQueries).toHaveBeenCalledWith({
      queryKey: ["receipts"],
    });
  });

  it("invalidates the receipts query cache when ReceiptDeleted fires", async () => {
    renderHook(() => useSignalR(true));

    await act(async () => {
      await Promise.resolve();
    });

    act(() => {
      hubState.events["ReceiptDeleted"]?.[0]?.("abc");
    });

    expect(mockInvalidateQueries).toHaveBeenCalledWith({
      queryKey: ["receipts"],
    });
  });

  it("shows a toast notification when ReceiptCreated fires", async () => {
    renderHook(() => useSignalR(true));

    await act(async () => {
      await Promise.resolve();
    });

    act(() => {
      hubState.events["ReceiptCreated"]?.[0]?.({ id: "abc" });
    });

    expect(mockToastInfo).toHaveBeenCalled();
  });
});

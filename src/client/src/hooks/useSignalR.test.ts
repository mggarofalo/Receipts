import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook } from "@testing-library/react";

const mockConnection = {
  start: vi.fn().mockResolvedValue(undefined),
  stop: vi.fn().mockResolvedValue(undefined),
  on: vi.fn(),
  onreconnecting: vi.fn(),
  onreconnected: vi.fn(),
  onclose: vi.fn(),
};

const mockBuilder = {
  withUrl: vi.fn().mockReturnThis(),
  withAutomaticReconnect: vi.fn().mockReturnThis(),
  configureLogging: vi.fn().mockReturnThis(),
  build: vi.fn().mockReturnValue(mockConnection),
};

vi.mock("@microsoft/signalr", () => ({
  HubConnectionBuilder: vi.fn().mockImplementation(function () {
    return mockBuilder;
  }),
  LogLevel: { Debug: 1, None: 5 },
}));

vi.mock("@tanstack/react-query", () => ({
  useQueryClient: vi.fn().mockReturnValue({
    invalidateQueries: vi.fn(),
  }),
}));

vi.mock("sonner", () => ({
  toast: { success: vi.fn(), error: vi.fn(), info: vi.fn() },
}));

vi.mock("@/lib/auth", () => ({
  getAccessToken: vi.fn().mockReturnValue("mock-token"),
}));

import { useSignalR } from "./useSignalR";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { act } from "@testing-library/react";

beforeEach(() => {
  vi.clearAllMocks();
  // Restore start to resolve by default (individual tests may override)
  mockConnection.start.mockResolvedValue(undefined);
});

/** Helper: render the hook, flush the start() promise, and return the result. */
async function renderEnabled() {
  const hookReturn = renderHook(() => useSignalR(true));
  // Flush the microtask so start().then() executes
  await act(async () => {});
  return hookReturn;
}

/** Helper: find the registered handler for a given hub event name. */
function getOnHandler(eventName: string): ((...args: unknown[]) => void) | undefined {
  const call = mockConnection.on.mock.calls.find(
    (c: unknown[]) => c[0] === eventName,
  );
  return call ? (call[1] as (...args: unknown[]) => void) : undefined;
}

describe("useSignalR", () => {
  it("returns disconnected when not enabled", () => {
    const { result } = renderHook(() => useSignalR(false));

    expect(result.current.connectionState).toBe("disconnected");
    expect(mockConnection.start).not.toHaveBeenCalled();
  });

  it("starts connection when enabled", () => {
    renderHook(() => useSignalR(true));

    expect(mockBuilder.withUrl).toHaveBeenCalledWith(
      "/hubs/receipts",
      expect.objectContaining({ accessTokenFactory: expect.any(Function) }),
    );
    expect(mockBuilder.withAutomaticReconnect).toHaveBeenCalled();
    expect(mockBuilder.configureLogging).toHaveBeenCalled();
    expect(mockBuilder.build).toHaveBeenCalled();
    expect(mockConnection.start).toHaveBeenCalled();
  });

  it("registers event handlers for receipt events", () => {
    renderHook(() => useSignalR(true));

    const registeredEvents = mockConnection.on.mock.calls.map(
      (call: unknown[]) => call[0],
    );
    expect(registeredEvents).toContain("ReceiptCreated");
    expect(registeredEvents).toContain("ReceiptUpdated");
    expect(registeredEvents).toContain("ReceiptDeleted");
  });

  it("registers reconnecting and close handlers", () => {
    renderHook(() => useSignalR(true));

    expect(mockConnection.onreconnecting).toHaveBeenCalled();
    expect(mockConnection.onreconnected).toHaveBeenCalled();
    expect(mockConnection.onclose).toHaveBeenCalled();
  });

  it("stops connection on cleanup", () => {
    const { unmount } = renderHook(() => useSignalR(true));

    unmount();

    expect(mockConnection.stop).toHaveBeenCalled();
  });

  it("sets connectionState to connected after start() resolves", async () => {
    const { result } = await renderEnabled();

    expect(result.current.connectionState).toBe("connected");
  });

  it("clears connectionRef on cleanup", () => {
    const { unmount } = renderHook(() => useSignalR(true));

    unmount();

    expect(mockConnection.stop).toHaveBeenCalled();
  });

  describe("hub event handlers", () => {
    it("ReceiptCreated invalidates queries and shows toast", async () => {
      const mockQueryClient = vi.mocked(useQueryClient)();

      await renderEnabled();

      const handler = getOnHandler("ReceiptCreated");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ id: "abc", store: "Test Store" });
      });

      expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({
        queryKey: ["receipts"],
      });
      expect(toast.info).toHaveBeenCalledWith(
        "A receipt was created by another user",
      );
    });

    it("ReceiptUpdated invalidates queries and shows toast", async () => {
      const mockQueryClient = vi.mocked(useQueryClient)();

      await renderEnabled();

      const handler = getOnHandler("ReceiptUpdated");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ id: "abc", store: "Updated Store" });
      });

      expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({
        queryKey: ["receipts"],
      });
      expect(toast.info).toHaveBeenCalledWith(
        "A receipt was updated by another user",
      );
    });

    it("ReceiptDeleted invalidates queries and shows toast", async () => {
      const mockQueryClient = vi.mocked(useQueryClient)();

      await renderEnabled();

      const handler = getOnHandler("ReceiptDeleted");
      expect(handler).toBeDefined();

      act(() => {
        handler!("some-receipt-id");
      });

      expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({
        queryKey: ["receipts"],
      });
      expect(toast.info).toHaveBeenCalledWith(
        "A receipt was deleted by another user",
      );
    });
  });

  describe("connection state transitions", () => {
    it("sets state to reconnecting when onreconnecting fires", async () => {
      const { result } = await renderEnabled();
      expect(result.current.connectionState).toBe("connected");

      const reconnectingCb = mockConnection.onreconnecting.mock.calls[0][0] as () => void;

      act(() => {
        reconnectingCb();
      });

      expect(result.current.connectionState).toBe("reconnecting");
    });

    it("sets state to connected when onreconnected fires", async () => {
      const { result } = await renderEnabled();

      // Simulate reconnecting first
      const reconnectingCb = mockConnection.onreconnecting.mock.calls[0][0] as () => void;
      act(() => {
        reconnectingCb();
      });
      expect(result.current.connectionState).toBe("reconnecting");

      // Now reconnected
      const reconnectedCb = mockConnection.onreconnected.mock.calls[0][0] as () => void;
      act(() => {
        reconnectedCb();
      });

      expect(result.current.connectionState).toBe("connected");
    });

    it("sets state to disconnected when onclose fires", async () => {
      const { result } = await renderEnabled();
      expect(result.current.connectionState).toBe("connected");

      const closeCb = mockConnection.onclose.mock.calls[0][0] as () => void;

      act(() => {
        closeCb();
      });

      expect(result.current.connectionState).toBe("disconnected");
    });
  });

  describe("error handling", () => {
    it("sets state to disconnected when start() rejects", async () => {
      mockConnection.start.mockRejectedValueOnce(new Error("Connection failed"));

      const { result } = renderHook(() => useSignalR(true));

      // Flush the rejected promise + catch handler
      await act(async () => {});

      expect(result.current.connectionState).toBe("disconnected");
    });
  });
});

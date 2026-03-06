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

vi.mock("@/lib/signalr-toast-buffer", () => ({
  bufferToast: vi.fn(),
}));

vi.mock("@/lib/auth", () => ({
  getAccessToken: vi.fn().mockReturnValue("mock-token"),
}));

import { useSignalR } from "./useSignalR";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { bufferToast } from "@/lib/signalr-toast-buffer";
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
      "/hubs/entities",
      expect.objectContaining({ accessTokenFactory: expect.any(Function) }),
    );
    expect(mockBuilder.withAutomaticReconnect).toHaveBeenCalled();
    expect(mockBuilder.configureLogging).toHaveBeenCalled();
    expect(mockBuilder.build).toHaveBeenCalled();
    expect(mockConnection.start).toHaveBeenCalled();
  });

  it("registers EntityChanged event handler", () => {
    renderHook(() => useSignalR(true));

    const registeredEvents = mockConnection.on.mock.calls.map(
      (call: unknown[]) => call[0],
    );
    expect(registeredEvents).toContain("EntityChanged");
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

  describe("EntityChanged handler", () => {
    it("invalidates receipt query keys and buffers toast for receipt created", async () => {
      const mockQueryClient = vi.mocked(useQueryClient)();

      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "receipt", changeType: "created", id: "abc-123", count: 1 });
      });

      expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({
        queryKey: ["receipts"],
      });
      expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({
        queryKey: ["receipts-with-items"],
      });
      expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({
        queryKey: ["trips"],
      });
      expect(bufferToast).toHaveBeenCalledWith("receipt", "created", 1);
      expect(toast.info).not.toHaveBeenCalled();
    });

    it("invalidates account query keys and buffers toast for account updated", async () => {
      const mockQueryClient = vi.mocked(useQueryClient)();

      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "account", changeType: "updated", id: "abc-123", count: 1 });
      });

      expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({
        queryKey: ["accounts"],
      });
      expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({
        queryKey: ["transaction-accounts"],
      });
      expect(bufferToast).toHaveBeenCalledWith("account", "updated", 1);
      expect(toast.info).not.toHaveBeenCalled();
    });

    it("invalidates transaction query keys and buffers toast for transaction deleted", async () => {
      const mockQueryClient = vi.mocked(useQueryClient)();

      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "transaction", changeType: "deleted", id: "abc-123", count: 1 });
      });

      expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({
        queryKey: ["transactions"],
      });
      expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({
        queryKey: ["receipts-with-items"],
      });
      expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({
        queryKey: ["trips"],
      });
      expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({
        queryKey: ["transaction-accounts"],
      });
      expect(bufferToast).toHaveBeenCalledWith("transaction", "deleted", 1);
      expect(toast.info).not.toHaveBeenCalled();
    });

    it("buffers toast with display name for receipt-item", async () => {
      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "receipt-item", changeType: "created", id: "abc", count: 1 });
      });

      expect(bufferToast).toHaveBeenCalledWith("receipt item", "created", 1);
      expect(toast.info).not.toHaveBeenCalled();
    });

    it("buffers toast with display name for item-template", async () => {
      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "item-template", changeType: "updated", id: "abc", count: 1 });
      });

      expect(bufferToast).toHaveBeenCalledWith("item template", "updated", 1);
      expect(toast.info).not.toHaveBeenCalled();
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

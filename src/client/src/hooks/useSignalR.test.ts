import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook } from "@testing-library/react";

const mockConnection = {
  start: vi.fn().mockResolvedValue(undefined),
  stop: vi.fn().mockResolvedValue(undefined),
  on: vi.fn(),
  onreconnecting: vi.fn(),
  onreconnected: vi.fn(),
  onclose: vi.fn(),
  connectionId: "mock-conn-id",
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
  parseJwtPayload: vi.fn().mockReturnValue({
    userId: "current-user-id",
    email: "user@example.com",
    roles: [],
    mustResetPassword: false,
  }),
}));

vi.mock("@/lib/signalr-connection", () => ({
  setConnectionId: vi.fn(),
  getConnectionId: vi.fn().mockReturnValue("mock-conn-id"),
}));

import { useSignalR } from "./useSignalR";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { bufferToast } from "@/lib/signalr-toast-buffer";
import { act } from "@testing-library/react";
import { setConnectionId, getConnectionId } from "@/lib/signalr-connection";
import { parseJwtPayload } from "@/lib/auth";

beforeEach(() => {
  vi.clearAllMocks();
  // Restore start to resolve by default (individual tests may override)
  mockConnection.start.mockResolvedValue(undefined);
  mockConnection.connectionId = "mock-conn-id";
  vi.mocked(getConnectionId).mockReturnValue("mock-conn-id");
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

  it("sets connectionId on connect", async () => {
    await renderEnabled();

    expect(setConnectionId).toHaveBeenCalledWith("mock-conn-id");
  });

  it("clears connectionRef on cleanup", () => {
    const { unmount } = renderHook(() => useSignalR(true));

    unmount();

    expect(mockConnection.stop).toHaveBeenCalled();
    expect(setConnectionId).toHaveBeenCalledWith(null);
  });

  describe("EntityChanged handler", () => {
    it("invalidates receipt query keys and buffers toast for receipt created", async () => {
      const mockQueryClient = vi.mocked(useQueryClient)();

      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "receipt", changeType: "created", id: "abc-123", count: 1, userId: "other-user-id", authMethod: "jwt", connectionId: "other-conn" });
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
      expect(bufferToast).toHaveBeenCalledWith("receipt", "created", 1, "other-user");
      expect(toast.info).not.toHaveBeenCalled();
    });

    it("invalidates account query keys and buffers toast for account updated", async () => {
      const mockQueryClient = vi.mocked(useQueryClient)();

      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "account", changeType: "updated", id: "abc-123", count: 1, userId: "other-user-id", authMethod: "jwt", connectionId: "other-conn" });
      });

      expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({
        queryKey: ["accounts"],
      });
      expect(mockQueryClient.invalidateQueries).toHaveBeenCalledWith({
        queryKey: ["transaction-accounts"],
      });
      expect(bufferToast).toHaveBeenCalledWith("account", "updated", 1, "other-user");
      expect(toast.info).not.toHaveBeenCalled();
    });

    it("invalidates transaction query keys and buffers toast for transaction deleted", async () => {
      const mockQueryClient = vi.mocked(useQueryClient)();

      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "transaction", changeType: "deleted", id: "abc-123", count: 1, userId: "other-user-id", authMethod: "jwt", connectionId: "other-conn" });
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
      expect(bufferToast).toHaveBeenCalledWith("transaction", "deleted", 1, "other-user");
      expect(toast.info).not.toHaveBeenCalled();
    });

    it("buffers toast with display name for receipt-item", async () => {
      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "receipt-item", changeType: "created", id: "abc", count: 1, userId: "other-user-id", authMethod: "jwt", connectionId: "other-conn" });
      });

      expect(bufferToast).toHaveBeenCalledWith("receipt item", "created", 1, "other-user");
      expect(toast.info).not.toHaveBeenCalled();
    });

    it("buffers toast with display name for item-template", async () => {
      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "item-template", changeType: "updated", id: "abc", count: 1, userId: "other-user-id", authMethod: "jwt", connectionId: "other-conn" });
      });

      expect(bufferToast).toHaveBeenCalledWith("item template", "updated", 1, "other-user");
      expect(toast.info).not.toHaveBeenCalled();
    });

    it("suppresses toast when notification.connectionId matches own connectionId", async () => {
      const mockQueryClient = vi.mocked(useQueryClient)();

      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "receipt", changeType: "created", id: "abc", count: 1, userId: "current-user-id", authMethod: "jwt", connectionId: "mock-conn-id" });
      });

      // Queries still invalidated
      expect(mockQueryClient.invalidateQueries).toHaveBeenCalled();
      // But no toast
      expect(bufferToast).not.toHaveBeenCalled();
    });

    it("calls bufferToast with api-key origin when authMethod=apikey and userId matches", async () => {
      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "receipt", changeType: "updated", id: "abc", count: 1, userId: "current-user-id", authMethod: "apikey", connectionId: null });
      });

      expect(bufferToast).toHaveBeenCalledWith("receipt", "updated", 1, "api-key");
    });

    it("calls bufferToast with other-session origin when same userId, different connectionId", async () => {
      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "receipt", changeType: "created", id: "abc", count: 1, userId: "current-user-id", authMethod: "jwt", connectionId: "different-conn" });
      });

      expect(bufferToast).toHaveBeenCalledWith("receipt", "created", 1, "other-session");
    });

    it("calls bufferToast with other-user origin when different userId", async () => {
      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "receipt", changeType: "deleted", id: "abc", count: 1, userId: "someone-else", authMethod: "jwt", connectionId: "other-conn" });
      });

      expect(bufferToast).toHaveBeenCalledWith("receipt", "deleted", 1, "other-user");
    });

    it("classifies as other-user when parseJwtPayload returns null", async () => {
      vi.mocked(parseJwtPayload).mockReturnValueOnce(null);

      await renderEnabled();

      const handler = getOnHandler("EntityChanged");
      expect(handler).toBeDefined();

      act(() => {
        handler!({ entityType: "receipt", changeType: "created", id: "abc", count: 1, userId: "current-user-id", authMethod: "jwt", connectionId: "different-conn" });
      });

      expect(bufferToast).toHaveBeenCalledWith("receipt", "created", 1, "other-user");
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

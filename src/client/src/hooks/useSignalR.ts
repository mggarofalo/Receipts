import { useEffect, useRef, useCallback } from "react";
import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import type { HubConnection } from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { getAccessToken } from "@/lib/auth";

const HUB_URL = "/hubs/receipts";

const EVENT_QUERY_MAP: Record<string, string[]> = {
  AccountCreated: ["accounts"],
  AccountUpdated: ["accounts"],
  AccountDeleted: ["accounts"],
  ReceiptCreated: ["receipts"],
  ReceiptUpdated: ["receipts"],
  ReceiptDeleted: ["receipts"],
  ReceiptItemCreated: ["receiptItems"],
  ReceiptItemUpdated: ["receiptItems"],
  ReceiptItemDeleted: ["receiptItems"],
  TransactionCreated: ["transactions"],
  TransactionUpdated: ["transactions"],
  TransactionDeleted: ["transactions"],
};

let connectionInstance: HubConnection | null = null;
let connectionState: HubConnectionState = HubConnectionState.Disconnected;
const stateListeners = new Set<(state: HubConnectionState) => void>();

function notifyStateListeners() {
  stateListeners.forEach((l) => l(connectionState));
}

export function useSignalRState(): HubConnectionState {
  const [, rerender] = [null, useRef(0)];
  void rerender; // suppress unused warning

  useEffect(() => {
    const listener = () => {
      // trigger re-render
    };
    stateListeners.add(listener);
    return () => {
      stateListeners.delete(listener);
    };
  }, []);

  return connectionState;
}

export function useSignalR() {
  const queryClient = useQueryClient();
  const connectionRef = useRef<HubConnection | null>(null);

  const handleEvent = useCallback(
    (event: string) => {
      const queryKeys = EVENT_QUERY_MAP[event];
      if (queryKeys) {
        queryKeys.forEach((key) => {
          queryClient.invalidateQueries({ queryKey: [key] });
        });
        if (import.meta.env.DEV) {
          console.debug(`[SignalR] Invalidated queries for event: ${event}`);
        }
      }
    },
    [queryClient],
  );

  const start = useCallback(async () => {
    if (
      connectionRef.current?.state === HubConnectionState.Connected ||
      connectionRef.current?.state === HubConnectionState.Connecting
    ) {
      return;
    }

    const token = getAccessToken();
    if (!token) return;

    const connection = new HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => getAccessToken() ?? "",
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          const delays = [0, 2000, 10000, 30000];
          return delays[retryContext.previousRetryCount] ?? 30000;
        },
      })
      .configureLogging(import.meta.env.DEV ? LogLevel.Debug : LogLevel.Error)
      .build();

    // Register event handlers
    Object.keys(EVENT_QUERY_MAP).forEach((event) => {
      connection.on(event, () => handleEvent(event));
    });

    connection.onreconnecting(() => {
      connectionState = HubConnectionState.Reconnecting;
      notifyStateListeners();
    });

    connection.onreconnected(() => {
      connectionState = HubConnectionState.Connected;
      notifyStateListeners();
      toast.success("Real-time updates reconnected");
    });

    connection.onclose(() => {
      connectionState = HubConnectionState.Disconnected;
      notifyStateListeners();
    });

    try {
      await connection.start();
      connectionState = HubConnectionState.Connected;
      notifyStateListeners();
      connectionInstance = connection;
      connectionRef.current = connection;
      if (import.meta.env.DEV) {
        console.debug("[SignalR] Connected");
      }
    } catch (err) {
      connectionState = HubConnectionState.Disconnected;
      notifyStateListeners();
      if (import.meta.env.DEV) {
        console.debug("[SignalR] Connection failed:", err);
      }
    }
  }, [handleEvent]);

  const stop = useCallback(async () => {
    if (connectionRef.current) {
      await connectionRef.current.stop();
      connectionRef.current = null;
      connectionInstance = null;
      connectionState = HubConnectionState.Disconnected;
      notifyStateListeners();
    }
  }, []);

  return { start, stop };
}

export { connectionInstance, connectionState, stateListeners };

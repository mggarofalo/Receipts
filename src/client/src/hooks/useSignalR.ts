import { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { getAccessToken } from "@/lib/auth";
import { bufferToast } from "@/lib/signalr-toast-buffer";

export type SignalRConnectionState =
  | "connected"
  | "disconnected"
  | "reconnecting";

interface EntityChangeNotification {
  entityType: string;
  changeType: string;
  id: string | null;
  count?: number;
}

const queryKeyMap: Record<string, string[][]> = {
  receipt: [["receipts"], ["receipts-with-items"], ["trips"]],
  "receipt-item": [["receipt-items"], ["receipts-with-items"], ["trips"]],
  transaction: [
    ["transactions"],
    ["receipts-with-items"],
    ["trips"],
    ["transaction-accounts"],
  ],
  adjustment: [["adjustments"], ["receipts-with-items"], ["trips"]],
  account: [["accounts"], ["transaction-accounts"]],
  category: [["categories"]],
  subcategory: [["subcategories"]],
  "item-template": [["itemTemplates"]],
};

const displayNameMap: Record<string, string> = {
  receipt: "receipt",
  "receipt-item": "receipt item",
  transaction: "transaction",
  adjustment: "adjustment",
  account: "account",
  category: "category",
  subcategory: "subcategory",
  "item-template": "item template",
};

export function useSignalR(enabled: boolean) {
  const queryClient = useQueryClient();
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const [connectionState, setConnectionState] =
    useState<SignalRConnectionState>("disconnected");

  useEffect(() => {
    if (!enabled) {
      return;
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/entities", {
        accessTokenFactory: () => getAccessToken() ?? "",
      })
      .withAutomaticReconnect()
      .configureLogging(
        import.meta.env.DEV ? signalR.LogLevel.Debug : signalR.LogLevel.None,
      )
      .build();

    connection.onreconnecting(() => {
      setConnectionState("reconnecting");
      if (import.meta.env.DEV) {
        console.debug("[SignalR] Reconnecting...");
      }
    });

    connection.onreconnected(() => {
      setConnectionState("connected");
      if (import.meta.env.DEV) {
        console.debug("[SignalR] Reconnected.");
      }
    });

    connection.onclose(() => {
      setConnectionState("disconnected");
      if (import.meta.env.DEV) {
        console.debug("[SignalR] Connection closed.");
      }
    });

    connection.on(
      "EntityChanged",
      (notification: EntityChangeNotification) => {
        if (import.meta.env.DEV) {
          console.debug("[SignalR] EntityChanged", notification);
        }

        const keys = queryKeyMap[notification.entityType];
        if (keys) {
          for (const queryKey of keys) {
            queryClient.invalidateQueries({ queryKey });
          }
        }

        const displayName =
          displayNameMap[notification.entityType] ?? notification.entityType;
        bufferToast(displayName, notification.changeType, notification.count ?? 1);
      },
    );

    connectionRef.current = connection;

    connection
      .start()
      .then(() => {
        setConnectionState("connected");
        if (import.meta.env.DEV) {
          console.debug("[SignalR] Connected to /entities hub.");
        }
      })
      .catch((err: unknown) => {
        if (import.meta.env.DEV) {
          console.debug("[SignalR] Connection error:", err);
        }
        setConnectionState("disconnected");
      });

    return () => {
      connectionRef.current = null;
      connection.stop();
    };
  }, [enabled, queryClient]);

  return { connectionState };
}

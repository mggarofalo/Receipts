import { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { getAccessToken } from "@/lib/auth";

export type SignalRConnectionState =
  | "connected"
  | "disconnected"
  | "reconnecting";

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
      .withUrl("/hubs/receipts", {
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

    connection.on("ReceiptCreated", (receipt) => {
      if (import.meta.env.DEV) {
        console.debug("[SignalR] ReceiptCreated", receipt);
      }
      queryClient.invalidateQueries({ queryKey: ["receipts"] });
      toast.info("A receipt was created by another user");
    });

    connection.on("ReceiptUpdated", (receipt) => {
      if (import.meta.env.DEV) {
        console.debug("[SignalR] ReceiptUpdated", receipt);
      }
      queryClient.invalidateQueries({ queryKey: ["receipts"] });
      toast.info("A receipt was updated by another user");
    });

    connection.on("ReceiptDeleted", (id: string) => {
      if (import.meta.env.DEV) {
        console.debug("[SignalR] ReceiptDeleted", id);
      }
      queryClient.invalidateQueries({ queryKey: ["receipts"] });
      toast.info("A receipt was deleted by another user");
    });

    connectionRef.current = connection;

    connection
      .start()
      .then(() => {
        setConnectionState("connected");
        if (import.meta.env.DEV) {
          console.debug("[SignalR] Connected to /receipts hub.");
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

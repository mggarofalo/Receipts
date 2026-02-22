import { useState, useEffect } from "react";
import { HubConnectionState } from "@microsoft/signalr";
import { connectionState, stateListeners } from "@/hooks/useSignalR";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";

function getStatusConfig(state: HubConnectionState) {
  switch (state) {
    case HubConnectionState.Connected:
      return {
        color: "bg-green-500",
        label: "Live updates connected",
        pulse: false,
      };
    case HubConnectionState.Connecting:
    case HubConnectionState.Reconnecting:
      return {
        color: "bg-yellow-500",
        label: "Reconnecting...",
        pulse: true,
      };
    default:
      return {
        color: "bg-red-500",
        label: "Live updates disconnected",
        pulse: false,
      };
  }
}

export function ConnectionStatus() {
  const [state, setState] = useState<HubConnectionState>(connectionState);

  useEffect(() => {
    const listener = (newState: HubConnectionState) => setState(newState);
    stateListeners.add(listener);
    return () => {
      stateListeners.delete(listener);
    };
  }, []);

  const { color, label, pulse } = getStatusConfig(state);

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <div className="flex items-center gap-1.5" aria-label={label}>
          <span
            className={`h-2 w-2 rounded-full ${color} ${pulse ? "animate-pulse" : ""}`}
            aria-hidden="true"
          />
          <span className="sr-only">{label}</span>
        </div>
      </TooltipTrigger>
      <TooltipContent side="bottom">
        <p>{label}</p>
      </TooltipContent>
    </Tooltip>
  );
}

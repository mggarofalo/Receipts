import { useMemo } from "react";
import { locationHistory } from "@/lib/location-history";
import { useFieldHistory } from "./useFieldHistory";

export function useLocationHistory() {
  const { entries: locations, options, add, clear } =
    useFieldHistory(locationHistory);

  return useMemo(
    () => ({ locations, options, add, clear }),
    [locations, options, add, clear],
  );
}

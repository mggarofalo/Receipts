import { useMemo } from "react";
import { createFieldHistory } from "@/lib/field-history";
import { useFieldHistory } from "./useFieldHistory";

const locationFieldHistory = createFieldHistory("receipts:location-history", 50);

export function useLocationHistory() {
  const { entries: locations, options, add, clear } =
    useFieldHistory(locationFieldHistory);

  return useMemo(
    () => ({ locations, options, add, clear }),
    [locations, options, add, clear],
  );
}

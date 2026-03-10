import { useState, useCallback, useMemo } from "react";
import {
  getLocationHistory,
  addLocation,
  clearLocationHistory,
} from "@/lib/location-history";
import type { ComboboxOption } from "@/components/ui/combobox";

export function useLocationHistory() {
  const [locations, setLocations] = useState<string[]>(getLocationHistory);

  const add = useCallback((location: string) => {
    addLocation(location);
    setLocations(getLocationHistory());
  }, []);

  const clear = useCallback(() => {
    clearLocationHistory();
    setLocations([]);
  }, []);

  const options: ComboboxOption[] = useMemo(
    () => locations.map((loc) => ({ value: loc, label: loc })),
    [locations],
  );

  return useMemo(
    () => ({ locations, options, add, clear }),
    [locations, options, add, clear],
  );
}

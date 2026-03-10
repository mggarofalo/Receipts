import { useState, useCallback, useMemo } from "react";
import type { FilterPreset } from "@/lib/search";
import {
  getSavedFilters,
  saveFilterPreset,
  deleteFilterPreset,
} from "@/lib/search";

export function useSavedFilters(entityType: string) {
  const [filters, setFilters] = useState<FilterPreset[]>(() =>
    getSavedFilters(entityType),
  );

  const save = useCallback(
    (preset: FilterPreset) => {
      saveFilterPreset(preset);
      setFilters(getSavedFilters(entityType));
    },
    [entityType],
  );

  const remove = useCallback(
    (id: string) => {
      deleteFilterPreset(id);
      setFilters(getSavedFilters(entityType));
    },
    [entityType],
  );

  return useMemo(() => ({ filters, save, remove }), [filters, save, remove]);
}

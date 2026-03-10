import { useState, useCallback, useMemo } from "react";
import type { FieldHistory } from "@/lib/field-history";
import type { ComboboxOption } from "@/components/ui/combobox";

/**
 * React hook that wraps a `FieldHistory` instance with component state.
 *
 * Returns the current entries as both a raw `string[]` and as
 * `ComboboxOption[]` ready for the `<Combobox>` component, plus `add` and
 * `clear` callbacks.
 *
 * All returned references are stable (wrapped in `useCallback`/`useMemo`)
 * to avoid infinite render loops when placed in dependency arrays.
 */
export function useFieldHistory(fieldHistory: FieldHistory) {
  const [entries, setEntries] = useState<string[]>(fieldHistory.getHistory);

  const add = useCallback(
    (value: string) => {
      fieldHistory.addEntry(value);
      setEntries(fieldHistory.getHistory());
    },
    [fieldHistory],
  );

  const clear = useCallback(() => {
    fieldHistory.clearHistory();
    setEntries([]);
  }, [fieldHistory]);

  const options: ComboboxOption[] = useMemo(
    () => entries.map((entry) => ({ value: entry, label: entry })),
    [entries],
  );

  return useMemo(
    () => ({ entries, options, add, clear }),
    [entries, options, add, clear],
  );
}

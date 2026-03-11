import { useState, useCallback, useMemo } from "react";
import {
  getSearchHistory,
  addSearchHistoryEntry,
  clearSearchHistory,
} from "@/lib/search";

export function useSearchHistory() {
  const [history, setHistory] = useState<string[]>(getSearchHistory);

  const addEntry = useCallback((term: string) => {
    addSearchHistoryEntry(term);
    setHistory(getSearchHistory());
  }, []);

  const clearAll = useCallback(() => {
    clearSearchHistory();
    setHistory([]);
  }, []);

  return useMemo(
    () => ({ history, addEntry, clearAll }),
    [history, addEntry, clearAll],
  );
}

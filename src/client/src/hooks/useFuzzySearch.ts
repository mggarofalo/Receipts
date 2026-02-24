import { useState, useMemo, useEffect, useRef } from "react";
import type { FuseSearchConfig, SearchResult } from "@/lib/search";
import { createFuseInstance, addSearchHistoryEntry } from "@/lib/search";

interface UseFuzzySearchOptions<T> {
  data: T[];
  config: FuseSearchConfig<T>;
  debounceMs?: number;
}

interface UseFuzzySearchReturn<T> {
  search: string;
  setSearch: (value: string) => void;
  results: SearchResult<T>[];
  totalCount: number;
  isSearching: boolean;
  clearSearch: () => void;
}

export function useFuzzySearch<T>({
  data,
  config,
  debounceMs = 150,
}: UseFuzzySearchOptions<T>): UseFuzzySearchReturn<T> {
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const timerRef = useRef<ReturnType<typeof setTimeout>>(null);

  useEffect(() => {
    if (timerRef.current) clearTimeout(timerRef.current);
    timerRef.current = setTimeout(() => {
      setDebouncedSearch(search);
    }, debounceMs);
    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
    };
  }, [search, debounceMs]);

  useEffect(() => {
    if (debouncedSearch.length >= 2) {
      addSearchHistoryEntry(debouncedSearch);
    }
  }, [debouncedSearch]);

  const fuse = useMemo(() => createFuseInstance(data, config), [data, config]);

  const results: SearchResult<T>[] = useMemo(() => {
    if (!debouncedSearch.trim()) {
      return data.map((item) => ({ item, refIndex: 0 }));
    }
    return fuse.search(debouncedSearch);
  }, [fuse, debouncedSearch, data]);

  const isSearching = search !== debouncedSearch;

  function clearSearch() {
    setSearch("");
    setDebouncedSearch("");
  }

  return {
    search,
    setSearch,
    results,
    totalCount: data.length,
    isSearching,
    clearSearch,
  };
}

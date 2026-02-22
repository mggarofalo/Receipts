import { useState, useMemo, useCallback } from "react";
import Fuse from "fuse.js";
import type { IFuseOptions } from "fuse.js";

const MAX_RECENT = 5;

function getStorageKey(scope: string) {
  return `receipts:search-history:${scope}`;
}

function loadRecentSearches(scope: string): string[] {
  try {
    const raw = localStorage.getItem(getStorageKey(scope));
    return raw ? (JSON.parse(raw) as string[]) : [];
  } catch {
    return [];
  }
}

function saveRecentSearches(scope: string, searches: string[]) {
  try {
    localStorage.setItem(getStorageKey(scope), JSON.stringify(searches));
  } catch {
    // ignore storage errors
  }
}

export interface UseSearchOptions<T> {
  items: T[];
  fuseOptions: IFuseOptions<T>;
  scope: string;
}

export function useSearch<T>({ items, fuseOptions, scope }: UseSearchOptions<T>) {
  const [query, setQueryState] = useState("");
  const [recentSearches, setRecentSearches] = useState<string[]>(() =>
    loadRecentSearches(scope),
  );

  const fuse = useMemo(
    () => new Fuse(items, { threshold: 0.35, includeMatches: true, ...fuseOptions }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [items],
  );

  const results = useMemo(() => {
    if (!query.trim()) return items;
    return fuse.search(query).map((r) => r.item);
  }, [fuse, items, query]);

  const setQuery = useCallback(
    (q: string) => {
      setQueryState(q);
      if (q.trim() && q.trim().length > 1) {
        setRecentSearches((prev) => {
          const next = [q, ...prev.filter((s) => s !== q)].slice(0, MAX_RECENT);
          saveRecentSearches(scope, next);
          return next;
        });
      }
    },
    [scope],
  );

  const clearQuery = useCallback(() => setQueryState(""), []);

  return { query, setQuery, clearQuery, results, recentSearches };
}

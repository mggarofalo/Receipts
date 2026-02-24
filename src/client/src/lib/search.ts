import Fuse, { type IFuseOptions, type FuseResult } from "fuse.js";

export interface FuseSearchConfig<T> {
  keys: Array<{ name: keyof T & string; weight: number }>;
  threshold?: number;
}

export type SearchResult<T> = FuseResult<T>;

export interface FilterPreset {
  id: string;
  name: string;
  entityType: string;
  values: Record<string, unknown>;
  createdAt: string;
}

const SEARCH_HISTORY_KEY = "receipts:search-history";
const SAVED_FILTERS_KEY = "receipts:saved-filters";
const MAX_HISTORY_ITEMS = 20;

export function createFuseInstance<T>(
  data: T[],
  config: FuseSearchConfig<T>,
): Fuse<T> {
  const options: IFuseOptions<T> = {
    keys: config.keys.map((k) => ({ name: k.name, weight: k.weight })),
    threshold: config.threshold ?? 0.3,
    ignoreLocation: true,
    includeMatches: true,
  };
  return new Fuse(data, options);
}

export function getSearchHistory(): string[] {
  try {
    const raw = localStorage.getItem(SEARCH_HISTORY_KEY);
    return raw ? (JSON.parse(raw) as string[]) : [];
  } catch {
    return [];
  }
}

export function addSearchHistoryEntry(term: string): void {
  const history = getSearchHistory().filter((h) => h !== term);
  history.unshift(term);
  if (history.length > MAX_HISTORY_ITEMS) {
    history.length = MAX_HISTORY_ITEMS;
  }
  localStorage.setItem(SEARCH_HISTORY_KEY, JSON.stringify(history));
}

export function clearSearchHistory(): void {
  localStorage.removeItem(SEARCH_HISTORY_KEY);
}

export function getSavedFilters(entityType?: string): FilterPreset[] {
  try {
    const raw = localStorage.getItem(SAVED_FILTERS_KEY);
    const all: FilterPreset[] = raw ? (JSON.parse(raw) as FilterPreset[]) : [];
    return entityType ? all.filter((f) => f.entityType === entityType) : all;
  } catch {
    return [];
  }
}

export function saveFilterPreset(preset: FilterPreset): void {
  const all = getSavedFilters();
  const idx = all.findIndex((f) => f.id === preset.id);
  if (idx >= 0) {
    all[idx] = preset;
  } else {
    all.push(preset);
  }
  localStorage.setItem(SAVED_FILTERS_KEY, JSON.stringify(all));
}

export function deleteFilterPreset(id: string): void {
  const all = getSavedFilters().filter((f) => f.id !== id);
  localStorage.setItem(SAVED_FILTERS_KEY, JSON.stringify(all));
}

export interface FilterDefinition {
  key: string;
  type: "select" | "dateRange" | "numberRange" | "boolean";
  field: string;
}

export function applyFilters<T>(
  items: T[],
  filters: FilterDefinition[],
  values: Record<string, unknown>,
): T[] {
  return items.filter((item) => {
    for (const filter of filters) {
      const val = values[filter.key];
      const itemVal = (item as Record<string, unknown>)[filter.field];

      if (filter.type === "select") {
        if (val && val !== "all" && itemVal !== val) return false;
      }

      if (filter.type === "boolean") {
        if (val && val !== "all") {
          const expected = val === "true";
          if (itemVal !== expected) return false;
        }
      }

      if (filter.type === "dateRange") {
        const range = val as { from?: string; to?: string } | undefined;
        if (range?.from && typeof itemVal === "string" && itemVal < range.from)
          return false;
        if (range?.to && typeof itemVal === "string" && itemVal > range.to)
          return false;
      }

      if (filter.type === "numberRange") {
        const range = val as { min?: number; max?: number } | undefined;
        if (
          range?.min !== undefined &&
          typeof itemVal === "number" &&
          itemVal < range.min
        )
          return false;
        if (
          range?.max !== undefined &&
          typeof itemVal === "number" &&
          itemVal > range.max
        )
          return false;
      }
    }
    return true;
  });
}

const DEFAULT_MAX_ENTRIES = 50;

export interface FieldHistory {
  /** Retrieve saved entries from localStorage, sorted by most-recently-used. */
  getHistory(): string[];
  /**
   * Add an entry to the front of the history list.
   * Deduplication is case-insensitive: if "Milk" exists and "milk" is added,
   * the old entry is removed and the new casing is placed at the front.
   */
  addEntry(entry: string): void;
  /** Remove all saved entries. */
  clearHistory(): void;
  /** The localStorage key used by this instance. */
  readonly key: string;
}

/**
 * Factory that creates a localStorage-backed history for a specific form field.
 *
 * Each call returns `{ getHistory, addEntry, clearHistory }` bound to the
 * given `key`, following the same MRU (most-recently-used) pattern as
 * `location-history.ts`.
 */
export function createFieldHistory(
  key: string,
  maxEntries: number = DEFAULT_MAX_ENTRIES,
): FieldHistory {
  function getHistory(): string[] {
    try {
      const raw = localStorage.getItem(key);
      if (!raw) return [];
      const parsed: unknown = JSON.parse(raw);
      return Array.isArray(parsed)
        ? parsed.filter((x): x is string => typeof x === "string")
        : [];
    } catch {
      return [];
    }
  }

  function addEntry(entry: string): void {
    const trimmed = entry.trim();
    if (!trimmed) return;

    const history = getHistory().filter(
      (h) => h.toLowerCase() !== trimmed.toLowerCase(),
    );
    history.unshift(trimmed);
    if (history.length > maxEntries) {
      history.length = maxEntries;
    }
    try {
      localStorage.setItem(key, JSON.stringify(history));
    } catch {
      // Ignore storage errors (full, disabled, etc.)
    }
  }

  function clearHistory(): void {
    localStorage.removeItem(key);
  }

  return { getHistory, addEntry, clearHistory, key };
}

// Pre-built field history instances for high-repetition form fields
export const itemDescriptionHistory = createFieldHistory(
  "receipts:item-description-history",
);

export const adjustmentDescriptionHistory = createFieldHistory(
  "receipts:adjustment-description-history",
);

export const subcategoryNameHistory = createFieldHistory(
  "receipts:subcategory-name-history",
);

export const itemCodeHistory = createFieldHistory(
  "receipts:item-code-history",
);

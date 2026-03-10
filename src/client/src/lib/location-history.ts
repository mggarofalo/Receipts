const LOCATION_HISTORY_KEY = "receipts:location-history";
const MAX_LOCATIONS = 50;

/**
 * Retrieve saved locations from localStorage, sorted by most-recently-used.
 */
export function getLocationHistory(): string[] {
  try {
    const raw = localStorage.getItem(LOCATION_HISTORY_KEY);
    return raw ? (JSON.parse(raw) as string[]) : [];
  } catch {
    return [];
  }
}

/**
 * Add a location to the front of the history list.
 * Deduplication is case-insensitive: if "Walmart" exists and "walmart" is added,
 * the old entry is removed and the new casing is placed at the front.
 */
export function addLocation(location: string): void {
  const trimmed = location.trim();
  if (!trimmed) return;

  const history = getLocationHistory().filter(
    (h) => h.toLowerCase() !== trimmed.toLowerCase(),
  );
  history.unshift(trimmed);
  if (history.length > MAX_LOCATIONS) {
    history.length = MAX_LOCATIONS;
  }
  try {
    localStorage.setItem(LOCATION_HISTORY_KEY, JSON.stringify(history));
  } catch {
    // Ignore storage errors (full, disabled, etc.)
  }
}

/**
 * Remove all saved locations.
 */
export function clearLocationHistory(): void {
  localStorage.removeItem(LOCATION_HISTORY_KEY);
}

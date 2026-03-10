import { createFieldHistory } from "./field-history";

const locationHistory = createFieldHistory("receipts:location-history", 50);

/**
 * Retrieve saved locations from localStorage, sorted by most-recently-used.
 */
export const getLocationHistory = locationHistory.getHistory;

/**
 * Add a location to the front of the history list.
 * Deduplication is case-insensitive: if "Walmart" exists and "walmart" is added,
 * the old entry is removed and the new casing is placed at the front.
 */
export const addLocation = locationHistory.addEntry;

/**
 * Remove all saved locations.
 */
export const clearLocationHistory = locationHistory.clearHistory;

const PAGE_SIZE_KEY = "table-page-size";
const VALID_PAGE_SIZES = [10, 25, 50, 100];

export function getPersistedPageSize(): number | null {
  try {
    const raw = localStorage.getItem(PAGE_SIZE_KEY);
    if (raw === null) return null;
    const parsed = Number(raw);
    return VALID_PAGE_SIZES.includes(parsed) ? parsed : null;
  } catch {
    return null;
  }
}

export function persistPageSize(size: number): void {
  if (!VALID_PAGE_SIZES.includes(size)) return;
  try {
    localStorage.setItem(PAGE_SIZE_KEY, String(size));
  } catch {
    // Ignore storage errors (full, disabled, etc.)
  }
}

import { toast } from "sonner";

const FLUSH_DELAY_MS = 5_000;

interface BufferEntry {
  count: number;
}

const buffer = new Map<string, BufferEntry>();
let flushTimer: ReturnType<typeof setTimeout> | null = null;

const pluralExceptions: Record<string, string> = {
  category: "categories",
  subcategory: "subcategories",
};

function pluralize(name: string, count: number): string {
  if (count === 1) return name;
  return pluralExceptions[name] ?? `${name}s`;
}

function makeKey(entityType: string, changeType: string): string {
  return `${entityType}::${changeType}`;
}

function flush(): void {
  flushTimer = null;

  for (const [key, entry] of buffer) {
    const [entityType, changeType] = key.split("::");
    const displayName = pluralize(entityType, entry.count);
    const action =
      changeType === "created"
        ? "created"
        : changeType === "deleted"
          ? "deleted"
          : "updated";

    if (entry.count === 1) {
      toast.info(`A ${displayName} was ${action} by another user`);
    } else {
      toast.info(
        `${entry.count} ${displayName} were ${action} by another user`,
      );
    }
  }

  buffer.clear();
}

export function bufferToast(
  entityType: string,
  changeType: string,
  count: number,
): void {
  const key = makeKey(entityType, changeType);
  const existing = buffer.get(key);

  if (existing) {
    existing.count += count;
  } else {
    buffer.set(key, { count });
  }

  if (flushTimer === null) {
    flushTimer = setTimeout(flush, FLUSH_DELAY_MS);
  }
}

/** Exposed for testing — clears pending state without flushing. */
export function _resetForTesting(): void {
  if (flushTimer !== null) {
    clearTimeout(flushTimer);
    flushTimer = null;
  }
  buffer.clear();
}

/** Exposed for testing — triggers an immediate flush. */
export function _flushForTesting(): void {
  if (flushTimer !== null) {
    clearTimeout(flushTimer);
    flushTimer = null;
  }
  flush();
}

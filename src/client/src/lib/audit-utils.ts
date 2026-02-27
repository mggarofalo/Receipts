import type { components } from "@/generated/api";

export type AuditLog = components["schemas"]["AuditLogResponse"];
export type AuthAuditLog = components["schemas"]["AuthAuditLogResponse"];

export interface FieldChange {
  field: string;
  oldValue: string | null;
  newValue: string | null;
}

export function parseChanges(changesJson: string): FieldChange[] {
  try {
    const parsed: unknown = JSON.parse(changesJson);
    if (!Array.isArray(parsed)) return [];
    return parsed.filter(
      (c): c is FieldChange =>
        typeof c === "object" &&
        c !== null &&
        "field" in c &&
        typeof c.field === "string",
    );
  } catch {
    return [];
  }
}

export const ENTITY_TYPES = [
  "Account",
  "Receipt",
  "ReceiptItem",
  "Transaction",
] as const;

export const ENTITY_TYPE_LABELS: Record<string, string> = {
  Account: "Account",
  Receipt: "Receipt",
  ReceiptItem: "Receipt Item",
  Transaction: "Transaction",
};

export const ACTION_TYPES = [
  "Created",
  "Updated",
  "Deleted",
  "Restored",
] as const;

export const AUTH_EVENT_TYPES = [
  "Login",
  "Logout",
  "TokenRefresh",
  "ApiKeyAuth",
] as const;

export function actionBadgeVariant(
  action: string,
): "default" | "secondary" | "destructive" | "outline" {
  switch (action) {
    case "Created":
      return "default";
    case "Updated":
      return "secondary";
    case "Deleted":
      return "destructive";
    case "Restored":
      return "outline";
    default:
      return "secondary";
  }
}

export function formatAuditTimestamp(iso: string): string {
  return new Date(iso).toLocaleString();
}

export function relativeTime(iso: string): string {
  const now = Date.now();
  const then = new Date(iso).getTime();
  const diffSeconds = Math.floor((now - then) / 1000);

  if (diffSeconds < 60) return "just now";
  const diffMinutes = Math.floor(diffSeconds / 60);
  if (diffMinutes < 60) return `${diffMinutes}m ago`;
  const diffHours = Math.floor(diffMinutes / 60);
  if (diffHours < 24) return `${diffHours}h ago`;
  const diffDays = Math.floor(diffHours / 24);
  if (diffDays < 30) return `${diffDays}d ago`;
  return formatAuditTimestamp(iso);
}

export function truncateId(id: string, length = 8): string {
  return id.length > length ? `${id.slice(0, length)}...` : id;
}

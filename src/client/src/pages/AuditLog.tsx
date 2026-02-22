import { useState, useMemo } from "react";
import { useRecentAuditLogs } from "@/hooks/useAudit";
import type { AuditLog as AuditLogEntry } from "@/lib/audit-utils";
import {
  ENTITY_TYPES,
  ENTITY_TYPE_LABELS,
  ACTION_TYPES,
} from "@/lib/audit-utils";
import { AuditLogTable } from "@/components/AuditLogTable";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

const COUNT_OPTIONS = [50, 100, 200] as const;

function exportToCsv(logs: AuditLogEntry[]) {
  const headers = [
    "Timestamp",
    "Entity Type",
    "Entity ID",
    "Action",
    "Changed By (User)",
    "Changed By (API Key)",
    "IP Address",
    "Changes JSON",
  ];
  const rows = logs.map((log) =>
    [
      log.changedAt,
      log.entityType,
      log.entityId,
      log.action,
      log.changedByUserId ?? "",
      log.changedByApiKeyId ?? "",
      log.ipAddress ?? "",
      `"${log.changesJson.replace(/"/g, '""')}"`,
    ].join(","),
  );
  const csv = [headers.join(","), ...rows].join("\n");
  const blob = new Blob([csv], { type: "text/csv" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = `audit-log-${new Date().toISOString().slice(0, 10)}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}

function AuditLog() {
  const [count, setCount] = useState<number>(50);
  const [search, setSearch] = useState("");
  const [entityTypeFilter, setEntityTypeFilter] = useState("all");
  const [actionFilter, setActionFilter] = useState("all");

  const { data, isLoading } = useRecentAuditLogs(count);

  const filtered = useMemo(() => {
    const logs = (data ?? []) as AuditLogEntry[];
    const term = search.toLowerCase();
    return logs.filter((log) => {
      if (term && !log.entityId.toLowerCase().includes(term)) return false;
      if (entityTypeFilter !== "all" && log.entityType !== entityTypeFilter)
        return false;
      if (actionFilter !== "all" && log.action !== actionFilter) return false;
      return true;
    });
  }, [data, search, entityTypeFilter, actionFilter]);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Audit Log</h1>
        <Button
          variant="outline"
          size="sm"
          onClick={() => exportToCsv(filtered)}
          disabled={filtered.length === 0}
        >
          Export CSV
        </Button>
      </div>

      <div className="flex items-center gap-3 flex-wrap">
        <Input
          placeholder="Search by entity ID..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="max-w-xs"
        />
        <Select value={entityTypeFilter} onValueChange={setEntityTypeFilter}>
          <SelectTrigger className="w-[160px]">
            <SelectValue placeholder="Entity Type" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Types</SelectItem>
            {ENTITY_TYPES.map((t) => (
              <SelectItem key={t} value={t}>
                {ENTITY_TYPE_LABELS[t]}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select value={actionFilter} onValueChange={setActionFilter}>
          <SelectTrigger className="w-[140px]">
            <SelectValue placeholder="Action" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Actions</SelectItem>
            {ACTION_TYPES.map((a) => (
              <SelectItem key={a} value={a}>
                {a}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select
          value={String(count)}
          onValueChange={(v) => setCount(Number(v))}
        >
          <SelectTrigger className="w-[100px]">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {COUNT_OPTIONS.map((c) => (
              <SelectItem key={c} value={String(c)}>
                {c} rows
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <AuditLogTable logs={filtered} isLoading={isLoading} />
    </div>
  );
}

export default AuditLog;

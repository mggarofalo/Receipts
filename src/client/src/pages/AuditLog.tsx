import { useEffect } from "react";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useRecentAuditLogs } from "@/hooks/useAudit";
import { useServerPagination } from "@/hooks/useServerPagination";
import { useServerSort } from "@/hooks/useServerSort";
import type { AuditLog as AuditLogEntry } from "@/lib/audit-utils";
import { AuditLogTable } from "@/components/AuditLogTable";
import { Pagination } from "@/components/Pagination";
import { Button } from "@/components/ui/button";

function csvField(value: string): string {
  if (value.includes(",") || value.includes('"') || value.includes("\n") || value.includes("\r")) {
    return `"${value.replace(/"/g, '""')}"`;
  }
  return value;
}

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
      log.changesJson,
    ]
      .map(csvField)
      .join(","),
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
  usePageTitle("Audit Log");
  const { sortBy, sortDirection, toggleSort } = useServerSort({ defaultSortBy: "changedAt", defaultSortDirection: "desc" });
  const { offset, limit, currentPage, pageSize, totalPages, setPage, setPageSize, resetPage } = useServerPagination();

  const { data: response, isLoading } = useRecentAuditLogs(offset, limit, sortBy, sortDirection);
  const serverTotal = response?.total ?? 0;
  const logs = (response?.data ?? []) as AuditLogEntry[];

  useEffect(() => { resetPage(); }, [sortBy, sortDirection, resetPage]);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Audit Log</h1>
        <Button
          variant="outline"
          size="sm"
          onClick={() => exportToCsv(logs)}
          disabled={logs.length === 0}
        >
          Export CSV
        </Button>
      </div>

      <AuditLogTable
        logs={logs}
        isLoading={isLoading}
        sortBy={sortBy}
        sortDirection={sortDirection}
        onToggleSort={toggleSort}
      />

      <Pagination
        currentPage={currentPage}
        totalItems={serverTotal}
        pageSize={pageSize}
        totalPages={totalPages(serverTotal)}
        onPageChange={(page) => setPage(page, serverTotal)}
        onPageSizeChange={setPageSize}
      />
    </div>
  );
}

export default AuditLog;

import { useState, useCallback } from "react";
import { format } from "date-fns";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useRecentAuditLogs } from "@/hooks/useAudit";
import { useServerPagination } from "@/hooks/useServerPagination";
import { useServerSort } from "@/hooks/useServerSort";
import type { AuditLog as AuditLogEntry } from "@/lib/audit-utils";
import {
  ENTITY_TYPES,
  ENTITY_TYPE_LABELS,
  ACTION_TYPES,
  ACTION_FILTER_VALUES,
} from "@/lib/audit-utils";
import { AuditLogTable } from "@/components/AuditLogTable";
import { Pagination } from "@/components/Pagination";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";
import { useDebouncedValue } from "@/hooks/useDebouncedValue";

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

function DateRangePicker({
  from,
  to,
  onFromChange,
  onToChange,
}: {
  from: Date | undefined;
  to: Date | undefined;
  onFromChange: (d: Date | undefined) => void;
  onToChange: (d: Date | undefined) => void;
}) {
  return (
    <div className="flex items-center gap-1">
      <Popover>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            size="sm"
            className={cn(
              "w-[120px] justify-start text-left font-normal",
              !from && "text-muted-foreground",
            )}
          >
            {from ? format(from, "MMM d, yyyy") : "From"}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            mode="single"
            selected={from}
            onSelect={onFromChange}
            disabled={(d) => (to ? d > to : false)}
          />
        </PopoverContent>
      </Popover>
      <span className="text-muted-foreground text-xs">—</span>
      <Popover>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            size="sm"
            className={cn(
              "w-[120px] justify-start text-left font-normal",
              !to && "text-muted-foreground",
            )}
          >
            {to ? format(to, "MMM d, yyyy") : "To"}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            mode="single"
            selected={to}
            onSelect={onToChange}
            disabled={(d) => (from ? d < from : false)}
          />
        </PopoverContent>
      </Popover>
      {(from || to) && (
        <Button
          variant="ghost"
          size="sm"
          onClick={() => {
            onFromChange(undefined);
            onToChange(undefined);
          }}
        >
          Clear
        </Button>
      )}
    </div>
  );
}

function AuditLog() {
  usePageTitle("Audit Log");
  const [search, setSearch] = useState("");
  const [entityTypeFilter, setEntityTypeFilter] = useState("all");
  const [actionFilter, setActionFilter] = useState("all");
  const [dateFrom, setDateFrom] = useState<Date | undefined>();
  const [dateTo, setDateTo] = useState<Date | undefined>();

  const debouncedSearch = useDebouncedValue(search, 300);

  const pagination = useServerPagination();
  const { sortBy, sortDirection, toggleSort } = useServerSort({
    defaultSortBy: "changedAt",
    defaultSortDirection: "desc",
  });

  // Reset pagination when filters change
  const handleEntityTypeChange = useCallback(
    (value: string) => {
      setEntityTypeFilter(value);
      pagination.resetPage();
    },
    [pagination],
  );

  const handleActionChange = useCallback(
    (value: string) => {
      setActionFilter(value);
      pagination.resetPage();
    },
    [pagination],
  );

  const handleDateFromChange = useCallback(
    (d: Date | undefined) => {
      setDateFrom(d);
      pagination.resetPage();
    },
    [pagination],
  );

  const handleDateToChange = useCallback(
    (d: Date | undefined) => {
      setDateTo(d);
      pagination.resetPage();
    },
    [pagination],
  );

  const { data, isLoading } = useRecentAuditLogs({
    offset: pagination.offset,
    limit: pagination.limit,
    sortBy,
    sortDirection,
    entityType: entityTypeFilter !== "all" ? entityTypeFilter : null,
    action: actionFilter !== "all" ? actionFilter : null,
    search: debouncedSearch || null,
    dateFrom: dateFrom ? dateFrom.toISOString() : null,
    dateTo: dateTo ? dateTo.toISOString() : null,
  });

  const logs = (data?.data ?? []) as AuditLogEntry[];
  const total = data?.total ?? 0;

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

      <div className="flex items-center gap-3 flex-wrap">
        <Input
          placeholder="Search by entity ID..."
          aria-label="Search audit log by entity ID"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            pagination.resetPage();
          }}
          className="max-w-xs"
        />
        <Select value={entityTypeFilter} onValueChange={handleEntityTypeChange}>
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
        <Select value={actionFilter} onValueChange={handleActionChange}>
          <SelectTrigger className="w-[140px]">
            <SelectValue placeholder="Action" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Actions</SelectItem>
            {ACTION_TYPES.map((a) => (
              <SelectItem key={a} value={ACTION_FILTER_VALUES[a] ?? a}>
                {a}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <DateRangePicker
          from={dateFrom}
          to={dateTo}
          onFromChange={handleDateFromChange}
          onToChange={handleDateToChange}
        />
      </div>

      <AuditLogTable
        logs={logs}
        isLoading={isLoading}
        sortBy={sortBy}
        sortDirection={sortDirection}
        onToggleSort={toggleSort}
      />

      <Pagination
        currentPage={pagination.currentPage}
        totalItems={total}
        pageSize={pagination.pageSize}
        totalPages={pagination.totalPages(total)}
        onPageChange={(page) => pagination.setPage(page, total)}
        onPageSizeChange={pagination.setPageSize}
      />
    </div>
  );
}

export default AuditLog;

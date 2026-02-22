import type { AuditLog } from "@/lib/audit-utils";
import {
  parseChanges,
  actionBadgeVariant,
  formatAuditTimestamp,
  truncateId,
  ENTITY_TYPE_LABELS,
} from "@/lib/audit-utils";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { FieldDiff } from "@/components/FieldDiff";

interface AuditLogTableProps {
  logs: AuditLog[];
  isLoading: boolean;
}

function AuditRow({ log }: { log: AuditLog }) {
  const changes = parseChanges(log.changesJson);
  const hasChanges = changes.length > 0;

  return (
    <Collapsible asChild>
      <>
        <TableRow>
          <TableCell className="text-xs">
            {formatAuditTimestamp(log.changedAt)}
          </TableCell>
          <TableCell>
            {ENTITY_TYPE_LABELS[log.entityType] ?? log.entityType}
          </TableCell>
          <TableCell>
            <Tooltip>
              <TooltipTrigger asChild>
                <span className="font-mono text-xs cursor-default">
                  {truncateId(log.entityId)}
                </span>
              </TooltipTrigger>
              <TooltipContent>{log.entityId}</TooltipContent>
            </Tooltip>
          </TableCell>
          <TableCell>
            <Badge variant={actionBadgeVariant(log.action)}>{log.action}</Badge>
          </TableCell>
          <TableCell>
            {log.changedByUserId ? (
              <Tooltip>
                <TooltipTrigger asChild>
                  <span className="font-mono text-xs cursor-default">
                    {truncateId(log.changedByUserId)}
                  </span>
                </TooltipTrigger>
                <TooltipContent>{log.changedByUserId}</TooltipContent>
              </Tooltip>
            ) : log.changedByApiKeyId ? (
              <Tooltip>
                <TooltipTrigger asChild>
                  <span className="font-mono text-xs cursor-default">
                    API: {truncateId(log.changedByApiKeyId)}
                  </span>
                </TooltipTrigger>
                <TooltipContent>{log.changedByApiKeyId}</TooltipContent>
              </Tooltip>
            ) : (
              <span className="text-muted-foreground">—</span>
            )}
          </TableCell>
          <TableCell className="text-center">
            {hasChanges ? (
              <CollapsibleTrigger className="text-primary hover:underline cursor-pointer">
                {changes.length}
              </CollapsibleTrigger>
            ) : (
              <span className="text-muted-foreground">—</span>
            )}
          </TableCell>
        </TableRow>
        {hasChanges && (
          <CollapsibleContent asChild>
            <tr>
              <td colSpan={6} className="p-0">
                <div className="bg-muted/30 px-6 py-3 border-b">
                  {changes.map((c) => (
                    <FieldDiff
                      key={c.field}
                      fieldName={c.field}
                      oldValue={c.oldValue}
                      newValue={c.newValue}
                    />
                  ))}
                </div>
              </td>
            </tr>
          </CollapsibleContent>
        )}
      </>
    </Collapsible>
  );
}

export function AuditLogTable({ logs, isLoading }: AuditLogTableProps) {
  if (isLoading) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-12 w-full" />
        ))}
      </div>
    );
  }

  if (logs.length === 0) {
    return (
      <div className="py-12 text-center text-muted-foreground">
        No audit log entries found.
      </div>
    );
  }

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Timestamp</TableHead>
            <TableHead>Entity Type</TableHead>
            <TableHead>Entity ID</TableHead>
            <TableHead>Action</TableHead>
            <TableHead>Changed By</TableHead>
            <TableHead className="text-center">Changes</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {logs.map((log) => (
            <AuditRow key={log.id} log={log} />
          ))}
        </TableBody>
      </Table>
    </div>
  );
}

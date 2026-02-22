import { useEntityAuditHistory } from "@/hooks/useAudit";
import {
  parseChanges,
  actionBadgeVariant,
  relativeTime,
  formatAuditTimestamp,
  truncateId,
} from "@/lib/audit-utils";
import type { AuditLog } from "@/lib/audit-utils";
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
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
import { FieldDiff } from "@/components/FieldDiff";

interface ChangeHistoryProps {
  entityType: string;
  entityId: string;
}

function TimelineEntry({ log }: { log: AuditLog }) {
  const changes = parseChanges(log.changesJson);
  const hasChanges = changes.length > 0;

  return (
    <Collapsible>
      <div className="flex items-start gap-3 py-3 border-b last:border-b-0">
        <div className="mt-1 h-2 w-2 rounded-full bg-muted-foreground shrink-0" />
        <div className="flex-1 min-w-0 space-y-1">
          <div className="flex items-center gap-2 flex-wrap">
            <Badge variant={actionBadgeVariant(log.action)}>{log.action}</Badge>
            <Tooltip>
              <TooltipTrigger asChild>
                <span className="text-xs text-muted-foreground cursor-default">
                  {relativeTime(log.changedAt)}
                </span>
              </TooltipTrigger>
              <TooltipContent>
                {formatAuditTimestamp(log.changedAt)}
              </TooltipContent>
            </Tooltip>
          </div>
          <div className="flex items-center gap-3 text-xs text-muted-foreground">
            {log.changedByUserId && (
              <Tooltip>
                <TooltipTrigger asChild>
                  <span className="font-mono cursor-default">
                    User: {truncateId(log.changedByUserId)}
                  </span>
                </TooltipTrigger>
                <TooltipContent>{log.changedByUserId}</TooltipContent>
              </Tooltip>
            )}
            {log.changedByApiKeyId && (
              <Tooltip>
                <TooltipTrigger asChild>
                  <span className="font-mono cursor-default">
                    API Key: {truncateId(log.changedByApiKeyId)}
                  </span>
                </TooltipTrigger>
                <TooltipContent>{log.changedByApiKeyId}</TooltipContent>
              </Tooltip>
            )}
            {log.ipAddress && <span>IP: {log.ipAddress}</span>}
          </div>
          {hasChanges && (
            <CollapsibleTrigger className="text-xs text-primary hover:underline cursor-pointer">
              {changes.length} field change{changes.length !== 1 ? "s" : ""}
            </CollapsibleTrigger>
          )}
          <CollapsibleContent>
            <div className="mt-2 rounded-md border bg-muted/30 p-2">
              {changes.map((c) => (
                <FieldDiff
                  key={c.field}
                  fieldName={c.field}
                  oldValue={c.oldValue}
                  newValue={c.newValue}
                />
              ))}
            </div>
          </CollapsibleContent>
        </div>
      </div>
    </Collapsible>
  );
}

export function ChangeHistory({ entityType, entityId }: ChangeHistoryProps) {
  const { data, isLoading } = useEntityAuditHistory(entityType, entityId);

  if (isLoading) {
    return (
      <div className="space-y-3 p-4">
        {Array.from({ length: 3 }).map((_, i) => (
          <div key={i} className="flex items-start gap-3">
            <Skeleton className="h-2 w-2 rounded-full mt-1" />
            <div className="flex-1 space-y-2">
              <Skeleton className="h-5 w-24" />
              <Skeleton className="h-4 w-48" />
            </div>
          </div>
        ))}
      </div>
    );
  }

  const logs = (data ?? []) as AuditLog[];

  if (logs.length === 0) {
    return (
      <p className="text-sm text-muted-foreground p-4">
        No history available for this entity.
      </p>
    );
  }

  return (
    <ScrollArea className="max-h-[400px]">
      <div className="px-4">
        {logs.map((log) => (
          <TimelineEntry key={log.id} log={log} />
        ))}
      </div>
    </ScrollArea>
  );
}

import type { AuthAuditLog } from "@/lib/audit-utils";
import { formatAuditTimestamp, truncateId } from "@/lib/audit-utils";
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

interface AuthAuditTableProps {
  logs: AuthAuditLog[];
  isLoading: boolean;
  showUsername?: boolean;
}

function eventBadgeVariant(
  eventType: string,
  success?: boolean,
): "default" | "secondary" | "destructive" | "outline" {
  if (success === false) return "destructive";
  switch (eventType) {
    case "Login":
      return "default";
    case "Logout":
      return "secondary";
    case "TokenRefresh":
      return "outline";
    default:
      return "secondary";
  }
}

function AuthAuditRow({
  log,
  showUsername,
}: {
  log: AuthAuditLog;
  showUsername: boolean;
}) {
  const hasDetails =
    !!log.failureReason || !!log.metadataJson || !!log.userAgent;

  return (
    <Collapsible asChild>
      <>
        <TableRow className={log.success === false ? "bg-destructive/5" : ""}>
          <TableCell className="text-xs">
            {formatAuditTimestamp(log.timestamp)}
          </TableCell>
          <TableCell>
            <Badge variant={eventBadgeVariant(log.eventType, log.success)}>
              {log.eventType}
            </Badge>
          </TableCell>
          {showUsername && (
            <TableCell>
              {log.username ?? <span className="text-muted-foreground">—</span>}
            </TableCell>
          )}
          <TableCell>
            {log.success === true ? (
              <Badge variant="outline">Success</Badge>
            ) : log.success === false ? (
              <Badge variant="destructive">Failed</Badge>
            ) : (
              <span className="text-muted-foreground">—</span>
            )}
          </TableCell>
          <TableCell className="text-xs">
            {log.ipAddress ?? <span className="text-muted-foreground">—</span>}
          </TableCell>
          <TableCell>
            {log.userAgent ? (
              <Tooltip>
                <TooltipTrigger asChild>
                  <span className="text-xs cursor-default">
                    {truncateId(log.userAgent, 30)}
                  </span>
                </TooltipTrigger>
                <TooltipContent className="max-w-sm break-all">
                  {log.userAgent}
                </TooltipContent>
              </Tooltip>
            ) : (
              <span className="text-muted-foreground">—</span>
            )}
          </TableCell>
          <TableCell>
            {hasDetails && (
              <CollapsibleTrigger className="text-xs text-primary hover:underline cursor-pointer">
                Details
              </CollapsibleTrigger>
            )}
          </TableCell>
        </TableRow>
        {hasDetails && (
          <CollapsibleContent asChild>
            <tr>
              <td colSpan={showUsername ? 7 : 6} className="p-0">
                <div className="bg-muted/30 px-6 py-3 border-b space-y-1 text-sm">
                  {log.failureReason && (
                    <p>
                      <span className="font-medium text-destructive">
                        Failure reason:
                      </span>{" "}
                      {log.failureReason}
                    </p>
                  )}
                  {log.userAgent && (
                    <p>
                      <span className="font-medium">User Agent:</span>{" "}
                      {log.userAgent}
                    </p>
                  )}
                  {log.metadataJson && (
                    <pre className="mt-1 rounded bg-muted p-2 text-xs overflow-auto">
                      {log.metadataJson}
                    </pre>
                  )}
                </div>
              </td>
            </tr>
          </CollapsibleContent>
        )}
      </>
    </Collapsible>
  );
}

export function AuthAuditTable({
  logs,
  isLoading,
  showUsername = false,
}: AuthAuditTableProps) {
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
        No auth audit entries found.
      </div>
    );
  }

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Timestamp</TableHead>
            <TableHead>Event Type</TableHead>
            {showUsername && <TableHead>Username</TableHead>}
            <TableHead>Result</TableHead>
            <TableHead>IP Address</TableHead>
            <TableHead>User Agent</TableHead>
            <TableHead />
          </TableRow>
        </TableHeader>
        <TableBody>
          {logs.map((log) => (
            <AuthAuditRow key={log.id} log={log} showUsername={showUsername} />
          ))}
        </TableBody>
      </Table>
    </div>
  );
}

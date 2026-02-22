import { useMemo } from "react";
import { useRecentAuditLogs } from "@/hooks/useAudit";
import { useRestoreAccount } from "@/hooks/useAccounts";
import { useRestoreReceipt } from "@/hooks/useReceipts";
import { useRestoreReceiptItem } from "@/hooks/useReceiptItems";
import { useRestoreTransaction } from "@/hooks/useTransactions";
import type { AuditLog } from "@/lib/audit-utils";
import {
  formatAuditTimestamp,
  truncateId,
  ENTITY_TYPE_LABELS,
} from "@/lib/audit-utils";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";

interface DeletedItem {
  entityType: string;
  entityId: string;
  deletedAt: string;
  deletedByUserId: string | null;
  deletedByApiKeyId: string | null;
}

function useDeletedItems() {
  const { data, isLoading } = useRecentAuditLogs(200);

  const deletedItems = useMemo(() => {
    const logs = (data ?? []) as AuditLog[];
    const restored = new Set(
      logs
        .filter((l) => l.action === "Restored")
        .map((l) => `${l.entityType}:${l.entityId}`),
    );

    return logs
      .filter(
        (l) =>
          l.action === "Deleted" &&
          !restored.has(`${l.entityType}:${l.entityId}`),
      )
      .map(
        (l): DeletedItem => ({
          entityType: l.entityType,
          entityId: l.entityId,
          deletedAt: l.changedAt,
          deletedByUserId: l.changedByUserId ?? null,
          deletedByApiKeyId: l.changedByApiKeyId ?? null,
        }),
      );
  }, [data]);

  return { deletedItems, isLoading };
}

function RestoreButton({
  entityType,
  entityId,
}: {
  entityType: string;
  entityId: string;
}) {
  const restoreAccount = useRestoreAccount();
  const restoreReceipt = useRestoreReceipt();
  const restoreReceiptItem = useRestoreReceiptItem();
  const restoreTransaction = useRestoreTransaction();

  const mutations: Record<
    string,
    { mutate: (id: string) => void; isPending: boolean }
  > = {
    Account: restoreAccount,
    Receipt: restoreReceipt,
    ReceiptItem: restoreReceiptItem,
    Transaction: restoreTransaction,
  };

  const mutation = mutations[entityType];
  if (!mutation) return null;

  return (
    <Button
      variant="outline"
      size="sm"
      disabled={mutation.isPending}
      onClick={() => mutation.mutate(entityId)}
    >
      {mutation.isPending ? "Restoring..." : "Restore"}
    </Button>
  );
}

function DeletedItemsTable({ items }: { items: DeletedItem[] }) {
  if (items.length === 0) {
    return (
      <div className="py-12 text-center text-muted-foreground">
        No deleted items found.
      </div>
    );
  }

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Entity Type</TableHead>
            <TableHead>Entity ID</TableHead>
            <TableHead>Deleted At</TableHead>
            <TableHead>Deleted By</TableHead>
            <TableHead className="w-24" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {items.map((item) => (
            <TableRow key={`${item.entityType}:${item.entityId}`}>
              <TableCell>
                {ENTITY_TYPE_LABELS[item.entityType] ?? item.entityType}
              </TableCell>
              <TableCell>
                <Tooltip>
                  <TooltipTrigger asChild>
                    <span className="font-mono text-xs cursor-default">
                      {truncateId(item.entityId)}
                    </span>
                  </TooltipTrigger>
                  <TooltipContent>{item.entityId}</TooltipContent>
                </Tooltip>
              </TableCell>
              <TableCell className="text-xs">
                {formatAuditTimestamp(item.deletedAt)}
              </TableCell>
              <TableCell>
                {item.deletedByUserId ? (
                  <Tooltip>
                    <TooltipTrigger asChild>
                      <span className="font-mono text-xs cursor-default">
                        {truncateId(item.deletedByUserId)}
                      </span>
                    </TooltipTrigger>
                    <TooltipContent>{item.deletedByUserId}</TooltipContent>
                  </Tooltip>
                ) : item.deletedByApiKeyId ? (
                  <Tooltip>
                    <TooltipTrigger asChild>
                      <span className="font-mono text-xs cursor-default">
                        API: {truncateId(item.deletedByApiKeyId)}
                      </span>
                    </TooltipTrigger>
                    <TooltipContent>{item.deletedByApiKeyId}</TooltipContent>
                  </Tooltip>
                ) : (
                  <span className="text-muted-foreground">—</span>
                )}
              </TableCell>
              <TableCell>
                <RestoreButton
                  entityType={item.entityType}
                  entityId={item.entityId}
                />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}

function RecycleBin() {
  const { deletedItems, isLoading } = useDeletedItems();

  const byType = useMemo(() => {
    const map: Record<string, DeletedItem[]> = {};
    for (const item of deletedItems) {
      (map[item.entityType] ??= []).push(item);
    }
    return map;
  }, [deletedItems]);

  if (isLoading) {
    return (
      <div className="space-y-4">
        <h1 className="text-2xl font-semibold">Recycle Bin</h1>
        <div className="space-y-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold">Recycle Bin</h1>

      <Alert>
        <AlertDescription>
          Showing recently deleted items. Older deletions may not appear.
        </AlertDescription>
      </Alert>

      <Tabs defaultValue="all">
        <TabsList>
          <TabsTrigger value="all">All ({deletedItems.length})</TabsTrigger>
          {Object.entries(byType).map(([type, items]) => (
            <TabsTrigger key={type} value={type}>
              {ENTITY_TYPE_LABELS[type] ?? type} ({items.length})
            </TabsTrigger>
          ))}
        </TabsList>

        <TabsContent value="all">
          <DeletedItemsTable items={deletedItems} />
        </TabsContent>

        {Object.entries(byType).map(([type, items]) => (
          <TabsContent key={type} value={type}>
            <DeletedItemsTable items={items} />
          </TabsContent>
        ))}
      </Tabs>
    </div>
  );
}

export default RecycleBin;

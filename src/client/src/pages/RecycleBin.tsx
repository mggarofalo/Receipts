import { useMemo } from "react";
import { useDeletedAccounts, useRestoreAccount } from "@/hooks/useAccounts";
import { useDeletedReceipts, useRestoreReceipt } from "@/hooks/useReceipts";
import {
  useDeletedReceiptItems,
  useRestoreReceiptItem,
} from "@/hooks/useReceiptItems";
import {
  useDeletedTransactions,
  useRestoreTransaction,
} from "@/hooks/useTransactions";
import { truncateId } from "@/lib/audit-utils";
import { Button } from "@/components/ui/button";
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
  entityTypeLabel: string;
  id: string;
  label: string;
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
            <TableHead>ID</TableHead>
            <TableHead>Details</TableHead>
            <TableHead className="w-24" />
          </TableRow>
        </TableHeader>
        <TableBody>
          {items.map((item) => (
            <TableRow key={`${item.entityType}:${item.id}`}>
              <TableCell>{item.entityTypeLabel}</TableCell>
              <TableCell>
                <Tooltip>
                  <TooltipTrigger asChild>
                    <span className="font-mono text-xs cursor-default">
                      {truncateId(item.id)}
                    </span>
                  </TooltipTrigger>
                  <TooltipContent>{item.id}</TooltipContent>
                </Tooltip>
              </TableCell>
              <TableCell className="text-sm text-muted-foreground">
                {item.label}
              </TableCell>
              <TableCell>
                <RestoreButton
                  entityType={item.entityType}
                  entityId={item.id}
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
  const accounts = useDeletedAccounts();
  const receipts = useDeletedReceipts();
  const receiptItems = useDeletedReceiptItems();
  const transactions = useDeletedTransactions();

  const isLoading =
    accounts.isLoading ||
    receipts.isLoading ||
    receiptItems.isLoading ||
    transactions.isLoading;

  const allItems = useMemo(() => {
    const items: DeletedItem[] = [];

    for (const a of accounts.data ?? []) {
      items.push({
        entityType: "Account",
        entityTypeLabel: "Account",
        id: a.id,
        label: `${a.name} (${a.accountCode})`,
      });
    }
    for (const r of receipts.data ?? []) {
      items.push({
        entityType: "Receipt",
        entityTypeLabel: "Receipt",
        id: r.id,
        label: `${r.location} - ${r.date}`,
      });
    }
    for (const ri of receiptItems.data ?? []) {
      items.push({
        entityType: "ReceiptItem",
        entityTypeLabel: "Receipt Item",
        id: ri.id,
        label: `${ri.description} (${ri.receiptItemCode})`,
      });
    }
    for (const t of transactions.data ?? []) {
      items.push({
        entityType: "Transaction",
        entityTypeLabel: "Transaction",
        id: t.id,
        label: `$${t.amount.toFixed(2)} - ${t.date}`,
      });
    }

    return items;
  }, [accounts.data, receipts.data, receiptItems.data, transactions.data]);

  const byType = useMemo(() => {
    const map: Record<string, DeletedItem[]> = {};
    for (const item of allItems) {
      (map[item.entityType] ??= []).push(item);
    }
    return map;
  }, [allItems]);

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

      <Tabs defaultValue="all">
        <TabsList>
          <TabsTrigger value="all">All ({allItems.length})</TabsTrigger>
          {Object.entries(byType).map(([type, items]) => (
            <TabsTrigger key={type} value={type}>
              {items[0]?.entityTypeLabel ?? type} ({items.length})
            </TabsTrigger>
          ))}
        </TabsList>

        <TabsContent value="all">
          <DeletedItemsTable items={allItems} />
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

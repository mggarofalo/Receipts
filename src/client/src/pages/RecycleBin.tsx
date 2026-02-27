import { useMemo } from "react";
import { Trash2 } from "lucide-react";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useDeletedAccounts, useRestoreAccount } from "@/hooks/useAccounts";
import {
  useDeletedCategories,
  useRestoreCategory,
} from "@/hooks/useCategories";
import {
  useDeletedSubcategories,
  useRestoreSubcategory,
} from "@/hooks/useSubcategories";
import { useDeletedReceipts, useRestoreReceipt } from "@/hooks/useReceipts";
import {
  useDeletedReceiptItems,
  useRestoreReceiptItem,
} from "@/hooks/useReceiptItems";
import {
  useDeletedTransactions,
  useRestoreTransaction,
} from "@/hooks/useTransactions";
import { usePurgeTrash } from "@/hooks/useTrash";
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
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { Spinner } from "@/components/ui/spinner";

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
  const restoreCategory = useRestoreCategory();
  const restoreSubcategory = useRestoreSubcategory();
  const restoreReceipt = useRestoreReceipt();
  const restoreReceiptItem = useRestoreReceiptItem();
  const restoreTransaction = useRestoreTransaction();

  const mutations: Record<
    string,
    { mutate: (id: string) => void; isPending: boolean }
  > = {
    Account: restoreAccount,
    Category: restoreCategory,
    Subcategory: restoreSubcategory,
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
      {mutation.isPending && <Spinner size="sm" />}
      {mutation.isPending ? "Restoring..." : "Restore"}
    </Button>
  );
}

function DeletedItemsTable({
  items,
  focusedKey,
  tableRef,
  onRowClick,
}: {
  items: DeletedItem[];
  focusedKey?: string | null;
  tableRef?: React.RefObject<HTMLDivElement | null>;
  onRowClick?: (index: number) => void;
}) {
  if (items.length === 0) {
    return (
      <div className="py-12 text-center text-muted-foreground">
        No deleted items found.
      </div>
    );
  }

  return (
    <div className="rounded-md border" ref={tableRef}>
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
          {items.map((item, index) => (
            <TableRow
              key={`${item.entityType}:${item.id}`}
              className={`${onRowClick ? "cursor-pointer" : ""} ${
                focusedKey === `${item.entityType}:${item.id}`
                  ? "bg-accent"
                  : ""
              }`}
              onClick={(e) => {
                if (!onRowClick) return;
                if ((e.target as HTMLElement).closest("button, input, a, [role='button']")) return;
                onRowClick(index);
              }}
            >
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
  usePageTitle("Recycle Bin");
  const accounts = useDeletedAccounts();
  const categories = useDeletedCategories();
  const subcategories = useDeletedSubcategories();
  const receipts = useDeletedReceipts();
  const receiptItems = useDeletedReceiptItems();
  const transactions = useDeletedTransactions();
  const purgeTrash = usePurgeTrash();

  const isLoading =
    accounts.isLoading ||
    categories.isLoading ||
    subcategories.isLoading ||
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
    for (const c of categories.data ?? []) {
      items.push({
        entityType: "Category",
        entityTypeLabel: "Category",
        id: c.id,
        label: c.name,
      });
    }
    for (const s of subcategories.data ?? []) {
      items.push({
        entityType: "Subcategory",
        entityTypeLabel: "Subcategory",
        id: s.id,
        label: s.name,
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
  }, [accounts.data, categories.data, subcategories.data, receipts.data, receiptItems.data, transactions.data]);

  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items: allItems,
    getId: (item) => `${item.entityType}:${item.id}`,
    enabled: !isLoading,
  });

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
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Recycle Bin</h1>
        <AlertDialog>
          <AlertDialogTrigger asChild>
            <Button
              variant="destructive"
              size="sm"
              disabled={allItems.length === 0 || purgeTrash.isPending}
            >
              {purgeTrash.isPending ? (
                <Spinner size="sm" />
              ) : (
                <Trash2 className="size-4" />
              )}
              {purgeTrash.isPending ? "Emptying..." : "Empty Trash"}
            </Button>
          </AlertDialogTrigger>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Empty Trash?</AlertDialogTitle>
              <AlertDialogDescription>
                This will permanently delete{" "}
                <strong>
                  {allItems.length} {allItems.length === 1 ? "item" : "items"}
                </strong>
                . This action cannot be undone.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction
                variant="destructive"
                onClick={() => purgeTrash.mutate()}
              >
                Empty Trash
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>

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
          <DeletedItemsTable
            items={allItems}
            focusedKey={focusedId}
            tableRef={tableRef}
            onRowClick={setFocusedIndex}
          />
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

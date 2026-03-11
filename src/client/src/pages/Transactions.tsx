import { useState, useMemo, useEffect } from "react";
import { generateId } from "@/lib/id";
import { Link } from "react-router";
import {
  useTransactions,
  useTransactionsByReceiptId,
  useCreateTransaction,
  useUpdateTransaction,
  useDeleteTransactions,
} from "@/hooks/useTransactions";
import { useAccounts } from "@/hooks/useAccounts";
import { useReceipts } from "@/hooks/useReceipts";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useFuzzySearch } from "@/hooks/useFuzzySearch";
import { useSavedFilters } from "@/hooks/useSavedFilters";
import { useServerPagination } from "@/hooks/useServerPagination";
import { useServerSort } from "@/hooks/useServerSort";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
import { useEntityLinkParams } from "@/hooks/useEntityLinkParams";
import type { FuseSearchConfig, FilterDefinition } from "@/lib/search";
import { applyFilters } from "@/lib/search";
import type { FilterValues } from "@/components/FilterPanel";
import { ActiveFilterBanner } from "@/components/ActiveFilterBanner";
import { TransactionForm } from "@/components/TransactionForm";
import { FuzzySearchInput } from "@/components/FuzzySearchInput";
import { FilterPanel } from "@/components/FilterPanel";
import type { FilterField } from "@/components/FilterPanel";
import { SearchHighlight } from "@/components/SearchHighlight";
import { getMatchIndices } from "@/lib/search-highlight";
import { SortableTableHead } from "@/components/SortableTableHead";
import { NoResults } from "@/components/NoResults";
import { Pagination } from "@/components/Pagination";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { TableSkeleton } from "@/components/ui/table-skeleton";
import { Spinner } from "@/components/ui/spinner";
import { formatCurrency } from "@/lib/format";
import { Pencil } from "lucide-react";

interface TransactionResponse {
  id: string;
  receiptId: string;
  accountId: string;
  amount: number;
  date: string;
}

interface ReceiptInfo {
  location: string;
  date: string;
}

interface EnrichedTransaction extends TransactionResponse {
  accountName?: string;
  receiptLocation?: string;
}

const SEARCH_CONFIG: FuseSearchConfig<EnrichedTransaction> = {
  keys: [
    { name: "date", weight: 1 },
    { name: "accountName", weight: 2 },
    { name: "receiptLocation", weight: 1 },
  ],
};

const FILTER_FIELDS: FilterField[] = [
  { type: "dateRange", key: "date", label: "Date" },
  { type: "numberRange", key: "amount", label: "Amount" },
];

const FILTER_DEFS: FilterDefinition[] = [
  { key: "date", type: "dateRange", field: "date" },
  { key: "amount", type: "numberRange", field: "amount" },
];

const FILTER_PARAMS = ["receiptId", "accountId"] as const;

function Transactions() {
  usePageTitle("Transactions");
  const { sortBy, sortDirection, toggleSort } = useServerSort({ defaultSortBy: "date", defaultSortDirection: "desc" });
  const { offset, limit, currentPage, pageSize, totalPages, setPage, setPageSize, resetPage } = useServerPagination();
  const { params: linkParams, clearParams, hasActiveFilter } = useEntityLinkParams(FILTER_PARAMS);
  const allTxnQuery = useTransactions(offset, limit, sortBy, sortDirection);
  const filteredTxnQuery = useTransactionsByReceiptId(linkParams.receiptId ?? null, offset, limit, sortBy, sortDirection);
  const activeTxnQuery = linkParams.receiptId ? filteredTxnQuery : allTxnQuery;
  const { data: transactionsData, total: serverTotal, isLoading } = activeTxnQuery;
  const { data: accountsData } = useAccounts(0, 1000);
  const { data: receiptsData } = useReceipts(0, 1000);
  const createTransaction = useCreateTransaction();
  const updateTransaction = useUpdateTransaction();
  const deleteTransactions = useDeleteTransactions();

  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [createOpen, setCreateOpen] = useState(false);
  const [editTransaction, setEditTransaction] =
    useState<TransactionResponse | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [filterValues, setFilterValues] = useState<FilterValues>({});

  const anyDialogOpen = createOpen || editTransaction !== null || deleteOpen;

  useEffect(() => {
    function onNewItem() {
      setCreateOpen(true);
    }
    window.addEventListener("shortcut:new-item", onNewItem);
    return () => window.removeEventListener("shortcut:new-item", onNewItem);
  }, []);

  const accountMap = useMemo(() => {
    const map = new Map<string, string>();
    const list = (accountsData as { id: string; name: string }[] | undefined) ?? [];
    for (const a of list) map.set(a.id, a.name);
    return map;
  }, [accountsData]);

  const receiptMap = useMemo(() => {
    const map = new Map<string, ReceiptInfo>();
    const list = (receiptsData as { id: string; location: string; date: string }[] | undefined) ?? [];
    for (const r of list) map.set(r.id, { location: r.location, date: r.date });
    return map;
  }, [receiptsData]);

  useEffect(() => { resetPage(); }, [sortBy, sortDirection, resetPage]);

  const data: EnrichedTransaction[] = useMemo(() => {
    const list = (transactionsData as TransactionResponse[] | undefined) ?? [];
    return list.map((txn) => {
      const receipt = receiptMap.get(txn.receiptId);
      return {
        ...txn,
        accountName: accountMap.get(txn.accountId),
        receiptLocation: receipt?.location,
      };
    });
  }, [transactionsData, accountMap, receiptMap]);

  const {
    filters: savedFilters,
    save: saveFilter,
    remove: removeFilter,
  } = useSavedFilters("transactions");

  const { search, setSearch, results, totalCount, clearSearch } =
    useFuzzySearch({ data, config: SEARCH_CONFIG });

  const filteredResults = useMemo(() => {
    let list = results.map((r) => r.item);
    list = applyFilters(list, FILTER_DEFS, filterValues);
    if (linkParams.accountId) list = list.filter((t) => t.accountId === linkParams.accountId);
    return list;
  }, [results, filterValues, linkParams.accountId]);

  const matchMap = useMemo(() => {
    const map = new Map<string, (typeof results)[number]>();
    for (const r of results) {
      map.set(r.item.id, r);
    }
    return map;
  }, [results]);

  function toggleSelect(id: string) {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  function toggleAll() {
    if (selected.size === filteredResults.length) {
      setSelected(new Set());
    } else {
      setSelected(new Set(filteredResults.map((t) => t.id)));
    }
  }

  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items: filteredResults,
    getId: (t) => t.id,
    enabled: !anyDialogOpen,
    onOpen: (t) => {
      setEditTransaction(t);
    },
    onDelete: () => setDeleteOpen(true),
    onSelectAll: () =>
      setSelected(new Set(filteredResults.map((t) => t.id))),
    onDeselectAll: () => setSelected(new Set()),
    onToggleSelect: (t) => toggleSelect(t.id),
    selected,
  });

  if (isLoading) {
    return <TableSkeleton columns={8} />;
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold tracking-tight">Transactions</h1>
      <div className="flex items-center justify-between">
        <FuzzySearchInput
          aria-label="Search transactions"
          value={search}
          onChange={setSearch}
          placeholder="Search transactions..."
          resultCount={filteredResults.length}
          totalCount={totalCount}
          className="max-w-sm"
        />
        <div className="flex gap-2">
          {selected.size > 0 && (
            <Button variant="destructive" onClick={() => setDeleteOpen(true)}>
              Delete ({selected.size})
            </Button>
          )}
          <Button onClick={() => setCreateOpen(true)}>New Transaction</Button>
        </div>
      </div>

      <FilterPanel
        fields={FILTER_FIELDS}
        values={filterValues}
        onChange={setFilterValues}
        savedFilters={savedFilters}
        onSaveFilter={(name) =>
          saveFilter({
            id: generateId(),
            name,
            entityType: "transactions",
            values: filterValues,
            createdAt: new Date().toISOString(),
          })
        }
        onDeleteFilter={removeFilter}
        onLoadFilter={(preset) =>
          setFilterValues(preset.values as FilterValues)
        }
      />

      {hasActiveFilter && (
        <ActiveFilterBanner
          message={
            linkParams.receiptId
              ? `Showing transactions for receipt: ${receiptMap.get(linkParams.receiptId)?.location || linkParams.receiptId}`
              : `Showing transactions for account: ${accountMap.get(linkParams.accountId!) ?? linkParams.accountId}`
          }
          onClear={clearParams}
        />
      )}

      {filteredResults.length === 0 ? (
        search ? (
          <NoResults
            searchTerm={search}
            onClearSearch={clearSearch}
            onSelectSuggestion={setSearch}
            entityName="transactions"
          />
        ) : (
          <div className="py-12 text-center text-muted-foreground">
            No transactions yet. Create one to get started.
          </div>
        )
      ) : (
        <>
          <Pagination
            currentPage={currentPage}
            totalItems={serverTotal}
            pageSize={pageSize}
            totalPages={totalPages(serverTotal)}
            onPageChange={(page) => setPage(page, serverTotal)}
            onPageSizeChange={setPageSize}
          />
          <div className="rounded-md border" ref={tableRef}>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-12">
                    <input
                      type="checkbox"
                      aria-label="Select all rows"
                      checked={
                        selected.size === filteredResults.length &&
                        filteredResults.length > 0
                      }
                      onChange={toggleAll}
                      className="h-4 w-4 rounded border-gray-300"
                    />
                  </TableHead>
                  <TableHead>Account</TableHead>
                  <TableHead>Receipt</TableHead>
                  <TableHead>Location</TableHead>
                  <SortableTableHead column="amount" label="Amount" currentSortBy={sortBy} currentSortDirection={sortDirection} onToggleSort={toggleSort} className="text-right" />
                  <TableHead>Receipt Date</TableHead>
                  <SortableTableHead column="date" label="Transaction Date" currentSortBy={sortBy} currentSortDirection={sortDirection} onToggleSort={toggleSort} />
                  <TableHead className="w-24">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredResults.map((txn, index) => {
                  const result = matchMap.get(txn.id);
                  const matches = result?.matches;
                  return (
                    <TableRow
                      key={txn.id}
                      className={`cursor-pointer ${focusedId === txn.id ? "bg-accent" : ""}`}
                      onClick={(e) => {
                        if ((e.target as HTMLElement).closest("button, input, a, [role='button']")) return;
                        setFocusedIndex(index);
                      }}
                    >
                      <TableCell>
                        <input
                          type="checkbox"
                          aria-label={`Select transaction ${txn.id}`}
                          checked={selected.has(txn.id)}
                          onChange={() => toggleSelect(txn.id)}
                          className="h-4 w-4 rounded border-gray-300"
                        />
                      </TableCell>
                      <TableCell>
                        <Link to={`/accounts?highlight=${txn.accountId}`} className="text-primary hover:underline">
                          {accountMap.get(txn.accountId) ?? "Unknown"}
                        </Link>
                      </TableCell>
                      <TableCell>
                        <Link to={`/receipts?highlight=${txn.receiptId}`} className="text-sm text-primary hover:underline">
                          {receiptMap.get(txn.receiptId)?.location || "Receipt"}
                        </Link>
                      </TableCell>
                      <TableCell>
                        {receiptMap.get(txn.receiptId)?.location ?? ""}
                      </TableCell>
                      <TableCell className="text-right">
                        {formatCurrency(txn.amount)}
                      </TableCell>
                      <TableCell>
                        {receiptMap.get(txn.receiptId)?.date ?? ""}
                      </TableCell>
                      <TableCell>
                        <SearchHighlight
                          text={txn.date}
                          indices={getMatchIndices(matches, "date")}
                        />
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="icon"
                          aria-label="Edit"
                          onClick={() => {
                            setEditTransaction(txn);
                          }}
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </div>
          <Pagination
            currentPage={currentPage}
            totalItems={serverTotal}
            pageSize={pageSize}
            totalPages={totalPages(serverTotal)}
            onPageChange={(page) => setPage(page, serverTotal)}
            onPageSizeChange={setPageSize}
          />
        </>
      )}

      <Dialog open={createOpen} onOpenChange={setCreateOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Transaction</DialogTitle>
          </DialogHeader>
          <TransactionForm
            mode="create"
            isSubmitting={createTransaction.isPending}
            onCancel={() => setCreateOpen(false)}
            onSubmit={(values) => {
              createTransaction.mutate(
                {
                  receiptId: values.receiptId,
                  body: { amount: values.amount, date: values.date, accountId: values.accountId },
                },
                { onSuccess: () => setCreateOpen(false) },
              );
            }}
          />
        </DialogContent>
      </Dialog>

      <Dialog
        open={editTransaction !== null}
        onOpenChange={(open) => {
          if (!open) setEditTransaction(null);
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Transaction</DialogTitle>
          </DialogHeader>
          {editTransaction && (
            <TransactionForm
              mode="edit"
              defaultValues={{
                receiptId: editTransaction.receiptId,
                accountId: editTransaction.accountId,
                amount: editTransaction.amount,
                date: editTransaction.date,
              }}
              isSubmitting={updateTransaction.isPending}
              onCancel={() => setEditTransaction(null)}
              onSubmit={(values) => {
                updateTransaction.mutate(
                  {
                    body: {
                      id: editTransaction.id,
                      amount: values.amount,
                      date: values.date,
                      accountId: values.accountId,
                    },
                  },
                  { onSuccess: () => setEditTransaction(null) },
                );
              }}
            />
          )}
        </DialogContent>
      </Dialog>

      <Dialog open={deleteOpen} onOpenChange={setDeleteOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Transactions</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selected.size} transaction(s)? This
            action can be undone by restoring.
          </p>
          <div className="flex justify-end gap-2 pt-4">
            <Button variant="outline" onClick={() => setDeleteOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deleteTransactions.isPending}
              onClick={() => {
                const ids = [...selected];
                setSelected(new Set());
                setDeleteOpen(false);
                deleteTransactions.mutate(ids);
              }}
            >
              {deleteTransactions.isPending && <Spinner size="sm" />}
              {deleteTransactions.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}

export default Transactions;

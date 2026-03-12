import { useState, useMemo, useEffect } from "react";
import { generateId } from "@/lib/id";
import { Link } from "react-router";
import {
  useReceiptItems,
  useReceiptItemsByReceiptId,
  useCreateReceiptItem,
  useUpdateReceiptItem,
  useDeleteReceiptItems,
} from "@/hooks/useReceiptItems";
import { useReceipts } from "@/hooks/useReceipts";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useFuzzySearch } from "@/hooks/useFuzzySearch";
import { useSavedFilters } from "@/hooks/useSavedFilters";
import { useServerPagination } from "@/hooks/useServerPagination";
import { useServerSort } from "@/hooks/useServerSort";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
import type { FuseSearchConfig, FilterDefinition } from "@/lib/search";
import { applyFilters } from "@/lib/search";
import type { FilterValues } from "@/components/FilterPanel";
import { ReceiptItemForm } from "@/components/ReceiptItemForm";
import { FuzzySearchInput } from "@/components/FuzzySearchInput";
import { FilterPanel } from "@/components/FilterPanel";
import type { FilterField } from "@/components/FilterPanel";
import { SearchHighlight } from "@/components/SearchHighlight";
import { getMatchIndices } from "@/lib/search-highlight";
import { useEntityLinkParams } from "@/hooks/useEntityLinkParams";
import { ActiveFilterBanner } from "@/components/ActiveFilterBanner";
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
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { TableSkeleton } from "@/components/ui/table-skeleton";
import { Spinner } from "@/components/ui/spinner";
import { formatCurrency } from "@/lib/format";
import { Pencil } from "lucide-react";

interface ReceiptItemResponse {
  id: string;
  receiptId: string;
  receiptItemCode: string | null;
  description: string;
  quantity: number;
  unitPrice: number;
  category: string;
  subcategory: string | null;
  pricingMode: "quantity" | "flat";
}

const SEARCH_CONFIG: FuseSearchConfig<ReceiptItemResponse> = {
  keys: [
    { name: "description", weight: 2 },
    { name: "category", weight: 1.5 },
    { name: "receiptItemCode", weight: 1 },
    { name: "subcategory", weight: 0.5 },
  ],
};

const FILTER_DEFS: FilterDefinition[] = [
  { key: "category", type: "select", field: "category" },
  { key: "unitPrice", type: "numberRange", field: "unitPrice" },
];

const FILTER_PARAMS = ["receiptId", "subcategory"] as const;

function ReceiptItems() {
  usePageTitle("Receipt Items");
  const { sortBy, sortDirection, toggleSort } = useServerSort({ defaultSortBy: "description", defaultSortDirection: "asc" });
  const { offset, limit, currentPage, pageSize, totalPages, setPage, setPageSize, resetPage } = useServerPagination();
  const { params: linkParams, clearParams, hasActiveFilter } = useEntityLinkParams(FILTER_PARAMS);
  const allItemsQuery = useReceiptItems(offset, limit, sortBy, sortDirection);
  const filteredItemsQuery = useReceiptItemsByReceiptId(linkParams.receiptId ?? null, offset, limit, sortBy, sortDirection);
  const activeItemsQuery = linkParams.receiptId ? filteredItemsQuery : allItemsQuery;
  const { data: itemsData, total: serverTotal, isLoading } = activeItemsQuery;
  const { data: receiptsData } = useReceipts(0, 1000);
  const createItem = useCreateReceiptItem();
  const updateItem = useUpdateReceiptItem();
  const deleteItems = useDeleteReceiptItems();

  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [createOpen, setCreateOpen] = useState(false);
  const [editItem, setEditItem] = useState<ReceiptItemResponse | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [filterValues, setFilterValues] = useState<FilterValues>({
    category: "all",
  });

  const anyDialogOpen = createOpen || editItem !== null || deleteOpen;

  useEffect(() => {
    function onNewItem() {
      setCreateOpen(true);
    }
    window.addEventListener("shortcut:new-item", onNewItem);
    return () => window.removeEventListener("shortcut:new-item", onNewItem);
  }, []);

  useEffect(() => { resetPage(); }, [sortBy, sortDirection, resetPage]);

  const data = useMemo(
    () => (itemsData as ReceiptItemResponse[] | undefined) ?? [],
    [itemsData],
  );

  const categories = useMemo(
    () => [...new Set(data.map((i) => i.category))].sort(),
    [data],
  );

  const filterFields: FilterField[] = useMemo(
    () => [
      {
        type: "select",
        key: "category",
        label: "Category",
        options: categories,
      },
      { type: "numberRange", key: "unitPrice", label: "Unit Price" },
    ],
    [categories],
  );

  const receiptMap = useMemo(() => {
    const map = new Map<string, string>();
    const list = (receiptsData as { id: string; location: string }[] | undefined) ?? [];
    for (const r of list) map.set(r.id, r.location);
    return map;
  }, [receiptsData]);

  const {
    filters: savedFilters,
    save: saveFilter,
    remove: removeFilter,
  } = useSavedFilters("receiptItems");

  const { search, setSearch, results, totalCount, clearSearch } =
    useFuzzySearch({ data, config: SEARCH_CONFIG });

  const filteredResults = useMemo(() => {
    let list = results.map((r) => r.item);
    list = applyFilters(list, FILTER_DEFS, filterValues);
    if (linkParams.subcategory) list = list.filter((i) => i.subcategory === linkParams.subcategory);
    return list;
  }, [results, filterValues, linkParams.subcategory]);

  const matchMap = useMemo(() => {
    const map = new Map<string, (typeof results)[number]>();
    for (const r of results) {
      map.set(r.item.id, r);
    }
    return map;
  }, [results]);

  const grandTotal = useMemo(
    () =>
      filteredResults.reduce(
        (sum, item) => sum + item.quantity * item.unitPrice,
        0,
      ),
    [filteredResults],
  );

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
      setSelected(new Set(filteredResults.map((item) => item.id)));
    }
  }

  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items: filteredResults,
    getId: (item) => item.id,
    enabled: !anyDialogOpen,
    onOpen: (item) => {
      setEditItem(item);
    },
    onDelete: () => setDeleteOpen(true),
    onSelectAll: () =>
      setSelected(new Set(filteredResults.map((item) => item.id))),
    onDeselectAll: () => setSelected(new Set()),
    onToggleSelect: (item) => toggleSelect(item.id),
    selected,
  });

  if (isLoading) {
    return <TableSkeleton columns={9} />;
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold tracking-tight">Receipt Items</h1>
      <div className="flex items-center justify-between">
        <FuzzySearchInput
          aria-label="Search receipt items"
          value={search}
          onChange={setSearch}
          placeholder="Search items..."
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
          <Button onClick={() => setCreateOpen(true)}>New Item</Button>
        </div>
      </div>

      <FilterPanel
        fields={filterFields}
        values={filterValues}
        onChange={setFilterValues}
        savedFilters={savedFilters}
        onSaveFilter={(name) =>
          saveFilter({
            id: generateId(),
            name,
            entityType: "receiptItems",
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
              ? `Showing items for receipt: ${receiptMap.get(linkParams.receiptId) ?? linkParams.receiptId}`
              : `Showing items for subcategory: ${linkParams.subcategory}`
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
            entityName="receipt items"
          />
        ) : (
          <div className="py-12 text-center text-muted-foreground">
            No receipt items yet. Create one to get started.
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
                  <TableHead>Code</TableHead>
                  <SortableTableHead column="description" label="Description" currentSortBy={sortBy} currentSortDirection={sortDirection} onToggleSort={toggleSort} />
                  <TableHead>Mode</TableHead>
                  <SortableTableHead column="quantity" label="Qty" currentSortBy={sortBy} currentSortDirection={sortDirection} onToggleSort={toggleSort} className="text-right" />
                  <SortableTableHead column="unitPrice" label="Unit Price" currentSortBy={sortBy} currentSortDirection={sortDirection} onToggleSort={toggleSort} className="text-right" />
                  <SortableTableHead column="totalAmount" label="Total" currentSortBy={sortBy} currentSortDirection={sortDirection} onToggleSort={toggleSort} className="text-right" />
                  <SortableTableHead column="category" label="Category" currentSortBy={sortBy} currentSortDirection={sortDirection} onToggleSort={toggleSort} />
                  <TableHead>Subcategory</TableHead>
                  <TableHead>Receipt</TableHead>
                  <TableHead className="w-24">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredResults.map((item, index) => {
                  const result = matchMap.get(item.id);
                  const matches = result?.matches;
                  return (
                    <TableRow
                      key={item.id}
                      className={`cursor-pointer ${focusedId === item.id ? "bg-accent" : ""}`}
                      onClick={(e) => {
                        if ((e.target as HTMLElement).closest("button, input, a, [role='button']")) return;
                        setFocusedIndex(index);
                      }}
                    >
                      <TableCell>
                        <input
                          type="checkbox"
                          aria-label={`Select ${item.description}`}
                          checked={selected.has(item.id)}
                          onChange={() => toggleSelect(item.id)}
                          className="h-4 w-4 rounded border-gray-300"
                        />
                      </TableCell>
                      <TableCell className="font-mono">
                        <SearchHighlight
                          text={item.receiptItemCode ?? ""}
                          indices={getMatchIndices(matches, "receiptItemCode")}
                        />
                      </TableCell>
                      <TableCell>
                        <SearchHighlight
                          text={item.description}
                          indices={getMatchIndices(matches, "description")}
                        />
                      </TableCell>
                      <TableCell>
                        <span className="text-xs text-muted-foreground">
                          {item.pricingMode === "flat" ? "Flat" : "Qty"}
                        </span>
                      </TableCell>
                      <TableCell className="text-right">
                        {item.quantity}
                      </TableCell>
                      <TableCell className="text-right">
                        {formatCurrency(item.unitPrice)}
                      </TableCell>
                      <TableCell className="text-right">
                        {formatCurrency(item.quantity * item.unitPrice)}
                      </TableCell>
                      <TableCell>
                        <SearchHighlight
                          text={item.category}
                          indices={getMatchIndices(matches, "category")}
                        />
                      </TableCell>
                      <TableCell>{item.subcategory ?? ""}</TableCell>
                      <TableCell>
                        <Link to={`/receipts?highlight=${item.receiptId}`} className="text-sm text-primary hover:underline">
                          Receipt
                        </Link>
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="icon"
                          aria-label="Edit"
                          onClick={() => {
                            setEditItem(item);
                          }}
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
              <TableFooter>
                <TableRow>
                  <TableCell colSpan={6} className="text-right font-medium">
                    Grand Total
                  </TableCell>
                  <TableCell className="text-right font-bold">
                    {formatCurrency(grandTotal)}
                  </TableCell>
                  <TableCell colSpan={4} />
                </TableRow>
              </TableFooter>
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
            <DialogTitle>Create Receipt Item</DialogTitle>
          </DialogHeader>
          <ReceiptItemForm
            mode="create"
            isSubmitting={createItem.isPending}
            onCancel={() => setCreateOpen(false)}
            onSubmit={(values) => {
              const { receiptId, ...body } = values;
              createItem.mutate(
                { receiptId, body },
                { onSuccess: () => setCreateOpen(false) },
              );
            }}
          />
        </DialogContent>
      </Dialog>

      <Dialog
        open={editItem !== null}
        onOpenChange={(open) => {
          if (!open) setEditItem(null);
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Receipt Item</DialogTitle>
          </DialogHeader>
          {editItem && (
            <ReceiptItemForm
              mode="edit"
              defaultValues={{
                receiptId: editItem.receiptId,
                receiptItemCode: editItem.receiptItemCode ?? "",
                description: editItem.description,
                pricingMode: editItem.pricingMode ?? "quantity",
                quantity: editItem.quantity,
                unitPrice: editItem.unitPrice,
                category: editItem.category,
                subcategory: editItem.subcategory ?? "",
              }}
              isSubmitting={updateItem.isPending}
              onCancel={() => setEditItem(null)}
              onSubmit={(values) => {
                const { receiptId: _receiptId, ...rest } = values;
                updateItem.mutate(
                  {
                    body: { id: editItem.id, ...rest },
                  },
                  { onSuccess: () => setEditItem(null) },
                );
              }}
            />
          )}
        </DialogContent>
      </Dialog>

      <Dialog open={deleteOpen} onOpenChange={setDeleteOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Receipt Items</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selected.size} item(s)? This action
            can be undone by restoring.
          </p>
          <div className="flex justify-end gap-2 pt-4">
            <Button variant="outline" onClick={() => setDeleteOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deleteItems.isPending}
              onClick={() => {
                const ids = [...selected];
                setSelected(new Set());
                setDeleteOpen(false);
                deleteItems.mutate(ids);
              }}
            >
              {deleteItems.isPending && <Spinner size="sm" />}
              {deleteItems.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}

export default ReceiptItems;

import { useState, useMemo, useEffect } from "react";
import {
  useReceiptItems,
  useCreateReceiptItem,
  useUpdateReceiptItem,
  useDeleteReceiptItems,
} from "@/hooks/useReceiptItems";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useFuzzySearch } from "@/hooks/useFuzzySearch";
import { useSavedFilters } from "@/hooks/useSavedFilters";
import { usePagination } from "@/hooks/usePagination";
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

interface ReceiptItemResponse {
  id: string;
  receiptItemCode: string;
  description: string;
  quantity: number;
  unitPrice: number;
  category: string;
  subcategory: string;
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(amount);
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

function ReceiptItems() {
  usePageTitle("Receipt Items");
  const { data: items, isLoading } = useReceiptItems();
  const createItem = useCreateReceiptItem();
  const updateItem = useUpdateReceiptItem();
  const deleteItems = useDeleteReceiptItems();

  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [createOpen, setCreateOpen] = useState(false);
  const [editItem, setEditItem] = useState<ReceiptItemResponse | null>(null);
  const [editReceiptId, setEditReceiptId] = useState("");
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

  const data = useMemo(
    () => (items as ReceiptItemResponse[] | undefined) ?? [],
    [items],
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

  const {
    filters: savedFilters,
    save: saveFilter,
    remove: removeFilter,
  } = useSavedFilters("receiptItems");

  const { search, setSearch, results, totalCount, clearSearch } =
    useFuzzySearch({ data, config: SEARCH_CONFIG });

  const filteredResults = useMemo(() => {
    const list = results.map((r) => r.item);
    return applyFilters(list, FILTER_DEFS, filterValues);
  }, [results, filterValues]);

  const matchMap = useMemo(() => {
    const map = new Map<string, (typeof results)[number]>();
    for (const r of results) {
      map.set(r.item.id, r);
    }
    return map;
  }, [results]);

  const {
    paginatedItems,
    currentPage,
    pageSize,
    totalItems,
    totalPages,
    setPage,
    setPageSize,
  } = usePagination({ items: filteredResults });

  const grandTotal = useMemo(
    () =>
      paginatedItems.reduce(
        (sum, item) => sum + item.quantity * item.unitPrice,
        0,
      ),
    [paginatedItems],
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
    if (selected.size === paginatedItems.length) {
      setSelected(new Set());
    } else {
      setSelected(new Set(paginatedItems.map((item) => item.id)));
    }
  }

  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items: paginatedItems,
    getId: (item) => item.id,
    enabled: !anyDialogOpen,
    onOpen: (item) => setEditItem(item),
    onDelete: () => setDeleteOpen(true),
    onSelectAll: () =>
      setSelected(new Set(paginatedItems.map((item) => item.id))),
    onDeselectAll: () => setSelected(new Set()),
    onToggleSelect: (item) => toggleSelect(item.id),
    selected,
  });

  if (isLoading) {
    return <TableSkeleton columns={8} />;
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
            id: crypto.randomUUID(),
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
          <div className="rounded-md border" ref={tableRef}>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-12">
                    <input
                      type="checkbox"
                      aria-label="Select all rows"
                      checked={
                        selected.size === paginatedItems.length &&
                        paginatedItems.length > 0
                      }
                      onChange={toggleAll}
                      className="h-4 w-4 rounded border-gray-300"
                    />
                  </TableHead>
                  <TableHead>Code</TableHead>
                  <TableHead>Description</TableHead>
                  <TableHead className="text-right">Qty</TableHead>
                  <TableHead className="text-right">Unit Price</TableHead>
                  <TableHead className="text-right">Total</TableHead>
                  <TableHead>Category</TableHead>
                  <TableHead>Subcategory</TableHead>
                  <TableHead className="w-24">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((item, index) => {
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
                          text={item.receiptItemCode}
                          indices={getMatchIndices(matches, "receiptItemCode")}
                        />
                      </TableCell>
                      <TableCell>
                        <SearchHighlight
                          text={item.description}
                          indices={getMatchIndices(matches, "description")}
                        />
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
                      <TableCell>{item.subcategory}</TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => setEditItem(item)}
                        >
                          Edit
                        </Button>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
              <TableFooter>
                <TableRow>
                  <TableCell colSpan={5} className="text-right font-medium">
                    Grand Total
                  </TableCell>
                  <TableCell className="text-right font-bold">
                    {formatCurrency(grandTotal)}
                  </TableCell>
                  <TableCell colSpan={3} />
                </TableRow>
              </TableFooter>
            </Table>
          </div>
          <Pagination
            currentPage={currentPage}
            totalItems={totalItems}
            pageSize={pageSize}
            totalPages={totalPages}
            onPageChange={setPage}
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
          if (!open) {
            setEditItem(null);
            setEditReceiptId("");
          }
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
                receiptId: editReceiptId,
                receiptItemCode: editItem.receiptItemCode,
                description: editItem.description,
                quantity: editItem.quantity,
                unitPrice: editItem.unitPrice,
                category: editItem.category,
                subcategory: editItem.subcategory,
              }}
              isSubmitting={updateItem.isPending}
              onCancel={() => {
                setEditItem(null);
                setEditReceiptId("");
              }}
              onSubmit={(values) => {
                const { receiptId, ...rest } = values;
                updateItem.mutate(
                  {
                    receiptId,
                    body: { id: editItem.id, ...rest },
                  },
                  {
                    onSuccess: () => {
                      setEditItem(null);
                      setEditReceiptId("");
                    },
                  },
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

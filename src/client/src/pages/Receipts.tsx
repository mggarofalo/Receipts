import { useState, useMemo, useEffect } from "react";
import { useReceipts } from "@/hooks/useReceipts";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useFuzzySearch } from "@/hooks/useFuzzySearch";
import { useSavedFilters } from "@/hooks/useSavedFilters";
import { usePagination } from "@/hooks/usePagination";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
import type { FuseSearchConfig, FilterDefinition } from "@/lib/search";
import { applyFilters } from "@/lib/search";
import type { FilterValues } from "@/components/FilterPanel";
import { ReceiptDialogs } from "@/components/ReceiptDialogs";
import { FuzzySearchInput } from "@/components/FuzzySearchInput";
import { FilterPanel } from "@/components/FilterPanel";
import type { FilterField } from "@/components/FilterPanel";
import { SearchHighlight } from "@/components/SearchHighlight";
import { getMatchIndices } from "@/lib/search-highlight";
import { NoResults } from "@/components/NoResults";
import { Pagination } from "@/components/Pagination";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { TableSkeleton } from "@/components/ui/table-skeleton";
import { formatCurrency } from "@/lib/format";

interface ReceiptResponse {
  id: string;
  description?: string | null;
  location: string;
  date: string;
  taxAmount: number;
}

const SEARCH_CONFIG: FuseSearchConfig<ReceiptResponse> = {
  keys: [
    { name: "description", weight: 2 },
    { name: "location", weight: 1.5 },
  ],
};

const FILTER_FIELDS: FilterField[] = [
  { type: "dateRange", key: "date", label: "Date" },
  { type: "numberRange", key: "taxAmount", label: "Tax Amount" },
];

const FILTER_DEFS: FilterDefinition[] = [
  { key: "date", type: "dateRange", field: "date" },
  { key: "taxAmount", type: "numberRange", field: "taxAmount" },
];

function Receipts() {
  usePageTitle("Receipts");
  const { data: receipts, isLoading } = useReceipts();

  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [createOpen, setCreateOpen] = useState(false);
  const [editReceipt, setEditReceipt] = useState<ReceiptResponse | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [filterValues, setFilterValues] = useState<FilterValues>({});

  const anyDialogOpen = createOpen || editReceipt !== null || deleteOpen;

  useEffect(() => {
    function onNewItem() {
      setCreateOpen(true);
    }
    window.addEventListener("shortcut:new-item", onNewItem);
    return () => window.removeEventListener("shortcut:new-item", onNewItem);
  }, []);

  const data = useMemo(() => {
    const list = (receipts as ReceiptResponse[] | undefined) ?? [];
    return [...list].sort(
      (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime(),
    );
  }, [receipts]);

  const {
    filters: savedFilters,
    save: saveFilter,
    remove: removeFilter,
  } = useSavedFilters("receipts");

  const { search, setSearch, results, totalCount, clearSearch } =
    useFuzzySearch({ data, config: SEARCH_CONFIG });

  const filteredResults = useMemo(() => {
    const items = search.trim()
      ? results.map((r) => r.item)
      : results.map((r) => r.item);
    return applyFilters(items, FILTER_DEFS, filterValues);
  }, [results, filterValues, search]);

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
      setSelected(new Set(paginatedItems.map((r) => r.id)));
    }
  }

  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items: paginatedItems,
    getId: (r) => r.id,
    enabled: !anyDialogOpen,
    onOpen: (r) => setEditReceipt(r),
    onDelete: () => setDeleteOpen(true),
    onSelectAll: () =>
      setSelected(new Set(paginatedItems.map((r) => r.id))),
    onDeselectAll: () => setSelected(new Set()),
    onToggleSelect: (r) => toggleSelect(r.id),
    selected,
  });

  if (isLoading) {
    return <TableSkeleton columns={5} />;
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold tracking-tight">Receipts</h1>
      <div className="flex items-center justify-between">
        <FuzzySearchInput
          aria-label="Search receipts"
          value={search}
          onChange={setSearch}
          placeholder="Search receipts..."
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
          <Button onClick={() => setCreateOpen(true)}>New Receipt</Button>
        </div>
      </div>

      <FilterPanel
        fields={FILTER_FIELDS}
        values={filterValues}
        onChange={setFilterValues}
        savedFilters={savedFilters}
        onSaveFilter={(name) =>
          saveFilter({
            id: crypto.randomUUID(),
            name,
            entityType: "receipts",
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
            entityName="receipts"
          />
        ) : (
          <div className="py-12 text-center text-muted-foreground">
            No receipts yet. Create one to get started.
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
                  <TableHead>Description</TableHead>
                  <TableHead>Location</TableHead>
                  <TableHead>Date</TableHead>
                  <TableHead className="text-right">Tax Amount</TableHead>
                  <TableHead className="w-24">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((receipt, index) => {
                  const result = matchMap.get(receipt.id);
                  const matches = result?.matches;
                  return (
                    <TableRow
                      key={receipt.id}
                      className={`cursor-pointer ${focusedId === receipt.id ? "bg-accent" : ""}`}
                      onClick={(e) => {
                        if ((e.target as HTMLElement).closest("button, input, a, [role='button']")) return;
                        setFocusedIndex(index);
                      }}
                    >
                      <TableCell>
                        <input
                          type="checkbox"
                          aria-label={`Select ${receipt.description || receipt.id}`}
                          checked={selected.has(receipt.id)}
                          onChange={() => toggleSelect(receipt.id)}
                          className="h-4 w-4 rounded border-gray-300"
                        />
                      </TableCell>
                      <TableCell>
                        {receipt.description ? (
                          <SearchHighlight
                            text={receipt.description}
                            indices={getMatchIndices(matches, "description")}
                          />
                        ) : (
                          <span className="text-muted-foreground italic">
                            No description
                          </span>
                        )}
                      </TableCell>
                      <TableCell>
                        <SearchHighlight
                          text={receipt.location}
                          indices={getMatchIndices(matches, "location")}
                        />
                      </TableCell>
                      <TableCell>{receipt.date}</TableCell>
                      <TableCell className="text-right">
                        {formatCurrency(receipt.taxAmount)}
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => setEditReceipt(receipt)}
                        >
                          Edit
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
            totalItems={totalItems}
            pageSize={pageSize}
            totalPages={totalPages}
            onPageChange={setPage}
            onPageSizeChange={setPageSize}
          />
        </>
      )}

      <ReceiptDialogs
        createOpen={createOpen}
        onCreateOpenChange={setCreateOpen}
        editReceipt={editReceipt}
        onEditClose={() => setEditReceipt(null)}
        deleteOpen={deleteOpen}
        onDeleteOpenChange={setDeleteOpen}
        selectedIds={[...selected]}
        onDeleteComplete={() => {
          setSelected(new Set());
          setDeleteOpen(false);
        }}
      />
    </div>
  );
}

export default Receipts;

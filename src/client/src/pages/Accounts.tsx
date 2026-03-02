import { useState, useMemo, useEffect } from "react";
import { useAccounts } from "@/hooks/useAccounts";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useFuzzySearch } from "@/hooks/useFuzzySearch";
import { useSavedFilters } from "@/hooks/useSavedFilters";
import { usePagination } from "@/hooks/usePagination";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
import type { FuseSearchConfig, FilterDefinition } from "@/lib/search";
import { applyFilters } from "@/lib/search";
import type { FilterValues } from "@/components/FilterPanel";
import { AccountDialogs } from "@/components/AccountDialogs";
import { FuzzySearchInput } from "@/components/FuzzySearchInput";
import { FilterPanel } from "@/components/FilterPanel";
import type { FilterField } from "@/components/FilterPanel";
import { SearchHighlight } from "@/components/SearchHighlight";
import { getMatchIndices } from "@/lib/search-highlight";
import { NoResults } from "@/components/NoResults";
import { Pagination } from "@/components/Pagination";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { TableSkeleton } from "@/components/ui/table-skeleton";

interface AccountResponse {
  id: string;
  accountCode: string;
  name: string;
  isActive: boolean;
}

const SEARCH_CONFIG: FuseSearchConfig<AccountResponse> = {
  keys: [
    { name: "name", weight: 2 },
    { name: "accountCode", weight: 1 },
  ],
};

const FILTER_FIELDS: FilterField[] = [
  { type: "boolean", key: "isActive", label: "Active" },
];

const FILTER_DEFS: FilterDefinition[] = [
  { key: "isActive", type: "boolean", field: "isActive" },
];

function Accounts() {
  usePageTitle("Accounts");
  const { data: accounts, isLoading } = useAccounts();

  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [createOpen, setCreateOpen] = useState(false);
  const [editAccount, setEditAccount] = useState<AccountResponse | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [filterValues, setFilterValues] = useState<FilterValues>({
    isActive: "all",
  });

  const anyDialogOpen = createOpen || editAccount !== null || deleteOpen;

  useEffect(() => {
    function onNewItem() {
      setCreateOpen(true);
    }
    window.addEventListener("shortcut:new-item", onNewItem);
    return () => window.removeEventListener("shortcut:new-item", onNewItem);
  }, []);

  const data = (accounts as AccountResponse[] | undefined) ?? [];
  const {
    filters: savedFilters,
    save: saveFilter,
    remove: removeFilter,
  } = useSavedFilters("accounts");

  const { search, setSearch, results, totalCount, clearSearch } =
    useFuzzySearch({ data, config: SEARCH_CONFIG });

  const filteredResults = useMemo(() => {
    const items = results.map((r) => r.item);
    return applyFilters(items, FILTER_DEFS, filterValues);
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
      setSelected(new Set(paginatedItems.map((a) => a.id)));
    }
  }

  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items: paginatedItems,
    getId: (a) => a.id,
    enabled: !anyDialogOpen,
    onOpen: (a) => setEditAccount(a),
    onDelete: () => setDeleteOpen(true),
    onSelectAll: () =>
      setSelected(new Set(paginatedItems.map((a) => a.id))),
    onDeselectAll: () => setSelected(new Set()),
    onToggleSelect: (a) => toggleSelect(a.id),
    selected,
  });

  if (isLoading) {
    return <TableSkeleton columns={4} />;
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold tracking-tight">Accounts</h1>
      <div className="flex items-center justify-between">
        <FuzzySearchInput
          aria-label="Search accounts"
          value={search}
          onChange={setSearch}
          placeholder="Search accounts..."
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
          <Button onClick={() => setCreateOpen(true)}>New Account</Button>
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
            entityType: "accounts",
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
            entityName="accounts"
          />
        ) : (
          <div className="py-12 text-center text-muted-foreground">
            No accounts yet. Create one to get started.
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
                  <TableHead>Account Code</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="w-24">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((account, index) => {
                  const result = matchMap.get(account.id);
                  const matches = result?.matches;
                  return (
                    <TableRow
                      key={account.id}
                      className={`cursor-pointer ${focusedId === account.id ? "bg-accent" : ""}`}
                      onClick={(e) => {
                        if ((e.target as HTMLElement).closest("button, input, a, [role='button']")) return;
                        setFocusedIndex(index);
                      }}
                    >
                      <TableCell>
                        <input
                          type="checkbox"
                          aria-label={`Select ${account.name}`}
                          checked={selected.has(account.id)}
                          onChange={() => toggleSelect(account.id)}
                          className="h-4 w-4 rounded border-gray-300"
                        />
                      </TableCell>
                      <TableCell className="font-mono">
                        <SearchHighlight
                          text={account.accountCode}
                          indices={getMatchIndices(matches, "accountCode")}
                        />
                      </TableCell>
                      <TableCell>
                        <SearchHighlight
                          text={account.name}
                          indices={getMatchIndices(matches, "name")}
                        />
                      </TableCell>
                      <TableCell>
                        <Badge
                          variant={account.isActive ? "default" : "secondary"}
                        >
                          {account.isActive ? "Active" : "Inactive"}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => setEditAccount(account)}
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

      <AccountDialogs
        createOpen={createOpen}
        onCreateOpenChange={setCreateOpen}
        editAccount={editAccount}
        onEditClose={() => setEditAccount(null)}
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

export default Accounts;

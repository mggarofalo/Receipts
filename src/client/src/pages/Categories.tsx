import { useState, useMemo, useEffect } from "react";
import { useCategories } from "@/hooks/useCategories";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useFuzzySearch } from "@/hooks/useFuzzySearch";
import { useSavedFilters } from "@/hooks/useSavedFilters";
import { usePagination } from "@/hooks/usePagination";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
import type { FuseSearchConfig } from "@/lib/search";
import { CategoryDialogs } from "@/components/CategoryDialogs";
import { FuzzySearchInput } from "@/components/FuzzySearchInput";
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

interface CategoryResponse {
  id: string;
  name: string;
  description?: string | null;
}

const SEARCH_CONFIG: FuseSearchConfig<CategoryResponse> = {
  keys: [
    { name: "name", weight: 2 },
    { name: "description", weight: 1 },
  ],
};

function Categories() {
  usePageTitle("Categories");
  const { data: categories, isLoading } = useCategories();

  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [createOpen, setCreateOpen] = useState(false);
  const [editCategory, setEditCategory] = useState<CategoryResponse | null>(
    null,
  );
  const [deleteOpen, setDeleteOpen] = useState(false);

  const anyDialogOpen = createOpen || editCategory !== null || deleteOpen;

  useEffect(() => {
    function onNewItem() {
      setCreateOpen(true);
    }
    window.addEventListener("shortcut:new-item", onNewItem);
    return () => window.removeEventListener("shortcut:new-item", onNewItem);
  }, []);

  const data = (categories as CategoryResponse[] | undefined) ?? [];
  useSavedFilters("categories");

  const { search, setSearch, results, totalCount, clearSearch } =
    useFuzzySearch({ data, config: SEARCH_CONFIG });

  const filteredResults = useMemo(() => {
    return results.map((r) => r.item);
  }, [results]);

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
    onOpen: (a) => setEditCategory(a),
    onDelete: () => setDeleteOpen(true),
    onSelectAll: () =>
      setSelected(new Set(paginatedItems.map((a) => a.id))),
    onDeselectAll: () => setSelected(new Set()),
    onToggleSelect: (a) => toggleSelect(a.id),
    selected,
  });

  if (isLoading) {
    return <TableSkeleton columns={3} />;
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold tracking-tight">Categories</h1>
      <div className="flex items-center justify-between">
        <FuzzySearchInput
          aria-label="Search categories"
          value={search}
          onChange={setSearch}
          placeholder="Search categories..."
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
          <Button onClick={() => setCreateOpen(true)}>New Category</Button>
        </div>
      </div>

      {filteredResults.length === 0 ? (
        search ? (
          <NoResults
            searchTerm={search}
            onClearSearch={clearSearch}
            onSelectSuggestion={setSearch}
            entityName="categories"
          />
        ) : (
          <div className="py-12 text-center text-muted-foreground">
            No categories yet. Create one to get started.
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
                  <TableHead>Name</TableHead>
                  <TableHead>Description</TableHead>
                  <TableHead className="w-24">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((category, index) => {
                  const result = matchMap.get(category.id);
                  const matches = result?.matches;
                  return (
                    <TableRow
                      key={category.id}
                      className={`cursor-pointer ${focusedId === category.id ? "bg-accent" : ""}`}
                      onClick={(e) => {
                        if (
                          (e.target as HTMLElement).closest(
                            "button, input, a, [role='button']",
                          )
                        )
                          return;
                        setFocusedIndex(index);
                      }}
                    >
                      <TableCell>
                        <input
                          type="checkbox"
                          aria-label={`Select ${category.name}`}
                          checked={selected.has(category.id)}
                          onChange={() => toggleSelect(category.id)}
                          className="h-4 w-4 rounded border-gray-300"
                        />
                      </TableCell>
                      <TableCell>
                        <SearchHighlight
                          text={category.name}
                          indices={getMatchIndices(matches, "name")}
                        />
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {category.description ? (
                          <SearchHighlight
                            text={category.description}
                            indices={getMatchIndices(matches, "description")}
                          />
                        ) : (
                          <span className="italic">--</span>
                        )}
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => setEditCategory(category)}
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

      <CategoryDialogs
        createOpen={createOpen}
        onCreateOpenChange={setCreateOpen}
        editCategory={editCategory}
        onEditClose={() => setEditCategory(null)}
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

export default Categories;

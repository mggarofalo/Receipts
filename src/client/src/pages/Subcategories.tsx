import { useState, useMemo, useEffect } from "react";
import { Link } from "react-router";
import {
  useSubcategories,
  useSubcategoriesByCategoryId,
  useCreateSubcategory,
  useUpdateSubcategory,
  useDeleteSubcategories,
} from "@/hooks/useSubcategories";
import { useCategories } from "@/hooks/useCategories";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useEntityLinkParams } from "@/hooks/useEntityLinkParams";
import { useFuzzySearch } from "@/hooks/useFuzzySearch";
import { useSavedFilters } from "@/hooks/useSavedFilters";
import { useServerPagination } from "@/hooks/useServerPagination";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
import type { FuseSearchConfig, FilterDefinition } from "@/lib/search";
import { applyFilters } from "@/lib/search";
import type { FilterValues } from "@/components/FilterPanel";
import { SubcategoryForm } from "@/components/SubcategoryForm";
import { FuzzySearchInput } from "@/components/FuzzySearchInput";
import { FilterPanel } from "@/components/FilterPanel";
import type { FilterField } from "@/components/FilterPanel";
import { SearchHighlight } from "@/components/SearchHighlight";
import { getMatchIndices } from "@/lib/search-highlight";
import { ActiveFilterBanner } from "@/components/ActiveFilterBanner";
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
import { Pencil } from "lucide-react";

interface SubcategoryResponse {
  id: string;
  name: string;
  categoryId: string;
  description?: string | null;
}

interface CategoryResponse {
  id: string;
  name: string;
}

const SEARCH_CONFIG: FuseSearchConfig<SubcategoryResponse> = {
  keys: [
    { name: "name", weight: 2 },
    { name: "description", weight: 1 },
  ],
};

const FILTER_PARAMS = ["categoryId"] as const;

function Subcategories() {
  usePageTitle("Subcategories");
  const { params: linkParams, clearParams, hasActiveFilter } = useEntityLinkParams(FILTER_PARAMS);
  const { offset, limit, currentPage, pageSize, totalPages, setPage, setPageSize } = useServerPagination();
  const allSubcatQuery = useSubcategories(offset, limit);
  const filteredSubcatQuery = useSubcategoriesByCategoryId(linkParams.categoryId ?? null, offset, limit);
  const activeSubcatQuery = linkParams.categoryId ? filteredSubcatQuery : allSubcatQuery;
  const { data: subcategoriesResponse, isLoading: subcategoriesLoading } = activeSubcatQuery;
  const { data: categoriesResponse, isLoading: categoriesLoading } = useCategories();
  const createSubcategory = useCreateSubcategory();
  const updateSubcategory = useUpdateSubcategory();
  const deleteSubcategories = useDeleteSubcategories();

  const isLoading = subcategoriesLoading || categoriesLoading;

  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [createOpen, setCreateOpen] = useState(false);
  const [editSubcategory, setEditSubcategory] =
    useState<SubcategoryResponse | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [filterValues, setFilterValues] = useState<FilterValues>({
    categoryId: "all",
  });

  const anyDialogOpen = createOpen || editSubcategory !== null || deleteOpen;

  useEffect(() => {
    function onNewItem() {
      setCreateOpen(true);
    }
    window.addEventListener("shortcut:new-item", onNewItem);
    return () => window.removeEventListener("shortcut:new-item", onNewItem);
  }, []);

  const data = (subcategoriesResponse?.data as SubcategoryResponse[] | undefined) ?? [];
  const serverTotal = subcategoriesResponse?.total ?? 0;

  const categoryList = useMemo(
    () => (categoriesResponse?.data as CategoryResponse[] | undefined) ?? [],
    [categoriesResponse?.data],
  );

  const categoryMap = useMemo(() => {
    const map = new Map<string, string>();
    for (const c of categoryList) {
      map.set(c.id, c.name);
    }
    return map;
  }, [categoryList]);

  const categoryFilterOptions = useMemo(
    () => categoryList.map((c) => c.name),
    [categoryList],
  );

  const filterFields: FilterField[] = useMemo(
    () => [
      {
        type: "select",
        key: "categoryId",
        label: "Category",
        options: categoryFilterOptions,
      },
    ],
    [categoryFilterOptions],
  );

  const filterDefs: FilterDefinition[] = useMemo(
    () => [{ key: "categoryId", type: "select", field: "categoryName" }],
    [],
  );

  const {
    filters: savedFilters,
    save: saveFilter,
    remove: removeFilter,
  } = useSavedFilters("subcategories");

  const { search, setSearch, results, totalCount, clearSearch } =
    useFuzzySearch({ data, config: SEARCH_CONFIG });

  const filteredResults = useMemo(() => {
    const items = results.map((r) => ({
      ...r.item,
      categoryName: categoryMap.get(r.item.categoryId) ?? "",
    }));
    return applyFilters(items, filterDefs, filterValues);
  }, [results, filterValues, categoryMap, filterDefs]);

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
      setSelected(new Set(filteredResults.map((a) => a.id)));
    }
  }

  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items: filteredResults,
    getId: (a) => a.id,
    enabled: !anyDialogOpen,
    onOpen: (a) => setEditSubcategory(a),
    onDelete: () => setDeleteOpen(true),
    onSelectAll: () =>
      setSelected(new Set(filteredResults.map((a) => a.id))),
    onDeselectAll: () => setSelected(new Set()),
    onToggleSelect: (a) => toggleSelect(a.id),
    selected,
  });

  if (isLoading) {
    return <TableSkeleton columns={4} />;
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold tracking-tight">Subcategories</h1>
      <div className="flex items-center justify-between">
        <FuzzySearchInput
          aria-label="Search subcategories"
          value={search}
          onChange={setSearch}
          placeholder="Search subcategories..."
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
          <Button onClick={() => setCreateOpen(true)}>New Subcategory</Button>
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
            entityType: "subcategories",
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
          message={`Showing subcategories for category: ${categoryMap.get(linkParams.categoryId!) ?? linkParams.categoryId}`}
          onClear={clearParams}
        />
      )}

      {filteredResults.length === 0 ? (
        search ? (
          <NoResults
            searchTerm={search}
            onClearSearch={clearSearch}
            onSelectSuggestion={setSearch}
            entityName="subcategories"
          />
        ) : (
          <div className="py-12 text-center text-muted-foreground">
            No subcategories yet. Create one to get started.
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
                        selected.size === filteredResults.length &&
                        filteredResults.length > 0
                      }
                      onChange={toggleAll}
                      className="h-4 w-4 rounded border-gray-300"
                    />
                  </TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Category</TableHead>
                  <TableHead>Description</TableHead>
                  <TableHead>Related</TableHead>
                  <TableHead className="w-24">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredResults.map((subcategory, index) => {
                  const result = matchMap.get(subcategory.id);
                  const matches = result?.matches;
                  return (
                    <TableRow
                      key={subcategory.id}
                      className={`cursor-pointer ${focusedId === subcategory.id ? "bg-accent" : ""}`}
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
                          aria-label={`Select ${subcategory.name}`}
                          checked={selected.has(subcategory.id)}
                          onChange={() => toggleSelect(subcategory.id)}
                          className="h-4 w-4 rounded border-gray-300"
                        />
                      </TableCell>
                      <TableCell>
                        <SearchHighlight
                          text={subcategory.name}
                          indices={getMatchIndices(matches, "name")}
                        />
                      </TableCell>
                      <TableCell>
                        <Link to={`/categories?highlight=${subcategory.categoryId}`} className="text-primary hover:underline">
                          {categoryMap.get(subcategory.categoryId) ?? "Unknown"}
                        </Link>
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {subcategory.description ? (
                          <SearchHighlight
                            text={subcategory.description}
                            indices={getMatchIndices(matches, "description")}
                          />
                        ) : (
                          <span className="italic">--</span>
                        )}
                      </TableCell>
                      <TableCell>
                        <Link to={`/receipt-items?subcategory=${encodeURIComponent(subcategory.name)}`} className="text-sm text-primary hover:underline">
                          Items
                        </Link>
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="icon"
                          aria-label="Edit"
                          onClick={() => setEditSubcategory(subcategory)}
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

      {/* Create Dialog */}
      <Dialog open={createOpen} onOpenChange={setCreateOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Subcategory</DialogTitle>
          </DialogHeader>
          <SubcategoryForm
            mode="create"
            isSubmitting={createSubcategory.isPending}
            onCancel={() => setCreateOpen(false)}
            onSubmit={(values) => {
              createSubcategory.mutate(values, {
                onSuccess: () => setCreateOpen(false),
              });
            }}
          />
        </DialogContent>
      </Dialog>

      {/* Edit Dialog */}
      <Dialog
        open={editSubcategory !== null}
        onOpenChange={(open) => !open && setEditSubcategory(null)}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Subcategory</DialogTitle>
          </DialogHeader>
          {editSubcategory && (
            <SubcategoryForm
              mode="edit"
              defaultValues={{
                name: editSubcategory.name,
                categoryId: editSubcategory.categoryId,
                description: editSubcategory.description ?? "",
              }}
              isSubmitting={updateSubcategory.isPending}
              onCancel={() => setEditSubcategory(null)}
              onSubmit={(values) => {
                updateSubcategory.mutate(
                  { id: editSubcategory.id, ...values },
                  { onSuccess: () => setEditSubcategory(null) },
                );
              }}
            />
          )}
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteOpen} onOpenChange={setDeleteOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Subcategories</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selected.size} subcategory(ies)?
            This action can be undone by restoring.
          </p>
          <div className="flex justify-end gap-2 pt-4">
            <Button variant="outline" onClick={() => setDeleteOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deleteSubcategories.isPending}
              onClick={() => {
                const ids = [...selected];
                setSelected(new Set());
                setDeleteOpen(false);
                deleteSubcategories.mutate(ids);
              }}
            >
              {deleteSubcategories.isPending && <Spinner size="sm" />}
              {deleteSubcategories.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}

export default Subcategories;

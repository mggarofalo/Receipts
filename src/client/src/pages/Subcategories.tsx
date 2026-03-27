import { Fragment, useState, useMemo, useEffect, useCallback } from "react";
import { generateId } from "@/lib/id";
import { Link } from "react-router";
import {
  useSubcategories,
  useSubcategoriesByCategoryId,
  useCreateSubcategory,
  useUpdateSubcategory,
} from "@/hooks/useSubcategories";
import { useCategories } from "@/hooks/useCategories";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useEntityLinkParams } from "@/hooks/useEntityLinkParams";
import { useFuzzySearch } from "@/hooks/useFuzzySearch";
import { useSavedFilters } from "@/hooks/useSavedFilters";
import { useServerPagination } from "@/hooks/useServerPagination";
import { useServerSort } from "@/hooks/useServerSort";
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
import { ChevronDown, ChevronRight, Pencil } from "lucide-react";

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
  const { sortBy, sortDirection, toggleSort } = useServerSort({ defaultSortBy: "name", defaultSortDirection: "asc" });
  const { offset, limit, currentPage, pageSize, totalPages, setPage, setPageSize, resetPage } = useServerPagination();
  const allSubcatQuery = useSubcategories(offset, limit, sortBy, sortDirection);
  const filteredSubcatQuery = useSubcategoriesByCategoryId(linkParams.categoryId ?? null, offset, limit, sortBy, sortDirection);
  const activeSubcatQuery = linkParams.categoryId ? filteredSubcatQuery : allSubcatQuery;
  const { data: subcategoriesData, total: serverTotal, isLoading: subcategoriesLoading } = activeSubcatQuery;
  const { data: categoriesData, isLoading: categoriesLoading } = useCategories();
  const createSubcategory = useCreateSubcategory();
  const updateSubcategory = useUpdateSubcategory();
  const isLoading = subcategoriesLoading || categoriesLoading;

  const [createOpen, setCreateOpen] = useState(false);
  const [editSubcategory, setEditSubcategory] =
    useState<SubcategoryResponse | null>(null);
  const [filterValues, setFilterValues] = useState<FilterValues>({
    categoryId: "all",
  });
  const [expandedCategories, setExpandedCategories] = useState<Set<string>>(
    new Set(),
  );

  const anyDialogOpen = createOpen || editSubcategory !== null;

  useEffect(() => {
    function onNewItem() {
      setCreateOpen(true);
    }
    window.addEventListener("shortcut:new-item", onNewItem);
    return () => window.removeEventListener("shortcut:new-item", onNewItem);
  }, []);

  const handleSort = useCallback((column: string) => {
    toggleSort(column);
    resetPage();
  }, [toggleSort, resetPage]);

  const data = (subcategoriesData as SubcategoryResponse[] | undefined) ?? [];

  const categoryList = useMemo(
    () => (categoriesData as CategoryResponse[] | undefined) ?? [],
    [categoriesData],
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

  const groupedByCategory = useMemo(() => {
    const groups = new Map<string, SubcategoryResponse[]>();
    for (const item of filteredResults) {
      const existing = groups.get(item.categoryId);
      if (existing) {
        existing.push(item);
      } else {
        groups.set(item.categoryId, [item]);
      }
    }
    const sorted = [...groups.entries()].sort((a, b) => {
      const nameA = categoryMap.get(a[0]) ?? "";
      const nameB = categoryMap.get(b[0]) ?? "";
      return nameA.localeCompare(nameB);
    });
    return sorted;
  }, [filteredResults, categoryMap]);

  const visibleSubcategories = useMemo(
    () =>
      groupedByCategory.flatMap(([categoryId, items]) =>
        expandedCategories.has(categoryId) ? items : [],
      ),
    [groupedByCategory, expandedCategories],
  );

  function toggleCategory(categoryId: string) {
    setExpandedCategories((prev) => {
      const next = new Set(prev);
      if (next.has(categoryId)) next.delete(categoryId);
      else next.add(categoryId);
      return next;
    });
  }

  function expandAll() {
    setExpandedCategories(
      new Set(groupedByCategory.map(([categoryId]) => categoryId)),
    );
  }

  function collapseAll() {
    setExpandedCategories(new Set());
  }

  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items: visibleSubcategories,
    getId: (a) => a.id,
    enabled: !anyDialogOpen,
    onOpen: (a) => setEditSubcategory(a),
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
          <Button variant="outline" size="sm" onClick={expandAll}>
            Expand All
          </Button>
          <Button variant="outline" size="sm" onClick={collapseAll}>
            Collapse All
          </Button>
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
            id: generateId(),
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
                  <SortableTableHead column="name" label="Name" currentSortBy={sortBy} currentSortDirection={sortDirection} onToggleSort={handleSort} />
                  <TableHead>Category</TableHead>
                  <TableHead>Description</TableHead>
                  <TableHead>Related</TableHead>
                  <TableHead className="w-24">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {groupedByCategory.map(([categoryId, items]) => {
                  const isExpanded = expandedCategories.has(categoryId);
                  const categoryName =
                    categoryMap.get(categoryId) ?? "Unknown";
                  return (
                    <Fragment key={categoryId}>
                      <TableRow
                        className="cursor-pointer bg-muted/50 hover:bg-muted"
                        onClick={() => toggleCategory(categoryId)}
                        data-testid={`category-header-${categoryId}`}
                      >
                        <TableCell colSpan={5}>
                          <div className="flex items-center gap-2 font-medium">
                            {isExpanded ? (
                              <ChevronDown className="h-4 w-4" />
                            ) : (
                              <ChevronRight className="h-4 w-4" />
                            )}
                            {categoryName}
                            <span className="ml-1 text-xs text-muted-foreground">
                              ({items.length})
                            </span>
                          </div>
                        </TableCell>
                      </TableRow>
                      {isExpanded &&
                        items.map((subcategory) => {
                          const visibleIndex =
                            visibleSubcategories.indexOf(subcategory);
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
                                setFocusedIndex(visibleIndex);
                              }}
                            >
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
                                    indices={getMatchIndices(
                                      matches,
                                      "description",
                                    )}
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
                                  onClick={() =>
                                    setEditSubcategory(subcategory)
                                  }
                                >
                                  <Pencil className="h-4 w-4" />
                                </Button>
                              </TableCell>
                            </TableRow>
                          );
                        })}
                    </Fragment>
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

    </div>
  );
}

export default Subcategories;

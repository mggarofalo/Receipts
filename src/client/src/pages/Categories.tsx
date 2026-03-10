import { useState, useMemo, useEffect } from "react";
import { Link } from "react-router";
import {
  useCategories,
  useCreateCategory,
  useUpdateCategory,
} from "@/hooks/useCategories";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useEntityLinkParams } from "@/hooks/useEntityLinkParams";
import { useFuzzySearch } from "@/hooks/useFuzzySearch";
import { useSavedFilters } from "@/hooks/useSavedFilters";
import { useServerPagination } from "@/hooks/useServerPagination";
import { useServerSort } from "@/hooks/useServerSort";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
import type { FuseSearchConfig } from "@/lib/search";
import { CategoryForm } from "@/components/CategoryForm";
import { FuzzySearchInput } from "@/components/FuzzySearchInput";
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
import { toast } from "sonner";
import { Pencil } from "lucide-react";

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

const HIGHLIGHT_PARAMS = ["highlight"] as const;

function Categories() {
  usePageTitle("Categories");
  const { params: linkParams } = useEntityLinkParams(HIGHLIGHT_PARAMS);
  const { sortBy, sortDirection, toggleSort } = useServerSort({ defaultSortBy: "name", defaultSortDirection: "asc" });
  const { offset, limit, currentPage, pageSize, totalPages, setPage, setPageSize, resetPage } = useServerPagination();
  const { data: categoriesResponse, isLoading } = useCategories(offset, limit, sortBy, sortDirection);
  const createCategory = useCreateCategory();
  const updateCategory = useUpdateCategory();
  const [createOpen, setCreateOpen] = useState(false);
  const [editCategory, setEditCategory] = useState<CategoryResponse | null>(
    null,
  );

  const anyDialogOpen = createOpen || editCategory !== null;

  useEffect(() => {
    function onNewItem() {
      setCreateOpen(true);
    }
    window.addEventListener("shortcut:new-item", onNewItem);
    return () => window.removeEventListener("shortcut:new-item", onNewItem);
  }, []);

  useEffect(() => { resetPage(); }, [sortBy, sortDirection, resetPage]);

  const data = (categoriesResponse?.data as CategoryResponse[] | undefined) ?? [];
  const serverTotal = categoriesResponse?.total ?? 0;
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

  useEffect(() => {
    if (linkParams.highlight && data.length > 0 && !data.some((c) => c.id === linkParams.highlight)) {
      toast.info("The highlighted item is not on this page.");
    }
  }, [linkParams.highlight, data]);

  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items: filteredResults,
    getId: (a) => a.id,
    enabled: !anyDialogOpen,
    onOpen: (a) => setEditCategory(a),
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
        <Button onClick={() => setCreateOpen(true)}>New Category</Button>
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
                  <SortableTableHead column="name" label="Name" currentSortBy={sortBy} currentSortDirection={sortDirection} onToggleSort={toggleSort} />
                  <TableHead>Description</TableHead>
                  <TableHead>Related</TableHead>
                  <TableHead className="w-24">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredResults.map((category, index) => {
                  const result = matchMap.get(category.id);
                  const matches = result?.matches;
                  return (
                    <TableRow
                      key={category.id}
                      className={`cursor-pointer ${focusedId === category.id ? "bg-accent" : ""} ${linkParams.highlight === category.id ? "ring-2 ring-primary" : ""}`}
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
                        <Link to={`/subcategories?categoryId=${category.id}`} className="text-sm text-primary hover:underline">
                          Subcategories
                        </Link>
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="icon"
                          aria-label="Edit"
                          onClick={() => setEditCategory(category)}
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
            <DialogTitle>Create Category</DialogTitle>
          </DialogHeader>
          <CategoryForm
            mode="create"
            isSubmitting={createCategory.isPending}
            onCancel={() => setCreateOpen(false)}
            onSubmit={(values) => {
              createCategory.mutate(values, {
                onSuccess: () => setCreateOpen(false),
              });
            }}
          />
        </DialogContent>
      </Dialog>

      {/* Edit Dialog */}
      <Dialog
        open={editCategory !== null}
        onOpenChange={(open) => !open && setEditCategory(null)}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Category</DialogTitle>
          </DialogHeader>
          {editCategory && (
            <CategoryForm
              mode="edit"
              defaultValues={{
                name: editCategory.name,
                description: editCategory.description ?? "",
              }}
              isSubmitting={updateCategory.isPending}
              onCancel={() => setEditCategory(null)}
              onSubmit={(values) => {
                updateCategory.mutate(
                  { id: editCategory.id, ...values },
                  { onSuccess: () => setEditCategory(null) },
                );
              }}
            />
          )}
        </DialogContent>
      </Dialog>

    </div>
  );
}

export default Categories;

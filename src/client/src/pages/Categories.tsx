import { useState, useMemo, useEffect } from "react";
import { Link } from "react-router";
import {
  useCategories,
  useCreateCategory,
  useUpdateCategory,
  useDeleteCategories,
} from "@/hooks/useCategories";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useEntityLinkParams } from "@/hooks/useEntityLinkParams";
import { useFuzzySearch } from "@/hooks/useFuzzySearch";
import { useSavedFilters } from "@/hooks/useSavedFilters";
import { useServerPagination } from "@/hooks/useServerPagination";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
import type { FuseSearchConfig } from "@/lib/search";
import { CategoryForm } from "@/components/CategoryForm";
import { FuzzySearchInput } from "@/components/FuzzySearchInput";
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
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { TableSkeleton } from "@/components/ui/table-skeleton";
import { Spinner } from "@/components/ui/spinner";
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
  const { offset, limit, currentPage, pageSize, totalPages, setPage, setPageSize } = useServerPagination();
  const { data: categoriesResponse, isLoading } = useCategories(offset, limit);
  const createCategory = useCreateCategory();
  const updateCategory = useUpdateCategory();
  const deleteCategories = useDeleteCategories();

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
    onOpen: (a) => setEditCategory(a),
    onDelete: () => setDeleteOpen(true),
    onSelectAll: () =>
      setSelected(new Set(filteredResults.map((a) => a.id))),
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
                        selected.size === filteredResults.length &&
                        filteredResults.length > 0
                      }
                      onChange={toggleAll}
                      className="h-4 w-4 rounded border-gray-300"
                    />
                  </TableHead>
                  <TableHead>Name</TableHead>
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

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteOpen} onOpenChange={setDeleteOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Categories</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selected.size} category(ies)? This
            action can be undone by restoring.
          </p>
          <div className="flex justify-end gap-2 pt-4">
            <Button variant="outline" onClick={() => setDeleteOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deleteCategories.isPending}
              onClick={() => {
                const ids = [...selected];
                setSelected(new Set());
                setDeleteOpen(false);
                deleteCategories.mutate(ids);
              }}
            >
              {deleteCategories.isPending && <Spinner size="sm" />}
              {deleteCategories.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}

export default Categories;

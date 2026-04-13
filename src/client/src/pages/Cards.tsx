import { useState, useMemo, useEffect, useCallback } from "react";
import { Link } from "react-router";
import {
  useCards,
  useCreateCard,
  useUpdateCard,
  useDeleteCard,
} from "@/hooks/useCards";
import { usePermission } from "@/hooks/usePermission";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useEntityLinkParams } from "@/hooks/useEntityLinkParams";
import { useFuzzySearch } from "@/hooks/useFuzzySearch";
import { useServerPagination } from "@/hooks/useServerPagination";
import { useServerSort } from "@/hooks/useServerSort";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
import type { FuseSearchConfig } from "@/lib/search";
import { CardForm } from "@/components/CardForm";
import { FuzzySearchInput } from "@/components/FuzzySearchInput";
import { SearchHighlight } from "@/components/SearchHighlight";
import { getMatchIndices } from "@/lib/search-highlight";
import { SortableTableHead } from "@/components/SortableTableHead";
import { NoResults } from "@/components/NoResults";
import { Pagination } from "@/components/Pagination";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Switch } from "@/components/ui/switch";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
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
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Info, Pencil } from "lucide-react";

interface CardResponse {
  id: string;
  cardCode: string;
  name: string;
  isActive: boolean;
}

const SEARCH_CONFIG: FuseSearchConfig<CardResponse> = {
  keys: [
    { name: "name", weight: 2 },
    { name: "cardCode", weight: 1 },
  ],
};

const STATUS_STORAGE_KEY = "cards-status-filter";
type StatusFilter = "all" | "true" | "false";

const HIGHLIGHT_PARAMS = ["highlight"] as const;

function Cards() {
  usePageTitle("Cards");
  const { params: linkParams } = useEntityLinkParams(HIGHLIGHT_PARAMS);
  const { sortBy, sortDirection, toggleSort } = useServerSort({ defaultSortBy: "name", defaultSortDirection: "asc" });
  const { offset, limit, currentPage, pageSize, totalPages, setPage, setPageSize, resetPage } = useServerPagination({ sortBy, sortDirection });
  const [statusFilter, setStatusFilter] = useState<StatusFilter>(() => {
    const saved = localStorage.getItem(STATUS_STORAGE_KEY);
    return saved === "all" || saved === "true" || saved === "false" ? saved : "true";
  });
  const isActiveParam = statusFilter === "all" ? undefined : statusFilter === "true";
  const { data: cardsData, total: serverTotal, isLoading } = useCards(offset, limit, sortBy, sortDirection, isActiveParam);
  const createCard = useCreateCard();
  const updateCard = useUpdateCard();
  const deleteCard = useDeleteCard();
  const { isAdmin } = usePermission();
  const [createOpen, setCreateOpen] = useState(false);
  const [editCard, setEditCard] = useState<CardResponse | null>(null);

  const anyDialogOpen = createOpen || editCard !== null;

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

  const handleToggleActive = useCallback((card: CardResponse, checked: boolean) => {
    updateCard.mutate({
      id: card.id,
      cardCode: card.cardCode,
      name: card.name,
      isActive: checked,
    });
  }, [updateCard]);

  const data = (cardsData as CardResponse[] | undefined) ?? [];

  const { search, setSearch, results, totalCount, clearSearch } =
    useFuzzySearch({ data, config: SEARCH_CONFIG });

  function handleStatusChange(value: string) {
    const v = value as StatusFilter;
    setStatusFilter(v);
    localStorage.setItem(STATUS_STORAGE_KEY, v);
    resetPage();
  }

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

  const highlightMissing =
    linkParams.highlight && data.length > 0 && !data.some((a) => a.id === linkParams.highlight);

  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items: filteredResults,
    getId: (a) => a.id,
    enabled: !anyDialogOpen,
    onOpen: (a) => setEditCard(a),
  });

  if (isLoading) {
    return <TableSkeleton columns={4} />;
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold tracking-tight">Cards</h1>
      <div className="flex items-center justify-between">
        <FuzzySearchInput
          aria-label="Search cards"
          value={search}
          onChange={setSearch}
          placeholder="Search cards..."
          resultCount={filteredResults.length}
          totalCount={totalCount}
          className="max-w-sm"
        />
        <Button onClick={() => setCreateOpen(true)}>New Card</Button>
      </div>

      <Tabs value={statusFilter} onValueChange={handleStatusChange}>
        <TabsList>
          <TabsTrigger value="true">Active</TabsTrigger>
          <TabsTrigger value="false">Inactive</TabsTrigger>
          <TabsTrigger value="all">All</TabsTrigger>
        </TabsList>
      </Tabs>

      {highlightMissing && (
        <Alert>
          <Info className="h-4 w-4" />
          <AlertDescription>The highlighted item is not on this page.</AlertDescription>
        </Alert>
      )}

      {filteredResults.length === 0 ? (
        search ? (
          <NoResults
            searchTerm={search}
            onClearSearch={clearSearch}
            onSelectSuggestion={setSearch}
            entityName="cards"
          />
        ) : (
          <div className="py-12 text-center text-muted-foreground">
            No cards yet. Create one to get started.
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
                  <SortableTableHead column="cardCode" label="Card Code" currentSortBy={sortBy} currentSortDirection={sortDirection} onToggleSort={handleSort} />
                  <SortableTableHead column="name" label="Name" currentSortBy={sortBy} currentSortDirection={sortDirection} onToggleSort={handleSort} />
                  <SortableTableHead column="isActive" label="Status" currentSortBy={sortBy} currentSortDirection={sortDirection} onToggleSort={handleSort} />
                  <TableHead>Related</TableHead>
                  <TableHead className="w-24">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredResults.map((card, index) => {
                  const result = matchMap.get(card.id);
                  const matches = result?.matches;
                  return (
                    <TableRow
                      key={card.id}
                      className={`cursor-pointer ${focusedId === card.id ? "bg-accent" : ""} ${linkParams.highlight === card.id ? "ring-2 ring-primary" : ""}`}
                      onClick={(e) => {
                        if ((e.target as HTMLElement).closest("button, input, a, [role='button']")) return;
                        setFocusedIndex(index);
                      }}
                    >
                      <TableCell className="font-mono">
                        <SearchHighlight
                          text={card.cardCode}
                          indices={getMatchIndices(matches, "cardCode")}
                        />
                      </TableCell>
                      <TableCell>
                        <SearchHighlight
                          text={card.name}
                          indices={getMatchIndices(matches, "name")}
                        />
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <Switch
                            checked={card.isActive}
                            onCheckedChange={(checked) => handleToggleActive(card, checked)}
                            aria-label={`Toggle ${card.name} active status`}
                          />
                          <Badge
                            variant={card.isActive ? "default" : "secondary"}
                          >
                            {card.isActive ? "Active" : "Inactive"}
                          </Badge>
                        </div>
                      </TableCell>
                      <TableCell>
                        <Link to="/receipts" className="text-sm text-primary hover:underline">
                          Receipts
                        </Link>
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="icon"
                          aria-label="Edit"
                          onClick={() => setEditCard(card)}
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
            <DialogTitle>Create Card</DialogTitle>
          </DialogHeader>
          <CardForm
            mode="create"
            isSubmitting={createCard.isPending}
            onCancel={() => setCreateOpen(false)}
            onSubmit={(values) => {
              createCard.mutate(values, {
                onSuccess: () => setCreateOpen(false),
              });
            }}
          />
        </DialogContent>
      </Dialog>

      {/* Edit Dialog */}
      <Dialog
        open={editCard !== null}
        onOpenChange={(open) => !open && setEditCard(null)}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Card</DialogTitle>
          </DialogHeader>
          {editCard && (
            <CardForm
              mode="edit"
              defaultValues={{
                cardCode: editCard.cardCode,
                name: editCard.name,
                isActive: editCard.isActive,
              }}
              isSubmitting={updateCard.isPending}
              onCancel={() => setEditCard(null)}
              onSubmit={(values) => {
                updateCard.mutate(
                  { id: editCard.id, ...values },
                  { onSuccess: () => setEditCard(null) },
                );
              }}
              isAdmin={isAdmin()}
              isDeleting={deleteCard.isPending}
              onDelete={() => {
                deleteCard.mutate(editCard.id, {
                  onSuccess: () => setEditCard(null),
                });
              }}
            />
          )}
        </DialogContent>
      </Dialog>

    </div>
  );
}

export default Cards;

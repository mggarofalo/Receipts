import { useState, useMemo } from "react";
import { generateId } from "@/lib/id";
import { useTripByReceiptId } from "@/hooks/useTrips";
import { useReceipts } from "@/hooks/useReceipts";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useFuzzySearch } from "@/hooks/useFuzzySearch";
import { useSavedFilters } from "@/hooks/useSavedFilters";
import { usePagination } from "@/hooks/usePagination";
import type { FuseSearchConfig, FilterDefinition } from "@/lib/search";
import { applyFilters } from "@/lib/search";
import type { FilterValues } from "@/components/FilterPanel";
import { FuzzySearchInput } from "@/components/FuzzySearchInput";
import { FilterPanel } from "@/components/FilterPanel";
import type { FilterField } from "@/components/FilterPanel";
import { SearchHighlight } from "@/components/SearchHighlight";
import { getMatchIndices } from "@/lib/search-highlight";
import { NoResults } from "@/components/NoResults";
import { Pagination } from "@/components/Pagination";
import { ValidationWarnings } from "@/components/ValidationWarnings";
import { BalanceSummaryCard } from "@/components/BalanceSummaryCard";
import { ReceiptItemsCard } from "@/components/ReceiptItemsCard";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { CardSkeleton } from "@/components/ui/card-skeleton";
import { TableSkeleton } from "@/components/ui/table-skeleton";
import { formatCurrency } from "@/lib/format";
import { formatAdjustmentType } from "@/lib/adjustment-types";

interface ReceiptResponse {
  id: string;
  location: string;
  date: string;
  taxAmount: number;
}

const SEARCH_CONFIG: FuseSearchConfig<ReceiptResponse> = {
  keys: [
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

function Trips() {
  usePageTitle("Trips");
  const [receiptId, setReceiptId] = useState<string | null>(null);
  const { data: trip, isLoading: tripLoading, isError } = useTripByReceiptId(receiptId);

  const { data: receiptsResponse, isLoading: receiptsLoading } = useReceipts(0, 10000);

  const [filterValues, setFilterValues] = useState<FilterValues>({});

  const data = useMemo(() => {
    const list = (receiptsResponse?.data as ReceiptResponse[] | undefined) ?? [];
    return [...list].sort(
      (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime(),
    );
  }, [receiptsResponse?.data]);

  const {
    filters: savedFilters,
    save: saveFilter,
    remove: removeFilter,
  } = useSavedFilters("trips");

  const { search, setSearch, results, totalCount, clearSearch } =
    useFuzzySearch({ data, config: SEARCH_CONFIG });

  const filteredResults = useMemo(() => {
    const items = results.map((r) => r.item);
    return applyFilters(items, FILTER_DEFS, filterValues);
  }, [results, filterValues]);

  const { paginatedItems, currentPage, pageSize, totalItems, totalPages, setPage, setPageSize } =
    usePagination({ items: filteredResults });

  const hasActiveFilters = useMemo(() => {
    return Object.values(filterValues).some((v) => {
      if (v == null) return false;
      if (typeof v === "object") {
        return Object.values(v as Record<string, unknown>).some((inner) => inner != null);
      }
      return v !== "" && v !== "all";
    });
  }, [filterValues]);

  const matchMap = useMemo(() => {
    const map = new Map<string, (typeof results)[number]>();
    for (const r of results) {
      map.set(r.item.id, r);
    }
    return map;
  }, [results]);

  const transactionsTotal =
    trip?.transactions?.reduce((sum: number, ta: { transaction: { amount: number } }) => sum + ta.transaction.amount, 0) ??
    0;

  const subtotal = trip?.receipt?.subtotal ?? 0;
  const adjustmentTotal = trip?.receipt?.adjustmentTotal ?? 0;
  const expectedTotal = trip?.receipt?.expectedTotal ?? 0;
  const taxAmount = trip?.receipt?.receipt?.taxAmount ?? 0;

  const allWarnings = [
    ...(trip?.receipt?.warnings ?? []),
    ...(trip?.warnings ?? []),
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">Trips</h1>

      <div className="space-y-4">
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
        </div>

        <FilterPanel
          fields={FILTER_FIELDS}
          values={filterValues}
          onChange={setFilterValues}
          savedFilters={savedFilters}
          onSaveFilter={(name) =>
            saveFilter({
              id: generateId(),
              name,
              entityType: "trips",
              values: filterValues,
              createdAt: new Date().toISOString(),
            })
          }
          onDeleteFilter={removeFilter}
          onLoadFilter={(preset) =>
            setFilterValues(preset.values as FilterValues)
          }
        />

        {receiptsLoading ? (
          <TableSkeleton columns={3} />
        ) : filteredResults.length === 0 ? (
          (search || hasActiveFilters) && totalCount > 0 ? (
            <NoResults
              searchTerm={search || "current filters"}
              onClearSearch={clearSearch}
              onSelectSuggestion={setSearch}
              entityName="receipts"
            />
          ) : (
            <div className="py-12 text-center text-muted-foreground">
              No receipts found.
            </div>
          )
        ) : (
          <>
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Location</TableHead>
                    <TableHead>Date</TableHead>
                    <TableHead className="text-right">Tax Amount</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {paginatedItems.map((receipt) => {
                    const result = matchMap.get(receipt.id);
                    const matches = result?.matches;
                    return (
                      <TableRow
                        key={receipt.id}
                        className={`cursor-pointer ${receiptId === receipt.id ? "bg-accent" : ""}`}
                        onClick={() => setReceiptId(receipt.id)}
                      >
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
      </div>

      {tripLoading && (
        <div className="space-y-4">
          <CardSkeleton lines={1} />
          <CardSkeleton lines={3} />
          <CardSkeleton lines={3} />
        </div>
      )}

      {isError && receiptId && (
        <div className="py-12 text-center text-muted-foreground">
          No trip found for this receipt.
        </div>
      )}

      {trip && (
        <>
          {allWarnings.length > 0 && (
            <ValidationWarnings warnings={allWarnings} />
          )}

          <BalanceSummaryCard
            subtotal={subtotal}
            taxAmount={taxAmount}
            adjustmentTotal={adjustmentTotal}
            expectedTotal={expectedTotal}
            transactionsTotal={transactionsTotal}
            showBalance={trip.transactions.length > 0}
          />

          {/* Receipt Info */}
          <Card>
            <CardHeader>
              <CardTitle>Receipt</CardTitle>
              <CardDescription>
                {trip.receipt.receipt.location} &mdash;{" "}
                {trip.receipt.receipt.date}
              </CardDescription>
            </CardHeader>
          </Card>

          <ReceiptItemsCard
            items={trip.receipt.items}
            subtotal={subtotal}
          />

          {/* Adjustments Table (read-only in trip view) */}
          <Card>
            <CardHeader>
              <CardTitle>
                Adjustments ({trip.receipt.adjustments.length})
              </CardTitle>
            </CardHeader>
            <CardContent>
              {trip.receipt.adjustments.length === 0 ? (
                <p className="text-sm text-muted-foreground">
                  No adjustments for this receipt.
                </p>
              ) : (
                <div className="rounded-md border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Type</TableHead>
                        <TableHead>Description</TableHead>
                        <TableHead className="text-right">Amount</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {trip.receipt.adjustments.map((adj: { id: string; type: string; description?: string | null; amount: number }) => (
                        <TableRow key={adj.id}>
                          <TableCell>
                            <Badge variant="outline">{formatAdjustmentType(adj.type)}</Badge>
                          </TableCell>
                          <TableCell className="text-muted-foreground">
                            {adj.description ?? "\u2014"}
                          </TableCell>
                          <TableCell className="text-right">
                            {formatCurrency(adj.amount)}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                    <TableFooter>
                      <TableRow>
                        <TableCell colSpan={2} className="text-right font-medium">
                          Adjustment Total
                        </TableCell>
                        <TableCell className="text-right font-bold">
                          {formatCurrency(adjustmentTotal)}
                        </TableCell>
                      </TableRow>
                    </TableFooter>
                  </Table>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Transactions Table */}
          <Card>
            <CardHeader>
              <CardTitle>
                Transactions ({trip.transactions.length})
              </CardTitle>
            </CardHeader>
            <CardContent>
              {trip.transactions.length === 0 ? (
                <p className="text-sm text-muted-foreground">
                  No transactions for this receipt.
                </p>
              ) : (
                <div className="rounded-md border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead className="text-right">Amount</TableHead>
                        <TableHead>Date</TableHead>
                        <TableHead>Account Code</TableHead>
                        <TableHead>Account Name</TableHead>
                        <TableHead>Status</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {trip.transactions.map((ta: { transaction: { id: string; amount: number; date: string }; account: { accountCode: string; name: string; isActive: boolean } }) => (
                        <TableRow key={ta.transaction.id}>
                          <TableCell className="text-right">
                            {formatCurrency(ta.transaction.amount)}
                          </TableCell>
                          <TableCell>{ta.transaction.date}</TableCell>
                          <TableCell className="font-mono">
                            {ta.account.accountCode}
                          </TableCell>
                          <TableCell>{ta.account.name}</TableCell>
                          <TableCell>
                            <Badge
                              variant={
                                ta.account.isActive ? "default" : "secondary"
                              }
                            >
                              {ta.account.isActive ? "Active" : "Inactive"}
                            </Badge>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                    <TableFooter>
                      <TableRow>
                        <TableCell className="text-right font-bold">
                          {formatCurrency(transactionsTotal)}
                        </TableCell>
                        <TableCell colSpan={4} />
                      </TableRow>
                    </TableFooter>
                  </Table>
                </div>
              )}
            </CardContent>
          </Card>
        </>
      )}
    </div>
  );
}

export default Trips;

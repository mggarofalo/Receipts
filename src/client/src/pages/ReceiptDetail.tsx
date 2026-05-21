import { useRef, useState } from "react";
import { Link, useParams, Navigate } from "react-router";
import { toast } from "sonner";
import { useTripByReceiptId } from "@/hooks/useTrips";
import { useUpdateReceipt } from "@/hooks/useReceipts";
import { useReceiptYnabSyncStatuses } from "@/hooks/useYnab";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useEnumMetadata } from "@/hooks/useEnumMetadata";
import {
  parseProblemDetails,
  extractFieldErrors,
} from "@/lib/problem-details";
import { ValidationWarnings } from "@/components/ValidationWarnings";
import { BalanceSummaryCard } from "@/components/BalanceSummaryCard";
import { ReceiptItemsCard } from "@/components/ReceiptItemsCard";
import { ReceiptTransactionsCard } from "@/components/ReceiptTransactionsCard";
import {
  ReceiptHeaderForm,
  type ReceiptHeaderFormValues,
} from "@/components/ReceiptHeaderForm";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
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
import { ChangeHistory } from "@/components/ChangeHistory";
import { YnabMemoSyncCard } from "@/components/YnabMemoSyncCard";
import { CardSkeleton } from "@/components/ui/card-skeleton";
import { formatCurrency } from "@/lib/format";
import { YnabPushButton } from "@/components/YnabPushButton";
import { YnabSplitComparisonCard } from "@/components/YnabSplitComparisonCard";
import {
  ReconcileSheet,
  type ReconcileLine,
} from "@/components/ReconcileSheet";
import { Icon, PageHead, YnabChip } from "@/components/primitives";

function ReceiptDetail() {
  usePageTitle("Receipt Detail");
  const { id } = useParams<{ id: string }>();
  const { adjustmentTypeLabels } = useEnumMetadata();

  const { data: trip, isLoading, isError } = useTripByReceiptId(id ?? null);
  const updateReceipt = useUpdateReceipt();
  const { statusMap: ynabStatusMap } = useReceiptYnabSyncStatuses(
    id ? [id] : [],
  );
  const persistedYnabStatus = id ? ynabStatusMap.get(id) : undefined;

  const [editOpen, setEditOpen] = useState(false);
  const [reconcileOpen, setReconcileOpen] = useState(false);
  const [serverErrors, setServerErrors] = useState<Record<string, string>>({});
  // When the reconcile sheet's "Accept transactions" path opens the Edit
  // dialog, it prefills tax to the value that balances the receipt total
  // against the transactions total. null = use the receipt's stored tax.
  const [reconcileTaxPrefill, setReconcileTaxPrefill] = useState<number | null>(
    null,
  );
  // "Edit and balance" scrolls/focuses the line-item table.
  const itemsRef = useRef<HTMLDivElement>(null);

  if (!id) {
    return <Navigate to="/receipts" replace />;
  }

  const transactionsTotal =
    trip?.transactions?.reduce(
      (sum: number, ta) => sum + Number(ta.transaction.amount ?? 0),
      0,
    ) ?? 0;

  const subtotal = Number(trip?.receipt?.subtotal ?? 0);
  const adjustmentTotal = Number(trip?.receipt?.adjustmentTotal ?? 0);
  const expectedTotal = Number(trip?.receipt?.expectedTotal ?? 0);
  const taxAmount = Number(trip?.receipt?.receipt?.taxAmount ?? 0);

  const allWarnings = [
    ...(trip?.receipt?.warnings ?? []),
    ...(trip?.warnings ?? []),
  ].map((w) => ({
    property: w.property,
    message: w.message,
    severity: w.severity != null ? Number(w.severity) : undefined,
  }));

  function handleUpdate(values: ReceiptHeaderFormValues) {
    if (!id) return;
    setServerErrors({});
    updateReceipt.mutate(
      {
        id,
        location: values.location,
        date: values.date,
        taxAmount: values.taxAmount,
      },
      {
        onSuccess: () => setEditOpen(false),
        onError: (error) => {
          const problem = parseProblemDetails(error);
          if (problem) setServerErrors(extractFieldErrors(problem));
        },
      },
    );
  }

  const reconcileLines: ReconcileLine[] = trip
    ? [
        ...trip.receipt.items.map((item) => {
          const amount = Number(item.quantity ?? 0) * Number(item.unitPrice ?? 0);
          const missingCategory = !item.category;
          const zeroAmount = amount === 0;
          return {
            id: `item-${item.id}`,
            kind: "item" as const,
            label: item.description ?? "Untitled item",
            qty: `${Number(item.quantity ?? 0)} × ${formatCurrency(Number(item.unitPrice ?? 0))}`,
            amount,
            flagged: missingCategory || zeroAmount,
            reason: zeroAmount
              ? "zero amount"
              : missingCategory
                ? "uncategorized"
                : undefined,
          };
        }),
        ...trip.receipt.adjustments.map((adj) => ({
          id: `adj-${adj.id}`,
          kind: "adjustment" as const,
          label:
            adj.description ??
            adjustmentTypeLabels[adj.type] ??
            adj.type ??
            "Adjustment",
          qty: adjustmentTypeLabels[adj.type] ?? adj.type ?? "adjustment",
          amount: Number(adj.amount ?? 0),
          flagged: false,
        })),
      ]
    : [];

  const transactionsImbalanced =
    trip != null &&
    trip.transactions.length > 0 &&
    Math.abs(expectedTotal - transactionsTotal) >= 0.005;
  const showReconcile = transactionsImbalanced || allWarnings.length > 0;

  const yChip: "synced" | "pending" | "error" | "none" =
    persistedYnabStatus === "Synced"
      ? "synced"
      : persistedYnabStatus === "Pending"
        ? "pending"
        : persistedYnabStatus === "Failed"
          ? "error"
          : "none";

  return (
    <>
      <PageHead
        title={trip?.receipt?.receipt?.location ?? "Receipt"}
        sub={
          trip
            ? `${trip.receipt.receipt.date} · REC-${id.slice(0, 8).toUpperCase()}`
            : "Loading…"
        }
        actions={
          trip && (
            <>
              <Link to="/receipts" className="btn">
                ← All receipts
              </Link>
              <button
                type="button"
                className="btn"
                onClick={() => {
                  setServerErrors({});
                  setReconcileTaxPrefill(null);
                  setEditOpen(true);
                }}
              >
                <Icon.Edit /> Edit
              </button>
              <YnabChip status={yChip} />
            </>
          )
        }
      />

      {isLoading && (
        <div
          role="status"
          aria-live="polite"
          aria-busy="true"
          style={{ display: "flex", flexDirection: "column", gap: 14 }}
        >
          <span className="sr-only">Loading receipt details…</span>
          <CardSkeleton lines={1} silent />
          <CardSkeleton lines={3} silent />
          <CardSkeleton lines={3} silent />
        </div>
      )}

      {isError && (
        <div className="empty" role="alert">
          <div className="icon-frame">
            <Icon.AlertTriangle />
          </div>
          <h3>Receipt not found</h3>
          <p>No receipt matches this ID. It may have been deleted.</p>
          <div className="actions">
            <Link to="/receipts" className="btn primary">
              Back to receipts
            </Link>
          </div>
        </div>
      )}

      {trip && (
        <div
          style={{ display: "flex", flexDirection: "column", gap: 14 }}
        >
          {(allWarnings.length > 0 || transactionsImbalanced) && (
            <div
              className="warn-banner"
              role="status"
              aria-live="polite"
            >
              <Icon.AlertTriangle className="ico" aria-hidden="true" />
              <div style={{ flex: 1 }}>
                {allWarnings.length > 0 ? (
                  <ValidationWarnings warnings={allWarnings} />
                ) : (
                  <div>
                    Receipt total doesn’t match the linked transactions.
                  </div>
                )}
              </div>
              {showReconcile && (
                <button
                  type="button"
                  className="btn"
                  onClick={() => setReconcileOpen(true)}
                >
                  Reconcile
                </button>
              )}
            </div>
          )}

          <BalanceSummaryCard
            subtotal={subtotal}
            taxAmount={taxAmount}
            adjustmentTotal={adjustmentTotal}
            expectedTotal={expectedTotal}
            transactionsTotal={transactionsTotal}
            showBalance={trip.transactions.length > 0}
          />

          <div
            ref={itemsRef}
            tabIndex={-1}
            aria-label="Line items"
            style={{ outline: "none" }}
          >
            <ReceiptItemsCard
              receiptId={id}
              items={trip.receipt.items.map((i) => ({
                id: i.id,
                receiptItemCode: i.receiptItemCode,
                description: i.description,
                quantity: Number(i.quantity ?? 0),
                unitPrice: Number(i.unitPrice ?? 0),
                category: i.category,
                subcategory: i.subcategory,
                normalizedDescriptionName: i.normalizedDescriptionName,
              }))}
              subtotal={subtotal}
              location={trip.receipt.receipt.location}
            />
          </div>

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
                      {trip.receipt.adjustments.map((adj) => (
                        <TableRow key={adj.id}>
                          <TableCell>
                            <Badge variant="outline">
                              {adjustmentTypeLabels[adj.type] ?? adj.type}
                            </Badge>
                          </TableCell>
                          <TableCell className="text-muted-foreground">
                            {adj.description ?? "—"}
                          </TableCell>
                          <TableCell className="text-right">
                            {formatCurrency(Number(adj.amount ?? 0))}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                    <TableFooter>
                      <TableRow>
                        <TableCell colSpan={2} className="text-right font-medium">
                          Adjustment total
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

          <ReceiptTransactionsCard
            receiptId={id}
            receiptDate={trip.receipt.receipt.date}
            transactions={trip.transactions.map((ta) => ({
              transaction: {
                id: ta.transaction.id,
                amount: Number(ta.transaction.amount ?? 0),
                date: ta.transaction.date,
                cardId: ta.transaction.cardId ?? null,
              },
              account: {
                id: ta.account.id,
                name: ta.account.name,
                isActive: ta.account.isActive ?? true,
              },
            }))}
            transactionsTotal={transactionsTotal}
          />

          <YnabMemoSyncCard receiptId={id} />

          <Card>
            <CardHeader>
              <CardTitle>YNAB sync</CardTitle>
              <CardDescription>
                Push this receipt’s transactions to YNAB with category splits.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <YnabPushButton
                receiptId={id}
                hasTransactions={trip.transactions.length > 0}
                persistedSyncStatus={persistedYnabStatus}
              />
            </CardContent>
          </Card>

          <YnabSplitComparisonCard receiptId={id} />

          <Card>
            <CardHeader>
              <CardTitle>Change history</CardTitle>
            </CardHeader>
            <CardContent>
              <ChangeHistory entityType="Receipt" entityId={id} />
            </CardContent>
          </Card>

          <ReconcileSheet
            open={reconcileOpen}
            onClose={() => setReconcileOpen(false)}
            receiptId={id}
            receiptLabel={trip.receipt.receipt.location}
            receiptDate={trip.receipt.receipt.date}
            receiptTotal={expectedTotal}
            transactionsTotal={transactionsTotal}
            lines={reconcileLines}
            onResolve={({ path }) => {
              if (path === "receipt") {
                // Accept receipt total: nothing to change — the receipt total
                // is already the source of truth.
                toast.success(
                  "Receipt total kept as the source of truth — no changes made.",
                );
              } else if (path === "transactions") {
                // Accept transactions: open Edit with tax prefilled so the
                // receipt total lands on the transactions total. If tax can't
                // absorb the gap (would go negative), fall back to a hint to
                // record an adjustment.
                const targetTax =
                  Math.round(
                    (transactionsTotal - subtotal - adjustmentTotal) * 100,
                  ) / 100;
                setServerErrors({});
                if (targetTax >= 0) {
                  setReconcileTaxPrefill(targetTax);
                  toast.message(
                    "Edit opened with tax set so the receipt total matches the transactions. Review and save.",
                  );
                } else {
                  setReconcileTaxPrefill(null);
                  toast.message(
                    "Open Edit to record an adjustment that balances to the transactions total.",
                  );
                }
                setEditOpen(true);
              } else {
                // Edit and balance: send the user to the line-item table.
                toast.message("Edit the line items below to balance the receipt.");
                // Deferred past the sheet's own focus-restore-on-close.
                setTimeout(() => {
                  itemsRef.current?.scrollIntoView({
                    behavior: "smooth",
                    block: "start",
                  });
                  itemsRef.current?.focus();
                }, 60);
              }
            }}
          />

          <Dialog
            open={editOpen}
            onOpenChange={(open) => {
              setEditOpen(open);
              if (!open) setReconcileTaxPrefill(null);
            }}
          >
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Edit receipt</DialogTitle>
              </DialogHeader>
              <ReceiptHeaderForm
                defaultValues={{
                  location: trip.receipt.receipt.location,
                  date: trip.receipt.receipt.date,
                  taxAmount:
                    reconcileTaxPrefill ??
                    Number(trip.receipt.receipt.taxAmount ?? 0),
                }}
                isSubmitting={updateReceipt.isPending}
                serverErrors={serverErrors}
                onCancel={() => setEditOpen(false)}
                onSubmit={handleUpdate}
              />
            </DialogContent>
          </Dialog>
        </div>
      )}
    </>
  );
}

export default ReceiptDetail;

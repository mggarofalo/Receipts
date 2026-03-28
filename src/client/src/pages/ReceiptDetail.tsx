import { useParams, Navigate } from "react-router";
import { useTripByReceiptId } from "@/hooks/useTrips";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useEnumMetadata } from "@/hooks/useEnumMetadata";
import { ValidationWarnings } from "@/components/ValidationWarnings";
import { BalanceSummaryCard } from "@/components/BalanceSummaryCard";
import { ReceiptItemsCard } from "@/components/ReceiptItemsCard";
import { ReceiptTransactionsCard } from "@/components/ReceiptTransactionsCard";
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
import { ChangeHistory } from "@/components/ChangeHistory";
import { CardSkeleton } from "@/components/ui/card-skeleton";
import { formatCurrency } from "@/lib/format";

function ReceiptDetail() {
  usePageTitle("Receipt Detail");
  const { id } = useParams<{ id: string }>();
  const { adjustmentTypeLabels } = useEnumMetadata();

  const { data: trip, isLoading, isError } = useTripByReceiptId(id ?? null);

  if (!id) {
    return <Navigate to="/receipts" replace />;
  }

  const transactionsTotal =
    trip?.transactions?.reduce(
      (sum: number, ta: { transaction: { amount: number } }) =>
        sum + ta.transaction.amount,
      0,
    ) ?? 0;

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
      <h1 className="text-2xl font-semibold tracking-tight">Receipt Details</h1>

      {isLoading && (
        <div role="status" aria-live="polite" className="space-y-4">
          <span className="sr-only">Loading receipt details...</span>
          <CardSkeleton lines={1} />
          <CardSkeleton lines={3} />
          <CardSkeleton lines={3} />
        </div>
      )}

      {isError && (
        <div role="alert" className="py-12 text-center text-muted-foreground">
          No receipt found for this ID.
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
            receiptId={id}
            items={trip.receipt.items}
            subtotal={subtotal}
          />

          {/* Adjustments Table (read-only) */}
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
                      {trip.receipt.adjustments.map(
                        (adj: {
                          id: string;
                          type: string;
                          description?: string | null;
                          amount: number;
                        }) => (
                          <TableRow key={adj.id}>
                            <TableCell>
                              <Badge variant="outline">
                                {adjustmentTypeLabels[adj.type] ?? adj.type}
                              </Badge>
                            </TableCell>
                            <TableCell className="text-muted-foreground">
                              {adj.description ?? "\u2014"}
                            </TableCell>
                            <TableCell className="text-right">
                              {formatCurrency(adj.amount)}
                            </TableCell>
                          </TableRow>
                        ),
                      )}
                    </TableBody>
                    <TableFooter>
                      <TableRow>
                        <TableCell
                          colSpan={2}
                          className="text-right font-medium"
                        >
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

          <ReceiptTransactionsCard
            receiptId={id}
            transactions={trip.transactions}
            transactionsTotal={transactionsTotal}
          />

          <Card>
            <CardHeader>
              <CardTitle>Change History</CardTitle>
            </CardHeader>
            <CardContent>
              <ChangeHistory entityType="Receipt" entityId={id} />
            </CardContent>
          </Card>
        </>
      )}
    </div>
  );
}

export default ReceiptDetail;

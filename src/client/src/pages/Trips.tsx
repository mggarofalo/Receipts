import { useState, useMemo } from "react";
import { useTripByReceiptId } from "@/hooks/useTrips";
import { useReceipts } from "@/hooks/useReceipts";
import { receiptToOption } from "@/lib/combobox-options";
import { usePageTitle } from "@/hooks/usePageTitle";
import { ValidationWarnings } from "@/components/ValidationWarnings";
import { BalanceSummaryCard } from "@/components/BalanceSummaryCard";
import { ReceiptItemsCard } from "@/components/ReceiptItemsCard";
import { Combobox } from "@/components/ui/combobox";
import { Label } from "@/components/ui/label";
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
import { formatCurrency } from "@/lib/format";

function Trips() {
  usePageTitle("Trips");
  const [receiptId, setReceiptId] = useState<string | null>(null);
  const { data: trip, isLoading, isError } = useTripByReceiptId(receiptId);
  const { data: receipts, isLoading: receiptsLoading } = useReceipts();

  const receiptOptions = useMemo(
    () =>
      ((receipts as { id: string; description?: string | null; location: string; date: string }[] | undefined) ?? []).map(receiptToOption),
    [receipts],
  );

  const transactionsTotal =
    trip?.transactions?.reduce((sum: number, ta: { transaction: { amount: number } }) => sum + ta.transaction.amount, 0) ??
    0;

  // Balance equation values from the server-computed response
  const subtotal = trip?.receipt?.subtotal ?? 0;
  const adjustmentTotal = trip?.receipt?.adjustmentTotal ?? 0;
  const expectedTotal = trip?.receipt?.expectedTotal ?? 0;
  const taxAmount = trip?.receipt?.receipt?.taxAmount ?? 0;

  // Combine warnings from both receipt and trip levels
  const allWarnings = [
    ...(trip?.receipt?.warnings ?? []),
    ...(trip?.warnings ?? []),
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">Trips</h1>
      <div className="max-w-md space-y-2">
        <Label>Select a Receipt</Label>
        <Combobox
          options={receiptOptions}
          value={receiptId ?? ""}
          onValueChange={(value) => setReceiptId(value || null)}
          placeholder="Search for a receipt..."
          searchPlaceholder="Search receipts..."
          emptyMessage="No receipts found."
          loading={receiptsLoading}
        />
      </div>

      {isLoading && (
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
            <CardContent>
              {trip.receipt.receipt.description && (
                <p className="text-sm text-muted-foreground">
                  {trip.receipt.receipt.description}
                </p>
              )}
            </CardContent>
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
                            <Badge variant="outline">{adj.type}</Badge>
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

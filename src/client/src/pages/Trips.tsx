import { useState, useMemo } from "react";
import { useTripByReceiptId } from "@/hooks/useTrips";
import { useReceipts } from "@/hooks/useReceipts";
import { receiptToOption } from "@/lib/combobox-options";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
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

  const itemsTotal =
    trip?.receipt?.items?.reduce(
      (sum: number, item: { quantity: number; unitPrice: number }) => sum + item.quantity * item.unitPrice,
      0,
    ) ?? 0;

  const transactionsTotal =
    trip?.transactions?.reduce((sum: number, ta: { transaction: { amount: number } }) => sum + ta.transaction.amount, 0) ??
    0;

  const tripItems = trip?.receipt?.items ?? [];
  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items: tripItems as { id: string }[],
    getId: (item: { id: string }) => item.id,
    enabled: tripItems.length > 0,
  });

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
          {/* Summary Cards */}
          <div className="grid grid-cols-3 gap-4">
            <Card>
              <CardHeader className="pb-2">
                <CardDescription>Items Total</CardDescription>
                <CardTitle>{formatCurrency(itemsTotal)}</CardTitle>
              </CardHeader>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardDescription>Transactions Total</CardDescription>
                <CardTitle>{formatCurrency(transactionsTotal)}</CardTitle>
              </CardHeader>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardDescription>Tax Amount</CardDescription>
                <CardTitle>
                  {formatCurrency(trip.receipt.receipt.taxAmount)}
                </CardTitle>
              </CardHeader>
            </Card>
          </div>

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

          {/* Items Table */}
          <Card>
            <CardHeader>
              <CardTitle>
                Items ({trip.receipt.items.length})
              </CardTitle>
            </CardHeader>
            <CardContent>
              {trip.receipt.items.length === 0 ? (
                <p className="text-sm text-muted-foreground">
                  No items for this receipt.
                </p>
              ) : (
                <div className="rounded-md border" ref={tableRef}>
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Code</TableHead>
                        <TableHead>Description</TableHead>
                        <TableHead className="text-right">Qty</TableHead>
                        <TableHead className="text-right">
                          Unit Price
                        </TableHead>
                        <TableHead className="text-right">Total</TableHead>
                        <TableHead>Category</TableHead>
                        <TableHead>Subcategory</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {trip.receipt.items.map((item: { id: string; receiptItemCode: string; description: string; quantity: number; unitPrice: number; category: string; subcategory: string }, index: number) => (
                        <TableRow
                          key={item.id}
                          className={`cursor-pointer ${focusedId === item.id ? "bg-accent" : ""}`}
                          onClick={(e) => {
                            if ((e.target as HTMLElement).closest("button, input, a, [role='button']")) return;
                            setFocusedIndex(index);
                          }}
                        >
                          <TableCell className="font-mono">
                            {item.receiptItemCode}
                          </TableCell>
                          <TableCell>{item.description}</TableCell>
                          <TableCell className="text-right">
                            {item.quantity}
                          </TableCell>
                          <TableCell className="text-right">
                            {formatCurrency(item.unitPrice)}
                          </TableCell>
                          <TableCell className="text-right">
                            {formatCurrency(item.quantity * item.unitPrice)}
                          </TableCell>
                          <TableCell>{item.category}</TableCell>
                          <TableCell>{item.subcategory}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                    <TableFooter>
                      <TableRow>
                        <TableCell colSpan={4} className="text-right font-medium">
                          Grand Total
                        </TableCell>
                        <TableCell className="text-right font-bold">
                          {formatCurrency(itemsTotal)}
                        </TableCell>
                        <TableCell colSpan={2} />
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

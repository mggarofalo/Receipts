import { useState } from "react";
import { useTripByReceiptId } from "@/hooks/useTrips";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(amount);
}

function Trips() {
  const [inputId, setInputId] = useState("");
  const [receiptId, setReceiptId] = useState<string | null>(null);
  const { data: trip, isLoading, isError } = useTripByReceiptId(receiptId);

  function handleLookup() {
    const trimmed = inputId.trim();
    if (trimmed) setReceiptId(trimmed);
  }

  const itemsTotal =
    trip?.receipt?.items?.reduce(
      (sum, item) => sum + item.quantity * item.unitPrice,
      0,
    ) ?? 0;

  const transactionsTotal =
    trip?.transactions?.reduce((sum, ta) => sum + ta.transaction.amount, 0) ??
    0;

  return (
    <div className="space-y-6">
      <div className="flex items-end gap-4">
        <div className="flex-1 max-w-md space-y-2">
          <Label htmlFor="receiptId">Receipt ID</Label>
          <Input
            id="receiptId"
            placeholder="Enter receipt UUID..."
            value={inputId}
            onChange={(e) => setInputId(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && handleLookup()}
          />
        </div>
        <Button onClick={handleLookup} disabled={!inputId.trim()}>
          Look Up Trip
        </Button>
      </div>

      {isLoading && (
        <div className="space-y-4">
          <div className="h-32 animate-pulse rounded bg-muted" />
          <div className="h-48 animate-pulse rounded bg-muted" />
          <div className="h-48 animate-pulse rounded bg-muted" />
        </div>
      )}

      {isError && receiptId && (
        <div className="py-12 text-center text-muted-foreground">
          No trip found for this receipt ID.
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
                <div className="rounded-md border">
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
                      {trip.receipt.items.map((item) => (
                        <TableRow key={item.id}>
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
                      {trip.transactions.map((ta) => (
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

import { useState } from "react";
import { useReceiptWithItems } from "@/hooks/useAggregates";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { ChangeHistory } from "@/components/ChangeHistory";
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

function ReceiptDetail() {
  const [inputId, setInputId] = useState("");
  const [receiptId, setReceiptId] = useState<string | null>(null);
  const { data, isLoading, isError } = useReceiptWithItems(receiptId);

  function handleLookup() {
    const trimmed = inputId.trim();
    if (trimmed) setReceiptId(trimmed);
  }

  const grandTotal =
    data?.items?.reduce(
      (sum, item) => sum + item.quantity * item.unitPrice,
      0,
    ) ?? 0;

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
          Look Up
        </Button>
      </div>

      {isLoading && (
        <div className="space-y-4">
          <div className="h-32 animate-pulse rounded bg-muted" />
          <div className="h-48 animate-pulse rounded bg-muted" />
        </div>
      )}

      {isError && receiptId && (
        <div className="py-12 text-center text-muted-foreground">
          No receipt found for this ID.
        </div>
      )}

      {data && (
        <>
          <Card>
            <CardHeader>
              <CardTitle>Receipt</CardTitle>
              <CardDescription>
                {data.receipt.location} &mdash; {data.receipt.date}
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-2">
              {data.receipt.description && (
                <p className="text-sm">{data.receipt.description}</p>
              )}
              <p className="text-sm text-muted-foreground">
                Tax: {formatCurrency(data.receipt.taxAmount)}
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Items ({data.items.length})</CardTitle>
            </CardHeader>
            <CardContent>
              {data.items.length === 0 ? (
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
                        <TableHead className="text-right">Unit Price</TableHead>
                        <TableHead className="text-right">Total</TableHead>
                        <TableHead>Category</TableHead>
                        <TableHead>Subcategory</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {data.items.map((item) => (
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
                        <TableCell
                          colSpan={4}
                          className="text-right font-medium"
                        >
                          Grand Total
                        </TableCell>
                        <TableCell className="text-right font-bold">
                          {formatCurrency(grandTotal)}
                        </TableCell>
                        <TableCell colSpan={2} />
                      </TableRow>
                    </TableFooter>
                  </Table>
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Change History</CardTitle>
            </CardHeader>
            <CardContent>
              <ChangeHistory entityType="Receipt" entityId={receiptId!} />
            </CardContent>
          </Card>
        </>
      )}
    </div>
  );
}

export default ReceiptDetail;

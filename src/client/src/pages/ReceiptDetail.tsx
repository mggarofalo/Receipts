import { useState } from "react";
import { useReceiptWithItems } from "@/hooks/useAggregates";
import { usePageTitle } from "@/hooks/usePageTitle";
import { ValidationWarnings } from "@/components/ValidationWarnings";
import { BalanceSummaryCard } from "@/components/BalanceSummaryCard";
import { ReceiptItemsCard } from "@/components/ReceiptItemsCard";
import { AdjustmentsCard } from "@/components/AdjustmentsCard";
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
import { CardSkeleton } from "@/components/ui/card-skeleton";
import { formatCurrency } from "@/lib/format";

function ReceiptDetail() {
  usePageTitle("Receipt Detail");
  const [inputId, setInputId] = useState("");
  const [receiptId, setReceiptId] = useState<string | null>(null);
  const { data, isLoading, isError } = useReceiptWithItems(receiptId);

  function handleLookup() {
    const trimmed = inputId.trim();
    if (trimmed) setReceiptId(trimmed);
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">Receipt Details</h1>
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
          <CardSkeleton lines={2} />
          <CardSkeleton lines={4} />
        </div>
      )}

      {isError && receiptId && (
        <div className="py-12 text-center text-muted-foreground">
          No receipt found for this ID.
        </div>
      )}

      {data && (
        <>
          {data.warnings && data.warnings.length > 0 && (
            <ValidationWarnings warnings={data.warnings} />
          )}

          <Card>
            <CardHeader>
              <CardTitle>Receipt</CardTitle>
              <CardDescription>
                {data.receipt.location} &mdash; {data.receipt.date}
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-2">
              <p className="text-sm text-muted-foreground">
                Tax: {formatCurrency(data.receipt.taxAmount)}
              </p>
            </CardContent>
          </Card>

          <BalanceSummaryCard
            subtotal={data.subtotal}
            taxAmount={data.receipt.taxAmount}
            adjustmentTotal={data.adjustmentTotal}
            expectedTotal={data.expectedTotal}
          />

          <ReceiptItemsCard
            items={data.items}
            subtotal={data.subtotal}
          />

          <AdjustmentsCard
            receiptId={receiptId!}
            adjustments={data.adjustments}
            adjustmentTotal={data.adjustmentTotal}
          />

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

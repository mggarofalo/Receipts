import { useSearchParams, Navigate } from "react-router";
import { useReceiptWithItems } from "@/hooks/useAggregates";
import { usePageTitle } from "@/hooks/usePageTitle";
import { ValidationWarnings } from "@/components/ValidationWarnings";
import { BalanceSummaryCard } from "@/components/BalanceSummaryCard";
import { ReceiptItemsCard } from "@/components/ReceiptItemsCard";
import { AdjustmentsCard } from "@/components/AdjustmentsCard";
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
  const [searchParams] = useSearchParams();
  const id = searchParams.get("id");

  const { data, isLoading, isError } = useReceiptWithItems(id);

  if (!id) {
    return <Navigate to="/receipts" replace />;
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">Receipt Details</h1>

      {isLoading && (
        <div className="space-y-4">
          <CardSkeleton lines={2} />
          <CardSkeleton lines={4} />
        </div>
      )}

      {isError && (
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
            receiptId={id}
            adjustments={data.adjustments}
            adjustmentTotal={data.adjustmentTotal}
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

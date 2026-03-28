import { formatCurrency } from "@/lib/format";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";

interface BalanceSidebarProps {
  subtotal: number;
  taxAmount: number;
  transactionTotal: number;
  isSubmitting: boolean;
  onSubmit: () => void;
  onCancel: () => void;
}

export function BalanceSidebar({
  subtotal,
  taxAmount,
  transactionTotal,
  isSubmitting,
  onSubmit,
  onCancel,
}: BalanceSidebarProps) {
  const expectedTotal = subtotal + taxAmount;
  const balanceDiff = Math.abs(expectedTotal - transactionTotal);
  const isBalanced = balanceDiff < 0.01;

  return (
    <div className="sticky top-6 space-y-4">
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-lg">Balance</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-y-2 text-sm">
            <span className="text-muted-foreground">Subtotal</span>
            <span className="text-right">{formatCurrency(subtotal)}</span>
            <span className="text-muted-foreground">Tax</span>
            <span className="text-right">{formatCurrency(taxAmount)}</span>
            <Separator className="col-span-2 my-1" />
            <span className="font-medium">Expected Total</span>
            <span className="text-right font-medium">
              {formatCurrency(expectedTotal)}
            </span>
            <span className="font-medium">Transaction Total</span>
            <span className="text-right font-medium">
              {formatCurrency(transactionTotal)}
            </span>
          </div>
          <div className="mt-3 text-right">
            <Badge variant={isBalanced ? "default" : "destructive"}>
              {isBalanced
                ? "Balanced"
                : `Unbalanced by ${formatCurrency(balanceDiff)}`}
            </Badge>
          </div>
        </CardContent>
      </Card>

      <div className="space-y-2">
        <Tooltip>
          <TooltipTrigger asChild>
            <span className="inline-block w-full">
              <Button
                className={`w-full ${!isBalanced ? "pointer-events-none" : ""}`}
                onClick={onSubmit}
                disabled={isSubmitting || !isBalanced}
              >
                {isSubmitting ? "Submitting..." : "Submit Receipt"}
              </Button>
            </span>
          </TooltipTrigger>
          {!isBalanced && (
            <TooltipContent>
              <p>
                Receipt is unbalanced by {formatCurrency(balanceDiff)}. Adjust
                transactions or line items so totals match.
              </p>
            </TooltipContent>
          )}
        </Tooltip>
        <Button variant="ghost" className="w-full" onClick={onCancel}>
          Cancel
        </Button>
      </div>
    </div>
  );
}

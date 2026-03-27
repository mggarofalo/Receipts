import { useMemo } from "react";
import { useAccounts } from "@/hooks/useAccounts";
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
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Pencil } from "lucide-react";
import type { WizardState } from "./wizardReducer";

interface Step4Props {
  state: WizardState;
  onBack: () => void;
  onEditStep: (step: number) => void;
  onSubmit: () => void;
  isSubmitting: boolean;
}

export function Step4Review({
  state,
  onBack,
  onEditStep,
  onSubmit,
  isSubmitting,
}: Step4Props) {
  const { data: accounts } = useAccounts();

  const accountNameMap = useMemo(() => {
    const map = new Map<string, string>();
    for (const acct of (accounts as { id: string; name: string }[] | undefined) ?? []) {
      map.set(acct.id, acct.name);
    }
    return map;
  }, [accounts]);

  const subtotal = useMemo(
    () =>
      state.items.reduce(
        (sum, item) => sum + item.quantity * item.unitPrice,
        0,
      ),
    [state.items],
  );

  const transactionTotal = useMemo(
    () => state.transactions.reduce((sum, t) => sum + t.amount, 0),
    [state.transactions],
  );

  const expectedTotal = subtotal + state.receipt.taxAmount;
  const balanceDiff = Math.abs(expectedTotal - transactionTotal);
  const isBalanced = balanceDiff < 0.01;

  return (
    <div className="space-y-6">
      {/* Trip Details */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="text-lg">Trip Details</CardTitle>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onEditStep(0)}
            >
              <Pencil className="mr-1 h-3 w-3" />
              Edit
            </Button>
          </div>
        </CardHeader>
        <CardContent className="grid grid-cols-3 gap-4 text-sm">
          <div>
            <span className="text-muted-foreground">Location</span>
            <p className="font-medium">{state.receipt.location}</p>
          </div>
          <div>
            <span className="text-muted-foreground">Date</span>
            <p className="font-medium">{state.receipt.date}</p>
          </div>
          <div>
            <span className="text-muted-foreground">Tax</span>
            <p className="font-medium">
              {formatCurrency(state.receipt.taxAmount)}
            </p>
          </div>
        </CardContent>
      </Card>

      {/* Transactions */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="text-lg">
              Transactions ({state.transactions.length})
            </CardTitle>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onEditStep(1)}
            >
              <Pencil className="mr-1 h-3 w-3" />
              Edit
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Account</TableHead>
                <TableHead>Amount</TableHead>
                <TableHead>Date</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {state.transactions.map((txn) => (
                <TableRow key={txn.id}>
                  <TableCell>
                    {accountNameMap.get(txn.accountId) ?? txn.accountId}
                  </TableCell>
                  <TableCell>{formatCurrency(txn.amount)}</TableCell>
                  <TableCell>{txn.date}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
          <p className="mt-2 text-right text-sm font-medium">
            Total: {formatCurrency(transactionTotal)}
          </p>
        </CardContent>
      </Card>

      {/* Items */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="text-lg">
              Line Items ({state.items.length})
            </CardTitle>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onEditStep(2)}
            >
              <Pencil className="mr-1 h-3 w-3" />
              Edit
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Description</TableHead>
                <TableHead>Qty</TableHead>
                <TableHead>Unit Price</TableHead>
                <TableHead>Line Total</TableHead>
                <TableHead>Category</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {state.items.map((item) => (
                <TableRow key={item.id}>
                  <TableCell>{item.description}</TableCell>
                  <TableCell>{item.quantity}</TableCell>
                  <TableCell>{formatCurrency(item.unitPrice)}</TableCell>
                  <TableCell>
                    {formatCurrency(item.quantity * item.unitPrice)}
                  </TableCell>
                  <TableCell>
                    {item.category}
                    {item.subcategory ? ` / ${item.subcategory}` : ""}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Balance Summary */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Balance</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-y-2 text-sm">
            <span className="text-muted-foreground">Subtotal</span>
            <span className="text-right">{formatCurrency(subtotal)}</span>
            <span className="text-muted-foreground">Tax</span>
            <span className="text-right">
              {formatCurrency(state.receipt.taxAmount)}
            </span>
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

      <div className="flex justify-between pt-4">
        <Button variant="outline" onClick={onBack}>
          Back
        </Button>
        <Tooltip>
          <TooltipTrigger asChild>
            <span className="inline-block">
              <Button
                onClick={onSubmit}
                disabled={isSubmitting || !isBalanced}
                className={!isBalanced ? "pointer-events-none" : ""}
              >
                {isSubmitting ? "Submitting..." : "Submit Receipt"}
              </Button>
            </span>
          </TooltipTrigger>
          {!isBalanced && (
            <TooltipContent>
              <p>
                Receipt is unbalanced by {formatCurrency(balanceDiff)}.
                Adjust transactions or line items so totals match.
              </p>
            </TooltipContent>
          )}
        </Tooltip>
      </div>
    </div>
  );
}

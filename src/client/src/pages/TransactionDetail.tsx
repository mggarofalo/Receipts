import { useState } from "react";
import {
  useTransactionAccount,
  useTransactionAccountsByReceiptId,
} from "@/hooks/useAggregates";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
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
import { ChangeHistory } from "@/components/ChangeHistory";
import { CardSkeleton } from "@/components/ui/card-skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { formatCurrency } from "@/lib/format";

type LookupMode = "receipt" | "transaction";

function TransactionDetail() {
  usePageTitle("Transaction Detail");
  const [mode, setMode] = useState<LookupMode>("receipt");
  const [inputId, setInputId] = useState("");
  const [receiptId, setReceiptId] = useState<string | null>(null);
  const [transactionId, setTransactionId] = useState<string | null>(null);

  const byReceipt = useTransactionAccountsByReceiptId(receiptId);
  const byTransaction = useTransactionAccount(transactionId);

  function handleLookup() {
    const trimmed = inputId.trim();
    if (!trimmed) return;
    if (mode === "receipt") {
      setTransactionId(null);
      setReceiptId(trimmed);
    } else {
      setReceiptId(null);
      setTransactionId(trimmed);
    }
  }

  const isLoading = byReceipt.isLoading || byTransaction.isLoading;
  const isError =
    (receiptId && byReceipt.isError) ||
    (transactionId && byTransaction.isError);

  const items =
    mode === "receipt" && byReceipt.data
      ? (byReceipt.data as {
          transaction: { id: string; amount: number; date: string };
          account: {
            id: string;
            accountCode: string;
            name: string;
            isActive: boolean;
          };
        }[])
      : mode === "transaction" && byTransaction.data
        ? [
            byTransaction.data as {
              transaction: { id: string; amount: number; date: string };
              account: {
                id: string;
                accountCode: string;
                name: string;
                isActive: boolean;
              };
            },
          ]
        : [];

  const total = items.reduce((sum, ta) => sum + ta.transaction.amount, 0);

  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items,
    getId: (ta) => ta.transaction.id,
    enabled: items.length > 0,
  });

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">Transaction Details</h1>
      <div className="flex items-end gap-4">
        <div className="space-y-2">
          <Label>Lookup By</Label>
          <div className="flex gap-2">
            <Button
              variant={mode === "receipt" ? "default" : "outline"}
              size="sm"
              onClick={() => setMode("receipt")}
            >
              Receipt ID
            </Button>
            <Button
              variant={mode === "transaction" ? "default" : "outline"}
              size="sm"
              onClick={() => setMode("transaction")}
            >
              Transaction ID
            </Button>
          </div>
        </div>
        <div className="flex-1 max-w-md space-y-2">
          <Label htmlFor="lookupId">
            {mode === "receipt" ? "Receipt" : "Transaction"} ID
          </Label>
          <Input
            id="lookupId"
            placeholder={`Enter ${mode} UUID...`}
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
          <CardSkeleton lines={1} />
          <CardSkeleton lines={4} />
        </div>
      )}

      {isError && (
        <div className="py-12 text-center text-muted-foreground">
          No transaction-account data found.
        </div>
      )}

      {items.length > 0 && (
        <>
          <Card>
            <CardHeader className="pb-2">
              <CardDescription>Total Amount</CardDescription>
              <CardTitle>{formatCurrency(total)}</CardTitle>
            </CardHeader>
          </Card>

          <div className="rounded-md border" ref={tableRef}>
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
                {items.map((ta, index) => (
                  <TableRow
                    key={ta.transaction.id}
                    className={`cursor-pointer ${
                      focusedId === ta.transaction.id ? "bg-accent" : ""
                    }`}
                    onClick={(e) => {
                      if ((e.target as HTMLElement).closest("button, input, a, [role='button']")) return;
                      setFocusedIndex(index);
                    }}
                  >
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
                        variant={ta.account.isActive ? "default" : "secondary"}
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
                    {formatCurrency(total)}
                  </TableCell>
                  <TableCell colSpan={4} />
                </TableRow>
              </TableFooter>
            </Table>
          </div>

          {mode === "transaction" && transactionId && byTransaction.data && (
            <Card>
              <CardHeader>
                <CardTitle>Change History</CardTitle>
              </CardHeader>
              <CardContent>
                <ChangeHistory
                  entityType="Transaction"
                  entityId={transactionId}
                />
              </CardContent>
            </Card>
          )}
        </>
      )}
    </div>
  );
}

export default TransactionDetail;

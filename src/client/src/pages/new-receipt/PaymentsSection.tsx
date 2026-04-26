import { useCallback } from "react";
import { generateId } from "@/lib/id";
import { formatCurrency } from "@/lib/format";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { CurrencyInput } from "@/components/ui/currency-input";
import { ConfidenceIndicator } from "@/pages/scan-receipt/ConfidenceIndicator";
import type { ReceiptConfidenceMap } from "@/pages/scan-receipt/types";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Plus, Trash2 } from "lucide-react";

export interface ReceiptPayment {
  id: string;
  method: string;
  amount: number;
  lastFour: string;
}

interface PaymentsSectionProps {
  payments: ReceiptPayment[];
  onChange: (payments: ReceiptPayment[]) => void;
  /**
   * Per-payment confidence levels keyed by index. Used to surface
   * low-confidence highlights when the receipt was populated from a scan.
   */
  confidence?: ReceiptConfidenceMap["payments"];
}

const LAST_FOUR_PATTERN = /^\d{0,4}$/;

export function PaymentsSection({
  payments,
  onChange,
  confidence,
}: PaymentsSectionProps) {
  const handleAdd = useCallback(() => {
    onChange([
      ...payments,
      { id: generateId(), method: "", amount: 0, lastFour: "" },
    ]);
  }, [payments, onChange]);

  const handleRemove = useCallback(
    (id: string) => {
      onChange(payments.filter((p) => p.id !== id));
    },
    [payments, onChange],
  );

  const handleField = useCallback(
    <K extends keyof Omit<ReceiptPayment, "id">>(
      id: string,
      field: K,
      value: ReceiptPayment[K],
    ) => {
      onChange(
        payments.map((p) => (p.id === id ? { ...p, [field]: value } : p)),
      );
    },
    [payments, onChange],
  );

  const total = payments.reduce((sum, p) => sum + (p.amount || 0), 0);

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <CardTitle className="text-lg">Detected Payments</CardTitle>
          <span className="text-sm text-muted-foreground">
            Total: {formatCurrency(total)}
          </span>
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        {payments.length === 0 ? (
          <p className="text-sm text-muted-foreground">
            No payments detected on the receipt.
          </p>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Method</TableHead>
                <TableHead>Amount</TableHead>
                <TableHead>Last 4</TableHead>
                <TableHead className="w-12">
                  <span className="sr-only">Actions</span>
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {payments.map((payment, index) => {
                const fieldConfidence = confidence?.[index];
                const methodId = `payment-${payment.id}-method`;
                const amountId = `payment-${payment.id}-amount`;
                const lastFourId = `payment-${payment.id}-lastFour`;
                return (
                  <TableRow key={payment.id}>
                    <TableCell>
                      <Label htmlFor={methodId} className="sr-only">
                        Payment method
                      </Label>
                      <div className="flex items-center gap-2">
                        <Input
                          id={methodId}
                          value={payment.method}
                          onChange={(e) =>
                            handleField(payment.id, "method", e.target.value)
                          }
                          placeholder="e.g. Visa, Cash"
                          className="h-8"
                        />
                        <ConfidenceIndicator
                          confidence={fieldConfidence?.method}
                        />
                      </div>
                    </TableCell>
                    <TableCell>
                      <Label htmlFor={amountId} className="sr-only">
                        Payment amount
                      </Label>
                      <div className="flex items-center gap-2">
                        <CurrencyInput
                          id={amountId}
                          value={payment.amount}
                          onChange={(v) =>
                            handleField(payment.id, "amount", v)
                          }
                          className="h-8"
                        />
                        <ConfidenceIndicator
                          confidence={fieldConfidence?.amount}
                        />
                      </div>
                    </TableCell>
                    <TableCell>
                      <Label htmlFor={lastFourId} className="sr-only">
                        Last four digits
                      </Label>
                      <div className="flex items-center gap-2">
                        <Input
                          id={lastFourId}
                          value={payment.lastFour}
                          inputMode="numeric"
                          pattern="\d{0,4}"
                          maxLength={4}
                          onChange={(e) => {
                            const next = e.target.value;
                            if (LAST_FOUR_PATTERN.test(next)) {
                              handleField(payment.id, "lastFour", next);
                            }
                          }}
                          placeholder="1234"
                          className="h-8 w-20 font-mono"
                          aria-invalid={
                            payment.lastFour.length > 0 &&
                            payment.lastFour.length !== 4
                          }
                        />
                        <ConfidenceIndicator
                          confidence={fieldConfidence?.lastFour}
                        />
                      </div>
                    </TableCell>
                    <TableCell>
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleRemove(payment.id)}
                        aria-label="Remove payment"
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        )}
        <Button
          type="button"
          variant="secondary"
          size="sm"
          onClick={handleAdd}
        >
          <Plus className="mr-1 h-4 w-4" />
          Add Payment
        </Button>
      </CardContent>
    </Card>
  );
}

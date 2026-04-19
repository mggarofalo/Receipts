import { useState, useMemo, useCallback, useRef, useEffect } from "react";
import { generateId } from "@/lib/id";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useFormShortcuts } from "@/hooks/useFormShortcuts";
import { useAccounts } from "@/hooks/useAccounts";
import { accountToOption } from "@/lib/combobox-options";
import { formatCurrency } from "@/lib/format";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { DateInput } from "@/components/ui/date-input";
import { Combobox } from "@/components/ui/combobox";
import { CurrencyInput } from "@/components/ui/currency-input";
import { Badge } from "@/components/ui/badge";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Plus, Trash2, AlertTriangle } from "lucide-react";
import type { WizardTransaction } from "./wizardReducer";

const txnSchema = z.object({
  accountId: z.string().min(1, "Account is required"),
  amount: z.number().refine((v) => v !== 0, "Amount is required"),
  date: z.string().min(1, "Date is required"),
});

type TxnFormValues = z.output<typeof txnSchema>;

interface Step2Props {
  data: WizardTransaction[];
  receiptDate: string;
  taxAmount: number;
  onNext: (data: WizardTransaction[]) => void;
  onBack: () => void;
}

export function Step2Transactions({
  data,
  receiptDate,
  taxAmount,
  onNext,
  onBack,
}: Step2Props) {
  const [transactions, setTransactions] = useState<WizardTransaction[]>(data);
  const formRef = useRef<HTMLFormElement>(null);
  const accountRef = useRef<HTMLButtonElement>(null);
  const { data: accounts } = useAccounts(0, 50, undefined, undefined, true);
  useFormShortcuts({ formRef });

  const accountOptions = useMemo(
    () => (accounts ?? []).map(accountToOption),
    [accounts],
  );

  const accountNameMap = useMemo(() => {
    const map = new Map<string, string>();
    for (const opt of accountOptions) {
      map.set(opt.value, opt.label);
    }
    return map;
  }, [accountOptions]);

  const form = useForm<TxnFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(txnSchema) as any,
    defaultValues: {
      accountId: "",
      amount: 0,
      date: receiptDate,
    },
  });

  // Sync the date field when the receipt date changes and the field is empty
  const prevReceiptDateRef = useRef(receiptDate);
  useEffect(() => {
    const currentDate = form.getValues("date");
    if (
      receiptDate !== prevReceiptDateRef.current &&
      (currentDate === "" || currentDate === prevReceiptDateRef.current)
    ) {
      form.setValue("date", receiptDate, { shouldValidate: true });
    }
    prevReceiptDateRef.current = receiptDate;
  }, [receiptDate, form]);

  const runningTotal = useMemo(
    () => transactions.reduce((sum, t) => sum + t.amount, 0),
    [transactions],
  );

  const handleAdd = useCallback(
    (values: TxnFormValues) => {
      const newTxn: WizardTransaction = {
        id: generateId(),
        ...values,
      };
      setTransactions((prev) => [...prev, newTxn]);
      // Blur before reset so CurrencyInput's handleBlur writes its stale
      // internal text state first, then form.reset() overwrites it.
      (document.activeElement as HTMLElement)?.blur?.();
      form.reset({ accountId: "", amount: 0, date: receiptDate });
    },
    [form, receiptDate],
  );

  // Focus the account field after adding a transaction for rapid entry
  const transactionCount = transactions.length;
  const prevCountRef = useRef(transactionCount);
  useEffect(() => {
    if (transactionCount > prevCountRef.current) {
      accountRef.current?.focus();
    }
    prevCountRef.current = transactionCount;
  }, [transactionCount]);

  const handleRemove = useCallback((id: string) => {
    setTransactions((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const handleNext = useCallback(() => {
    onNext(transactions);
  }, [onNext, transactions]);

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>Transactions</CardTitle>
          <div className="flex items-center gap-2">
            <span className="text-sm text-muted-foreground">
              Total: {formatCurrency(runningTotal)}
            </span>
            {taxAmount > 0 && (
              <Badge variant="outline" className="text-xs">
                Tax from Step 1: {formatCurrency(taxAmount)}
              </Badge>
            )}
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-6">
        <Form {...form}>
          <form
            ref={formRef}
            onSubmit={form.handleSubmit(handleAdd)}
            className="grid grid-cols-1 gap-4 sm:grid-cols-3 sm:items-end"
          >
            <FormField
              control={form.control}
              name="accountId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel required>Account</FormLabel>
                  <FormControl>
                    <Combobox
                      ref={accountRef}
                      options={accountOptions}
                      value={field.value}
                      onValueChange={field.onChange}
                      placeholder="Select account..."
                      searchPlaceholder="Search accounts..."
                      emptyMessage="No accounts found."
                      aria-required="true"
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="amount"
              render={({ field }) => (
                <FormItem>
                  <FormLabel required>Amount</FormLabel>
                  <FormControl>
                    <CurrencyInput {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="date"
              render={({ field }) => (
                <FormItem>
                  <FormLabel required>Date</FormLabel>
                  <FormControl>
                    <DateInput aria-required="true" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="sm:col-span-3 flex justify-end">
              <Button type="submit" variant="secondary" size="sm">
                <Plus className="mr-1 h-4 w-4" />
                Add Transaction
              </Button>
            </div>
          </form>
        </Form>

        {transactions.length > 0 && (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Account</TableHead>
                <TableHead>Amount</TableHead>
                <TableHead>Date</TableHead>
                <TableHead className="w-12" />
              </TableRow>
            </TableHeader>
            <TableBody>
              {transactions.map((txn) => (
                <TableRow key={txn.id}>
                  <TableCell>
                    {accountNameMap.get(txn.accountId) ?? txn.accountId}
                  </TableCell>
                  <TableCell>{formatCurrency(txn.amount)}</TableCell>
                  <TableCell>{txn.date}</TableCell>
                  <TableCell>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => handleRemove(txn.id)}
                    >
                      <Trash2 className="h-4 w-4" />
                      <span className="sr-only">Remove</span>
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}

        {transactions.length > 0 && Math.abs(runningTotal) < 0.01 && taxAmount > 0 && (
          <Alert variant="destructive">
            <AlertTriangle className="h-4 w-4" />
            <AlertDescription>
              Transaction total is $0.00 but tax is{" "}
              {formatCurrency(taxAmount)}. The receipt will be unbalanced.
            </AlertDescription>
          </Alert>
        )}

        <div className="flex justify-between pt-4">
          <Button variant="outline" onClick={onBack}>
            Back
          </Button>
          <Button onClick={handleNext} disabled={transactions.length === 0}>
            Next
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

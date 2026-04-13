import { useMemo, useCallback, useRef, useEffect } from "react";
import { generateId } from "@/lib/id";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useFormShortcuts } from "@/hooks/useFormShortcuts";
import { useCards } from "@/hooks/useCards";
import { cardToOption } from "@/lib/combobox-options";
import { formatCurrency } from "@/lib/format";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { DateInput } from "@/components/ui/date-input";
import { Combobox } from "@/components/ui/combobox";
import { CurrencyInput } from "@/components/ui/currency-input";
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
import { Plus, Trash2 } from "lucide-react";

const txnSchema = z.object({
  accountId: z.string().min(1, "Account is required"),
  amount: z.number().refine((v) => v !== 0, "Amount is required"),
  date: z.string().min(1, "Date is required"),
});

type TxnFormValues = z.output<typeof txnSchema>;

export interface ReceiptTransaction {
  id: string;
  accountId: string;
  amount: number;
  date: string;
}

interface TransactionsSectionProps {
  transactions: ReceiptTransaction[];
  defaultDate: string;
  onChange: (transactions: ReceiptTransaction[]) => void;
}

export function TransactionsSection({
  transactions,
  defaultDate,
  onChange,
}: TransactionsSectionProps) {
  const formRef = useRef<HTMLFormElement>(null);
  const accountRef = useRef<HTMLButtonElement>(null);
  const { data: cards } = useCards(0, 50, undefined, undefined, true);
  useFormShortcuts({ formRef });

  const accountOptions = useMemo(
    () =>
      (
        (cards as
          | { id: string; name: string; cardCode: string }[]
          | undefined) ?? []
      ).map(cardToOption),
    [cards],
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
      date: defaultDate,
    },
  });

  // Sync the date field when the receipt date changes and the field is empty
  const prevDefaultDateRef = useRef(defaultDate);
  useEffect(() => {
    const currentDate = form.getValues("date");
    if (
      defaultDate !== prevDefaultDateRef.current &&
      (currentDate === "" || currentDate === prevDefaultDateRef.current)
    ) {
      form.setValue("date", defaultDate, { shouldValidate: true });
    }
    prevDefaultDateRef.current = defaultDate;
  }, [defaultDate, form]);

  const runningTotal = useMemo(
    () => transactions.reduce((sum, t) => sum + t.amount, 0),
    [transactions],
  );

  const handleAdd = useCallback(
    (values: TxnFormValues) => {
      const newTxn: ReceiptTransaction = {
        id: generateId(),
        ...values,
      };
      onChange([...transactions, newTxn]);
      (document.activeElement as HTMLElement)?.blur?.();
      form.reset({ accountId: "", amount: 0, date: defaultDate });
    },
    [form, defaultDate, transactions, onChange],
  );

  // Focus account field after adding a transaction for rapid entry
  const prevCountRef = useRef(transactions.length);
  useEffect(() => {
    if (transactions.length > prevCountRef.current) {
      accountRef.current?.focus();
    }
    prevCountRef.current = transactions.length;
  }, [transactions.length]);

  const handleRemove = useCallback(
    (id: string) => {
      onChange(transactions.filter((t) => t.id !== id));
    },
    [transactions, onChange],
  );

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <CardTitle className="text-lg">Transactions</CardTitle>
          <span className="text-sm text-muted-foreground">
            Total: {formatCurrency(runningTotal)}
          </span>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <Form {...form}>
          <form
            ref={formRef}
            onSubmit={form.handleSubmit(handleAdd)}
            className="grid grid-cols-1 gap-4 sm:grid-cols-[1fr_auto_auto_auto] sm:items-end"
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

            <Button type="submit" variant="secondary" size="sm" className="sm:mb-0.5">
              <Plus className="mr-1 h-4 w-4" />
              Add
            </Button>
          </form>
        </Form>

        {transactions.length > 0 && (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Account</TableHead>
                <TableHead>Amount</TableHead>
                <TableHead>Date</TableHead>
                <TableHead className="w-12">
                  <span className="sr-only">Actions</span>
                </TableHead>
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
      </CardContent>
    </Card>
  );
}

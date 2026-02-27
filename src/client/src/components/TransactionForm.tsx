import { useMemo, useRef } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useFormShortcuts } from "@/hooks/useFormShortcuts";
import { useReceipts } from "@/hooks/useReceipts";
import { useAccounts } from "@/hooks/useAccounts";
import { receiptToOption, accountToOption } from "@/lib/combobox-options";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
import { Spinner } from "@/components/ui/spinner";

const transactionSchema = z.object({
  receiptId: z.string().min(1, "Receipt is required"),
  accountId: z.string().min(1, "Account is required"),
  amount: z.number(),
  date: z.string().min(1, "Date is required"),
});

type TransactionFormValues = z.output<typeof transactionSchema>;

interface TransactionFormProps {
  mode: "create" | "edit";
  defaultValues?: Partial<TransactionFormValues>;
  onSubmit: (values: TransactionFormValues) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
}

export function TransactionForm({
  mode,
  defaultValues,
  onSubmit,
  onCancel,
  isSubmitting,
}: TransactionFormProps) {
  const formRef = useRef<HTMLFormElement>(null);
  useFormShortcuts({ formRef });

  const { data: receipts, isLoading: receiptsLoading } = useReceipts();
  const { data: accounts, isLoading: accountsLoading } = useAccounts();

  const receiptOptions = useMemo(
    () =>
      ((receipts as { id: string; description?: string | null; location: string; date: string }[] | undefined) ?? []).map(receiptToOption),
    [receipts],
  );

  const accountOptions = useMemo(
    () =>
      ((accounts as { id: string; name: string; accountCode: string }[] | undefined) ?? []).map(accountToOption),
    [accounts],
  );

  const form = useForm<TransactionFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(transactionSchema) as any,
    defaultValues: {
      receiptId: "",
      accountId: "",
      amount: 0,
      date: "",
      ...defaultValues,
    },
  });

  return (
    <Form {...form}>
      <form ref={formRef} onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="receiptId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Receipt</FormLabel>
              <FormControl>
                <Combobox
                  options={receiptOptions}
                  value={field.value}
                  onValueChange={field.onChange}
                  placeholder="Select a receipt..."
                  searchPlaceholder="Search receipts..."
                  emptyMessage="No receipts found."
                  disabled={mode === "edit"}
                  loading={receiptsLoading}
                  aria-required="true"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="accountId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Account</FormLabel>
              <FormControl>
                <Combobox
                  options={accountOptions}
                  value={field.value}
                  onValueChange={field.onChange}
                  placeholder="Select an account..."
                  searchPlaceholder="Search accounts..."
                  emptyMessage="No accounts found."
                  disabled={mode === "edit"}
                  loading={accountsLoading}
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
              <FormLabel>Amount</FormLabel>
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
              <FormLabel>Date</FormLabel>
              <FormControl>
                <Input type="date" aria-required="true" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-2 pt-4">
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting && <Spinner size="sm" />}
            {isSubmitting
              ? "Saving..."
              : mode === "create"
                ? "Create Transaction"
                : "Update Transaction"}
          </Button>
        </div>
      </form>
    </Form>
  );
}

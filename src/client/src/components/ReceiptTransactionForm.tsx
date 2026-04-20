import { useMemo, useRef } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useFormShortcuts } from "@/hooks/useFormShortcuts";
import { useAccounts } from "@/hooks/useAccounts";
import { useCards } from "@/hooks/useCards";
import { accountToOption, cardToOption } from "@/lib/combobox-options";
import { Button } from "@/components/ui/button";
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
import { Spinner } from "@/components/ui/spinner";

const baseTransactionSchema = z.object({
  accountId: z.string().min(1, "Account is required"),
  amount: z.number().refine((v) => v !== 0, "Amount is required"),
  date: z.string().min(1, "Date is required"),
});

const createTransactionSchema = baseTransactionSchema.extend({
  cardId: z.string().min(1, "Card is required"),
});

// Edit mode keeps cardId string-typed but skips the min(1) check so legacy
// transactions with cardId = null can still be edited (amount, date, account)
// without forcing the user to assign a card first. RECEIPTS-574 will tighten
// this once all rows are backfilled.
const editTransactionSchema = baseTransactionSchema.extend({
  cardId: z.string(),
});

export type ReceiptTransactionFormValues = z.output<
  typeof createTransactionSchema
>;

interface ReceiptTransactionFormProps {
  mode: "create" | "edit";
  defaultValues?: Partial<ReceiptTransactionFormValues>;
  onSubmit: (values: ReceiptTransactionFormValues) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
  serverErrors?: Record<string, string>;
}

export function ReceiptTransactionForm({
  mode,
  defaultValues,
  onSubmit,
  onCancel,
  isSubmitting,
  serverErrors,
}: ReceiptTransactionFormProps) {
  const formRef = useRef<HTMLFormElement>(null);
  useFormShortcuts({ formRef });

  const { data: accounts, isLoading: accountsLoading } = useAccounts(0, 50, undefined, undefined, true);
  const { data: cards, isLoading: cardsLoading } = useCards(0, 500, undefined, undefined, true);

  const accountOptions = useMemo(
    () => (accounts ?? []).map(accountToOption),
    [accounts],
  );

  const cardOptions = useMemo(
    () => (cards ?? []).map(cardToOption),
    [cards],
  );

  const cardById = useMemo(() => {
    const map = new Map<string, { id: string; accountId?: string | null }>();
    for (const c of cards ?? []) map.set(c.id, c);
    return map;
  }, [cards]);

  const schema = mode === "create" ? createTransactionSchema : editTransactionSchema;
  const form = useForm<ReceiptTransactionFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(schema) as any,
    defaultValues: {
      cardId: "",
      accountId: "",
      amount: 0,
      date: "",
      ...defaultValues,
    },
  });

  function handleCardChange(value: string) {
    form.setValue("cardId", value, { shouldValidate: true });
    const card = cardById.get(value);
    if (card?.accountId) {
      form.setValue("accountId", card.accountId, { shouldValidate: true });
    }
  }

  return (
    <Form {...form}>
      <form
        ref={formRef}
        onSubmit={form.handleSubmit(onSubmit)}
        className="space-y-4"
      >
        <FormField
          control={form.control}
          name="cardId"
          render={({ field }) => (
            <FormItem>
              <FormLabel required>Card</FormLabel>
              <FormControl>
                <Combobox
                  options={cardOptions}
                  value={field.value}
                  onValueChange={handleCardChange}
                  placeholder="Select a card..."
                  searchPlaceholder="Search cards..."
                  emptyMessage="No cards found."
                  loading={cardsLoading}
                  aria-required="true"
                />
              </FormControl>
              <FormMessage />
              {serverErrors?.cardId && (
                <p className="text-sm font-medium text-destructive">
                  {serverErrors.cardId}
                </p>
              )}
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="accountId"
          render={({ field }) => (
            <FormItem>
              <FormLabel required>Account</FormLabel>
              <FormControl>
                <Combobox
                  options={accountOptions}
                  value={field.value}
                  onValueChange={field.onChange}
                  placeholder="Select an account..."
                  searchPlaceholder="Search accounts..."
                  emptyMessage="No accounts found."
                  loading={accountsLoading}
                  aria-required="true"
                />
              </FormControl>
              <FormMessage />
              {serverErrors?.accountId && (
                <p className="text-sm font-medium text-destructive">
                  {serverErrors.accountId}
                </p>
              )}
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
              {serverErrors?.amount && (
                <p className="text-sm font-medium text-destructive">
                  {serverErrors.amount}
                </p>
              )}
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
              {serverErrors?.date && (
                <p className="text-sm font-medium text-destructive">
                  {serverErrors.date}
                </p>
              )}
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
                ? "Add Transaction"
                : "Update Transaction"}
          </Button>
        </div>
      </form>
    </Form>
  );
}

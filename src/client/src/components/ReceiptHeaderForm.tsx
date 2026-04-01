import { useRef } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useFormShortcuts } from "@/hooks/useFormShortcuts";
import { useLocationHistory } from "@/hooks/useLocationHistory";
import { Button } from "@/components/ui/button";
import { DateInput } from "@/components/ui/date-input";
import { CurrencyInput } from "@/components/ui/currency-input";
import { Combobox } from "@/components/ui/combobox";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Spinner } from "@/components/ui/spinner";

const receiptHeaderSchema = z.object({
  location: z.string().min(1, "Location is required"),
  date: z.string().min(1, "Date is required"),
  taxAmount: z.number().min(0, "Tax amount must be zero or positive"),
});

export type ReceiptHeaderFormValues = z.output<typeof receiptHeaderSchema>;

interface ReceiptHeaderFormProps {
  defaultValues?: Partial<ReceiptHeaderFormValues>;
  onSubmit: (values: ReceiptHeaderFormValues) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
  serverErrors?: Record<string, string>;
}

export function ReceiptHeaderForm({
  defaultValues,
  onSubmit,
  onCancel,
  isSubmitting,
  serverErrors,
}: ReceiptHeaderFormProps) {
  const formRef = useRef<HTMLFormElement>(null);
  useFormShortcuts({ formRef });
  const { options: locationOptions } = useLocationHistory();

  const form = useForm<ReceiptHeaderFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(receiptHeaderSchema) as any,
    defaultValues: {
      location: "",
      date: "",
      taxAmount: 0,
      ...defaultValues,
    },
  });

  return (
    <Form {...form}>
      <form
        ref={formRef}
        onSubmit={form.handleSubmit(onSubmit)}
        className="space-y-4"
      >
        <FormField
          control={form.control}
          name="location"
          render={({ field }) => (
            <FormItem>
              <FormLabel required>Location</FormLabel>
              <FormControl>
                <Combobox
                  options={locationOptions}
                  value={field.value}
                  onValueChange={field.onChange}
                  placeholder="Store name or location"
                  searchPlaceholder="Search locations..."
                  emptyMessage="No saved locations."
                  allowCustom
                  aria-required="true"
                />
              </FormControl>
              <FormMessage />
              {serverErrors?.location && (
                <p className="text-sm font-medium text-destructive">
                  {serverErrors.location}
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

        <FormField
          control={form.control}
          name="taxAmount"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Tax Amount</FormLabel>
              <FormControl>
                <CurrencyInput {...field} />
              </FormControl>
              <FormMessage />
              {serverErrors?.taxAmount && (
                <p className="text-sm font-medium text-destructive">
                  {serverErrors.taxAmount}
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
            {isSubmitting ? "Saving..." : "Update Receipt"}
          </Button>
        </div>
      </form>
    </Form>
  );
}

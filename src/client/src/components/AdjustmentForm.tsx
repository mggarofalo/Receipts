import { useRef } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useFormShortcuts } from "@/hooks/useFormShortcuts";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { CurrencyInput } from "@/components/ui/currency-input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Spinner } from "@/components/ui/spinner";

const ADJUSTMENT_TYPES = [
  { value: "tip", label: "Tip" },
  { value: "discount", label: "Discount" },
  { value: "rounding", label: "Rounding" },
  { value: "loyaltyRedemption", label: "Loyalty Redemption" },
  { value: "coupon", label: "Coupon" },
  { value: "storeCredit", label: "Store Credit" },
  { value: "other", label: "Other" },
] as const;

const adjustmentSchema = z
  .object({
    type: z.string().min(1, "Type is required"),
    amount: z.number({ error: "Amount is required" }),
    description: z.string().optional(),
  })
  .refine((data) => data.type !== "other" || (data.description && data.description.trim().length > 0), {
    message: "Description is required when type is 'other'",
    path: ["description"],
  });

export type AdjustmentFormValues = z.output<typeof adjustmentSchema>;

interface AdjustmentFormProps {
  mode: "create" | "edit";
  defaultValues?: Partial<AdjustmentFormValues>;
  onSubmit: (values: AdjustmentFormValues) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
  serverErrors?: Record<string, string>;
}

export function AdjustmentForm({
  mode,
  defaultValues,
  onSubmit,
  onCancel,
  isSubmitting,
  serverErrors,
}: AdjustmentFormProps) {
  const formRef = useRef<HTMLFormElement>(null);
  useFormShortcuts({ formRef });

  const form = useForm<AdjustmentFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(adjustmentSchema) as any,
    defaultValues: {
      type: "",
      amount: 0,
      description: "",
      ...defaultValues,
    },
  });

  // eslint-disable-next-line react-hooks/incompatible-library
  const watchedType = form.watch("type");

  return (
    <Form {...form}>
      <form
        ref={formRef}
        onSubmit={form.handleSubmit(onSubmit)}
        className="space-y-4"
      >
        <FormField
          control={form.control}
          name="type"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Type</FormLabel>
              <FormControl>
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Select adjustment type" />
                  </SelectTrigger>
                  <SelectContent>
                    {ADJUSTMENT_TYPES.map((t) => (
                      <SelectItem key={t.value} value={t.value}>
                        {t.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </FormControl>
              <FormMessage />
              {serverErrors?.type && (
                <p className="text-sm font-medium text-destructive">
                  {serverErrors.type}
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
              <FormLabel>Amount</FormLabel>
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

        {watchedType === "other" && (
          <FormField
            control={form.control}
            name="description"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Description</FormLabel>
                <FormControl>
                  <Input
                    placeholder="Describe this adjustment..."
                    aria-required="true"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
                {serverErrors?.description && (
                  <p className="text-sm font-medium text-destructive">
                    {serverErrors.description}
                  </p>
                )}
              </FormItem>
            )}
          />
        )}

        <div className="flex justify-end gap-2 pt-4">
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting && <Spinner size="sm" />}
            {isSubmitting
              ? "Saving..."
              : mode === "create"
                ? "Add Adjustment"
                : "Update Adjustment"}
          </Button>
        </div>
      </form>
    </Form>
  );
}

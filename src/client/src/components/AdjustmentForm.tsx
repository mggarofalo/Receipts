import { useRef } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useFormShortcuts } from "@/hooks/useFormShortcuts";
import { useFieldHistory } from "@/hooks/useFieldHistory";
import { adjustmentDescriptionHistory } from "@/lib/field-history";
import { useEnumMetadata } from "@/hooks/useEnumMetadata";
import { Button } from "@/components/ui/button";
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

const adjustmentSchema = z
  .object({
    type: z.string().min(1, "Type is required"),
    amount: z.number({ error: "Amount is required" }),
    description: z.string().optional(),
  })
  .refine((data) => data.type.toLowerCase() !== "other" || (data.description && data.description.trim().length > 0), {
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
  const { adjustmentTypes } = useEnumMetadata();
  const { options: adjustmentDescOptions, add: addAdjustmentDesc } =
    useFieldHistory(adjustmentDescriptionHistory);

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
        onSubmit={form.handleSubmit((values) => {
          if (values.type.toLowerCase() === "other" && values.description) {
            addAdjustmentDesc(values.description);
          }
          onSubmit(values);
        })}
        className="space-y-4"
      >
        <FormField
          control={form.control}
          name="type"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Type</FormLabel>
              <FormControl>
                <Combobox
                  options={adjustmentTypes}
                  value={field.value}
                  onValueChange={field.onChange}
                  placeholder="Select adjustment type..."
                  searchPlaceholder="Search types..."
                />
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

        {watchedType.toLowerCase() === "other" && (
          <FormField
            control={form.control}
            name="description"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Description</FormLabel>
                <FormControl>
                  <Combobox
                    options={adjustmentDescOptions}
                    value={field.value ?? ""}
                    onValueChange={field.onChange}
                    placeholder="Describe this adjustment..."
                    searchPlaceholder="Search descriptions..."
                    emptyMessage="Type to describe this adjustment"
                    allowCustom
                    aria-required="true"
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

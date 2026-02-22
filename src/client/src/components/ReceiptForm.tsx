import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

const receiptSchema = z.object({
  description: z.string().optional(),
  location: z.string().min(1, "Location is required"),
  date: z.string().min(1, "Date is required"),
  taxAmount: z.coerce.number().min(0, "Tax amount must be non-negative"),
});

type ReceiptFormValues = z.infer<typeof receiptSchema>;

interface ReceiptFormProps {
  mode: "create" | "edit";
  defaultValues?: ReceiptFormValues;
  onSubmit: (values: ReceiptFormValues) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
}

export function ReceiptForm({
  mode,
  defaultValues,
  onSubmit,
  onCancel,
  isSubmitting,
}: ReceiptFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ReceiptFormValues>({
    resolver: zodResolver(receiptSchema),
    defaultValues: defaultValues ?? {
      description: "",
      location: "",
      date: "",
      taxAmount: 0,
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="space-y-2">
        <Label htmlFor="location">Location</Label>
        <Input id="location" {...register("location")} />
        {errors.location && (
          <p className="text-sm text-destructive">
            {errors.location.message}
          </p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="description">Description (optional)</Label>
        <Input id="description" {...register("description")} />
      </div>

      <div className="space-y-2">
        <Label htmlFor="date">Date</Label>
        <Input id="date" type="date" {...register("date")} />
        {errors.date && (
          <p className="text-sm text-destructive">{errors.date.message}</p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="taxAmount">Tax Amount</Label>
        <Input
          id="taxAmount"
          type="number"
          step="0.01"
          {...register("taxAmount")}
        />
        {errors.taxAmount && (
          <p className="text-sm text-destructive">
            {errors.taxAmount.message}
          </p>
        )}
      </div>

      <div className="flex justify-end gap-2 pt-4">
        <Button type="button" variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting
            ? "Saving..."
            : mode === "create"
              ? "Create Receipt"
              : "Update Receipt"}
        </Button>
      </div>
    </form>
  );
}

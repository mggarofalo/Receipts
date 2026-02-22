import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

const transactionSchema = z.object({
  receiptId: z.string().min(1, "Receipt ID is required"),
  accountId: z.string().min(1, "Account ID is required"),
  amount: z.coerce.number(),
  date: z.string().min(1, "Date is required"),
});

type TransactionFormValues = z.infer<typeof transactionSchema>;

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
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<TransactionFormValues>({
    resolver: zodResolver(transactionSchema),
    defaultValues: {
      receiptId: "",
      accountId: "",
      amount: 0,
      date: "",
      ...defaultValues,
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="space-y-2">
        <Label htmlFor="receiptId">Receipt ID</Label>
        <Input
          id="receiptId"
          {...register("receiptId")}
          disabled={mode === "edit"}
          placeholder="UUID of receipt"
        />
        {errors.receiptId && (
          <p className="text-sm text-destructive">
            {errors.receiptId.message}
          </p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="accountId">Account ID</Label>
        <Input
          id="accountId"
          {...register("accountId")}
          disabled={mode === "edit"}
          placeholder="UUID of account"
        />
        {errors.accountId && (
          <p className="text-sm text-destructive">
            {errors.accountId.message}
          </p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="amount">Amount</Label>
        <Input
          id="amount"
          type="number"
          step="0.01"
          {...register("amount")}
        />
        {errors.amount && (
          <p className="text-sm text-destructive">{errors.amount.message}</p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="date">Date</Label>
        <Input id="date" type="date" {...register("date")} />
        {errors.date && (
          <p className="text-sm text-destructive">{errors.date.message}</p>
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
              ? "Create Transaction"
              : "Update Transaction"}
        </Button>
      </div>
    </form>
  );
}

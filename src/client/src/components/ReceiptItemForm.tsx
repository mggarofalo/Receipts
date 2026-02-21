import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

const receiptItemSchema = z.object({
  receiptId: z.string().min(1, "Receipt ID is required"),
  receiptItemCode: z.string().min(1, "Item code is required"),
  description: z.string().min(1, "Description is required"),
  quantity: z.coerce.number().positive("Quantity must be positive"),
  unitPrice: z.coerce.number().min(0, "Unit price must be non-negative"),
  category: z.string().min(1, "Category is required"),
  subcategory: z.string().min(1, "Subcategory is required"),
});

type ReceiptItemFormValues = z.infer<typeof receiptItemSchema>;

interface ReceiptItemFormProps {
  mode: "create" | "edit";
  defaultValues?: Partial<ReceiptItemFormValues>;
  onSubmit: (values: ReceiptItemFormValues) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
}

export function ReceiptItemForm({
  mode,
  defaultValues,
  onSubmit,
  onCancel,
  isSubmitting,
}: ReceiptItemFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ReceiptItemFormValues>({
    resolver: zodResolver(receiptItemSchema),
    defaultValues: {
      receiptId: "",
      receiptItemCode: "",
      description: "",
      quantity: 1,
      unitPrice: 0,
      category: "",
      subcategory: "",
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
          placeholder="UUID of parent receipt"
        />
        {errors.receiptId && (
          <p className="text-sm text-destructive">
            {errors.receiptId.message}
          </p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="receiptItemCode">Item Code</Label>
        <Input id="receiptItemCode" {...register("receiptItemCode")} />
        {errors.receiptItemCode && (
          <p className="text-sm text-destructive">
            {errors.receiptItemCode.message}
          </p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="description">Description</Label>
        <Input id="description" {...register("description")} />
        {errors.description && (
          <p className="text-sm text-destructive">
            {errors.description.message}
          </p>
        )}
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="quantity">Quantity</Label>
          <Input
            id="quantity"
            type="number"
            step="1"
            {...register("quantity")}
          />
          {errors.quantity && (
            <p className="text-sm text-destructive">
              {errors.quantity.message}
            </p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="unitPrice">Unit Price</Label>
          <Input
            id="unitPrice"
            type="number"
            step="0.01"
            {...register("unitPrice")}
          />
          {errors.unitPrice && (
            <p className="text-sm text-destructive">
              {errors.unitPrice.message}
            </p>
          )}
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="category">Category</Label>
          <Input id="category" {...register("category")} />
          {errors.category && (
            <p className="text-sm text-destructive">
              {errors.category.message}
            </p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="subcategory">Subcategory</Label>
          <Input id="subcategory" {...register("subcategory")} />
          {errors.subcategory && (
            <p className="text-sm text-destructive">
              {errors.subcategory.message}
            </p>
          )}
        </div>
      </div>

      <div className="flex justify-end gap-2 pt-4">
        <Button type="button" variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting
            ? "Saving..."
            : mode === "create"
              ? "Create Item"
              : "Update Item"}
        </Button>
      </div>
    </form>
  );
}

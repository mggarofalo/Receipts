import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

const accountSchema = z.object({
  accountCode: z.string().min(1, "Account code is required"),
  name: z.string().min(1, "Name is required"),
  isActive: z.boolean(),
});

type AccountFormValues = z.infer<typeof accountSchema>;

interface AccountFormProps {
  mode: "create" | "edit";
  defaultValues?: AccountFormValues;
  onSubmit: (values: AccountFormValues) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
}

export function AccountForm({
  mode,
  defaultValues,
  onSubmit,
  onCancel,
  isSubmitting,
}: AccountFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AccountFormValues>({
    resolver: zodResolver(accountSchema),
    defaultValues: defaultValues ?? {
      accountCode: "",
      name: "",
      isActive: true,
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="space-y-2">
        <Label htmlFor="accountCode">Account Code</Label>
        <Input id="accountCode" {...register("accountCode")} />
        {errors.accountCode && (
          <p className="text-sm text-destructive">
            {errors.accountCode.message}
          </p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="name">Name</Label>
        <Input id="name" {...register("name")} />
        {errors.name && (
          <p className="text-sm text-destructive">{errors.name.message}</p>
        )}
      </div>

      <div className="flex items-center gap-2">
        <input
          type="checkbox"
          id="isActive"
          {...register("isActive")}
          className="h-4 w-4 rounded border-gray-300"
        />
        <Label htmlFor="isActive">Active</Label>
      </div>

      <div className="flex justify-end gap-2 pt-4">
        <Button type="button" variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting
            ? "Saving..."
            : mode === "create"
              ? "Create Account"
              : "Update Account"}
        </Button>
      </div>
    </form>
  );
}

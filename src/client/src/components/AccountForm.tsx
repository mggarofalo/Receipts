import { useRef } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useFormShortcuts } from "@/hooks/useFormShortcuts";
import { Button } from "@/components/ui/button";
import { SubmitButton } from "@/components/ui/submit-button";
import { Input } from "@/components/ui/input";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

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
  const formRef = useRef<HTMLFormElement>(null);
  useFormShortcuts({ formRef });

  const form = useForm<AccountFormValues>({
    resolver: zodResolver(accountSchema),
    defaultValues: defaultValues ?? {
      accountCode: "",
      name: "",
      isActive: true,
    },
  });

  return (
    <Form {...form}>
      <form ref={formRef} onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="accountCode"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Account Code</FormLabel>
              <FormControl>
                <Input aria-required="true" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Name</FormLabel>
              <FormControl>
                <Input aria-required="true" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="isActive"
          render={({ field }) => (
            <FormItem>
              <div className="flex items-center gap-2">
                <FormControl>
                  <input
                    type="checkbox"
                    checked={field.value}
                    onChange={field.onChange}
                    className="h-4 w-4 rounded border-gray-300"
                  />
                </FormControl>
                <FormLabel>Active</FormLabel>
              </div>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-2 pt-4">
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          <SubmitButton
            isSubmitting={isSubmitting ?? false}
            label={mode === "create" ? "Create Account" : "Update Account"}
            loadingLabel="Saving..."
          />
        </div>
      </form>
    </Form>
  );
}

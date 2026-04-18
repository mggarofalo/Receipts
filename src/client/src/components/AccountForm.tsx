import { useRef } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useFormShortcuts } from "@/hooks/useFormShortcuts";
import { Button } from "@/components/ui/button";
import { SubmitButton } from "@/components/ui/submit-button";
import { Input } from "@/components/ui/input";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Trash2 } from "lucide-react";

const accountSchema = z.object({
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
  onDelete?: () => void;
  isDeleting?: boolean;
  isAdmin?: boolean;
}

export function AccountForm({
  mode,
  defaultValues,
  onSubmit,
  onCancel,
  isSubmitting,
  onDelete,
  isDeleting,
  isAdmin,
}: AccountFormProps) {
  const formRef = useRef<HTMLFormElement>(null);
  useFormShortcuts({ formRef });

  const form = useForm<AccountFormValues>({
    resolver: zodResolver(accountSchema),
    defaultValues: defaultValues ?? {
      name: "",
      isActive: true,
    },
  });

  return (
    <Form {...form}>
      <form ref={formRef} onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel required>Name</FormLabel>
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

        <div className="flex items-center pt-4">
          {mode === "edit" && isAdmin && onDelete && (
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button
                  type="button"
                  variant="destructive"
                  size="sm"
                  disabled={isDeleting}
                >
                  <Trash2 className="mr-2 h-4 w-4" />
                  {isDeleting ? "Deleting..." : "Delete"}
                </Button>
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>Delete Account?</AlertDialogTitle>
                  <AlertDialogDescription>
                    This will permanently delete this logical account. Any cards
                    that currently point at it will be unlinked. This action
                    cannot be undone.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Cancel</AlertDialogCancel>
                  <AlertDialogAction
                    variant="destructive"
                    onClick={onDelete}
                  >
                    Delete
                  </AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
          )}
          <div className="ml-auto flex gap-2">
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancel
            </Button>
            <SubmitButton
              isSubmitting={isSubmitting ?? false}
              label={mode === "create" ? "Create Account" : "Update Account"}
              loadingLabel="Saving..."
            />
          </div>
        </div>
      </form>
    </Form>
  );
}

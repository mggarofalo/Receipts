import { useRef } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useFormShortcuts } from "@/hooks/useFormShortcuts";
import { useFieldHistory } from "@/hooks/useFieldHistory";
import { useCategories } from "@/hooks/useCategories";
import { subcategoryNameHistory } from "@/lib/field-history";
import { Combobox } from "@/components/ui/combobox";
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

const subcategorySchema = z.object({
  name: z.string().min(1, "Name is required"),
  categoryId: z.string().min(1, "Category is required"),
  description: z.string().optional(),
});

type SubcategoryFormValues = z.infer<typeof subcategorySchema>;

interface SubcategoryFormProps {
  mode: "create" | "edit";
  defaultValues?: Partial<SubcategoryFormValues>;
  onSubmit: (values: SubcategoryFormValues) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
}

export function SubcategoryForm({
  mode,
  defaultValues,
  onSubmit,
  onCancel,
  isSubmitting,
}: SubcategoryFormProps) {
  const formRef = useRef<HTMLFormElement>(null);
  useFormShortcuts({ formRef });
  const { options: subcategoryNameOptions, add: addSubcategoryName } =
    useFieldHistory(subcategoryNameHistory);
  const { data: categories } = useCategories();

  const categoryOptions = (
    categories?.data as { id: string; name: string }[] | undefined
  )?.map((c) => ({
    value: c.id,
    label: c.name,
  })) ?? [];

  const form = useForm<SubcategoryFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(subcategorySchema) as any,
    defaultValues: {
      name: "",
      categoryId: "",
      description: "",
      ...defaultValues,
    },
  });

  return (
    <Form {...form}>
      <form
        ref={formRef}
        onSubmit={form.handleSubmit((values) => {
          addSubcategoryName(values.name);
          onSubmit(values);
        })}
        className="space-y-4"
      >
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Name</FormLabel>
              <FormControl>
                <Combobox
                  options={subcategoryNameOptions}
                  value={field.value}
                  onValueChange={field.onChange}
                  placeholder="Select or type a name..."
                  searchPlaceholder="Search names..."
                  emptyMessage="Type to add a new subcategory"
                  allowCustom
                  aria-required="true"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="categoryId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Category</FormLabel>
              <FormControl>
                <Combobox
                  options={categoryOptions}
                  value={field.value}
                  onValueChange={field.onChange}
                  placeholder="Select a category..."
                  searchPlaceholder="Search categories..."
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Description (optional)</FormLabel>
              <FormControl>
                <Input {...field} />
              </FormControl>
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
            label={mode === "create" ? "Create Subcategory" : "Update Subcategory"}
            loadingLabel="Saving..."
          />
        </div>
      </form>
    </Form>
  );
}

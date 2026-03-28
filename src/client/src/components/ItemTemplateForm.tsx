import { useRef } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useFormShortcuts } from "@/hooks/useFormShortcuts";
import { useEnumMetadata } from "@/hooks/useEnumMetadata";
import { useCategories } from "@/hooks/useCategories";
import { useSubcategoriesByCategoryId } from "@/hooks/useSubcategories";
import { Combobox } from "@/components/ui/combobox";
import { CurrencyInput } from "@/components/ui/currency-input";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Spinner } from "@/components/ui/spinner";

const itemTemplateSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
  defaultCategory: z.string().optional(),
  defaultSubcategory: z.string().optional(),
  defaultUnitPrice: z.number().min(0).optional(),
  defaultPricingMode: z.string().optional(),
  defaultItemCode: z.string().optional(),
});

type ItemTemplateFormValues = z.infer<typeof itemTemplateSchema>;

interface ItemTemplateFormProps {
  mode: "create" | "edit";
  defaultValues?: Partial<ItemTemplateFormValues>;
  onSubmit: (values: ItemTemplateFormValues) => void;
  onCancel: () => void;
  isSubmitting?: boolean;
}

export function ItemTemplateForm({
  mode,
  defaultValues,
  onSubmit,
  onCancel,
  isSubmitting,
}: ItemTemplateFormProps) {
  const formRef = useRef<HTMLFormElement>(null);
  useFormShortcuts({ formRef });
  const { pricingModes } = useEnumMetadata();

  const { data: categories } = useCategories();
  const categoryOptions =
    (categories as { id: string; name: string; isActive: boolean }[] | undefined)
      ?.filter((c) => c.isActive)
      .map((c) => ({
        value: c.name,
        label: c.name,
      })) ?? [];

  const form = useForm<ItemTemplateFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(itemTemplateSchema) as any,
    defaultValues: {
      name: "",
      description: "",
      defaultCategory: "",
      defaultSubcategory: "",
      defaultUnitPrice: undefined,
      defaultPricingMode: "",
      defaultItemCode: "",
      ...defaultValues,
    },
  });

  // eslint-disable-next-line react-hooks/incompatible-library
  const watchedCategory = form.watch("defaultCategory");

  const selectedCategoryId =
    (categories as { id: string; name: string; isActive: boolean }[] | undefined)?.find(
      (c) => c.name === watchedCategory,
    )?.id ?? null;

  const { data: subcategoriesData } =
    useSubcategoriesByCategoryId(selectedCategoryId);

  const subcategoryOptions =
    (subcategoriesData as { id: string; name: string; isActive: boolean }[] | undefined)
      ?.filter((s) => s.isActive)
      .map((s) => ({
        value: s.name,
        label: s.name,
      })) ?? [];

  return (
    <Form {...form}>
      <form
        ref={formRef}
        onSubmit={form.handleSubmit(onSubmit)}
        className="space-y-4"
      >
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

        <div className="grid grid-cols-2 gap-4">
          <FormField
            control={form.control}
            name="defaultCategory"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Default Category (optional)</FormLabel>
                <FormControl>
                  <Combobox
                    options={categoryOptions}
                    value={field.value ?? ""}
                    onValueChange={field.onChange}
                    placeholder="Select category..."
                    searchPlaceholder="Search categories..."
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="defaultSubcategory"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Default Subcategory (optional)</FormLabel>
                <FormControl>
                  <Combobox
                    options={subcategoryOptions}
                    value={field.value ?? ""}
                    onValueChange={field.onChange}
                    placeholder="Select subcategory..."
                    searchPlaceholder="Search subcategories..."
                    allowCustom
                    disabled={!watchedCategory}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <FormField
            control={form.control}
            name="defaultUnitPrice"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Default Unit Price (optional)</FormLabel>
                <FormControl>
                  <CurrencyInput
                    value={field.value ?? 0}
                    onChange={field.onChange}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="defaultPricingMode"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Default Pricing Mode (optional)</FormLabel>
                <FormControl>
                  <Combobox
                    options={pricingModes}
                    value={field.value ?? ""}
                    onValueChange={field.onChange}
                    placeholder="Select pricing mode..."
                    searchPlaceholder="Search modes..."
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="defaultItemCode"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Default Item Code (optional)</FormLabel>
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
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting && <Spinner size="sm" />}
            {isSubmitting
              ? "Saving..."
              : mode === "create"
                ? "Create Template"
                : "Update Template"}
          </Button>
        </div>
      </form>
    </Form>
  );
}

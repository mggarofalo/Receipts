import { useState, useMemo, useRef, useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import Fuse from "fuse.js";
import { useFormShortcuts } from "@/hooks/useFormShortcuts";
import { useReceipts } from "@/hooks/useReceipts";
import { useCategories } from "@/hooks/useCategories";
import {
  useSubcategoriesByCategoryId,
  useCreateSubcategory,
} from "@/hooks/useSubcategories";
import { useItemTemplates } from "@/hooks/useItemTemplates";
import { receiptToOption } from "@/lib/combobox-options";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Combobox } from "@/components/ui/combobox";
import { CurrencyInput } from "@/components/ui/currency-input";
import {
  Popover,
  PopoverContent,
  PopoverAnchor,
} from "@/components/ui/popover";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Spinner } from "@/components/ui/spinner";
import { formatCurrency } from "@/lib/format";

interface ItemTemplate {
  id: string;
  name: string;
  description?: string | null;
  defaultCategory?: string | null;
  defaultSubcategory?: string | null;
  defaultUnitPrice?: number | null;
  defaultUnitPriceCurrency?: string | null;
  defaultPricingMode?: string | null;
  defaultItemCode?: string | null;
}

const receiptItemSchema = z.object({
  receiptId: z.string().min(1, "Receipt is required"),
  receiptItemCode: z.string().min(1, "Item code is required"),
  description: z.string().min(1, "Description is required"),
  pricingMode: z.enum(["quantity", "flat"]),
  quantity: z.number().positive("Quantity must be positive"),
  unitPrice: z.number().min(0, "Unit price must be non-negative"),
  category: z.string().min(1, "Category is required"),
  subcategory: z.string().min(1, "Subcategory is required"),
}).refine(
  (data) => data.pricingMode !== "flat" || data.quantity === 1,
  { message: "Quantity must be 1 for flat pricing", path: ["quantity"] }
);

type ReceiptItemFormValues = z.output<typeof receiptItemSchema>;

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
  const formRef = useRef<HTMLFormElement>(null);
  useFormShortcuts({ formRef });

  const { data: receipts, isLoading: receiptsLoading } = useReceipts();
  const { data: categoriesResponse } = useCategories();
  const categories = categoriesResponse?.data as { id: string; name: string }[] | undefined;
  const { data: itemTemplatesData } = useItemTemplates();
  const templates = useMemo(
    () => (itemTemplatesData as ItemTemplate[] | undefined) ?? [],
    [itemTemplatesData],
  );

  const receiptOptions = useMemo(
    () =>
      (
        (receipts as
          | {
              id: string;
              location: string;
              date: string;
            }[]
          | undefined) ?? []
      ).map(receiptToOption),
    [receipts],
  );

  const categoryOptions = useMemo(
    () =>
      (
        categories ?? []
      ).map((c) => ({
        value: c.name,
        label: c.name,
      })),
    [categories],
  );

  const form = useForm<ReceiptItemFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(receiptItemSchema) as any,
    defaultValues: {
      receiptId: "",
      receiptItemCode: "",
      description: "",
      pricingMode: "quantity",
      quantity: 1,
      unitPrice: 0,
      category: "",
      subcategory: "",
      ...defaultValues,
    },
  });

  // eslint-disable-next-line react-hooks/incompatible-library
  const watchedCategory = form.watch("category");

  const selectedCategoryId = useMemo(() => {
    if (!categories || !watchedCategory) return null;
    return categories?.find((c) => c.name === watchedCategory)?.id ?? null;
  }, [categories, watchedCategory]);

  const { data: subcategoriesData } =
    useSubcategoriesByCategoryId(selectedCategoryId);
  const createSubcategory = useCreateSubcategory();

  const subcategoryOptions = useMemo(
    () =>
      (
        (subcategoriesData as { id: string; name: string }[] | undefined) ?? []
      ).map((s) => ({
        value: s.name,
        label: s.name,
      })),
    [subcategoriesData],
  );

  // Reset subcategory when category changes (skip when applying a template)
  const prevCategoryRef = useRef(watchedCategory);
  const applyingTemplateRef = useRef(false);
  useEffect(() => {
    if (prevCategoryRef.current !== watchedCategory) {
      prevCategoryRef.current = watchedCategory;
      if (!applyingTemplateRef.current) {
        form.setValue("subcategory", "");
      }
      applyingTemplateRef.current = false;
    }
  }, [watchedCategory, form]);

  const watchedPricingMode = form.watch("pricingMode");
  const watchedQuantity = form.watch("quantity");
  const watchedUnitPrice = form.watch("unitPrice");
  const isFlat = watchedPricingMode === "flat";

  // Auto-set quantity to 1 when switching to flat mode
  const prevPricingModeRef = useRef(watchedPricingMode);
  useEffect(() => {
    if (prevPricingModeRef.current !== watchedPricingMode) {
      prevPricingModeRef.current = watchedPricingMode;
      if (watchedPricingMode === "flat") {
        form.setValue("quantity", 1);
      }
    }
  }, [watchedPricingMode, form]);

  async function handleFormSubmit(values: ReceiptItemFormValues) {
    const isCustomSubcategory =
      values.subcategory &&
      !subcategoryOptions.some((opt) => opt.value === values.subcategory);

    if (isCustomSubcategory && selectedCategoryId) {
      await createSubcategory.mutateAsync({
        name: values.subcategory,
        categoryId: selectedCategoryId,
      });
    }

    onSubmit(values);
  }

  const computedTotal = (watchedQuantity ?? 0) * (watchedUnitPrice ?? 0);

  // Fuse.js autocomplete for description
  const fuse = useMemo(
    () =>
      new Fuse(templates, {
        keys: ["name", "description", "defaultItemCode"],
        threshold: 0.4,
        includeScore: true,
      }),
    [templates],
  );

  const [descriptionInput, setDescriptionInput] = useState(
    defaultValues?.description ?? "",
  );
  const [autocompleteOpen, setAutocompleteOpen] = useState(false);

  const suggestions = useMemo(() => {
    if (!descriptionInput || descriptionInput.length < 2) return [];
    return fuse.search(descriptionInput, { limit: 5 }).map((r) => r.item);
  }, [fuse, descriptionInput]);

  function applyTemplate(template: ItemTemplate) {
    form.setValue("description", template.name);
    setDescriptionInput(template.name);
    if (template.defaultCategory) {
      applyingTemplateRef.current = true;
      form.setValue("category", template.defaultCategory);
    }
    if (template.defaultSubcategory) {
      form.setValue("subcategory", template.defaultSubcategory);
    }
    if (template.defaultUnitPrice != null) {
      form.setValue("unitPrice", template.defaultUnitPrice);
    }
    if (template.defaultItemCode) {
      form.setValue("receiptItemCode", template.defaultItemCode);
    }
    setAutocompleteOpen(false);
  }

  return (
    <Form {...form}>
      <form
        ref={formRef}
        onSubmit={form.handleSubmit(handleFormSubmit)}
        className="space-y-4"
      >
        <FormField
          control={form.control}
          name="receiptId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Receipt</FormLabel>
              <FormControl>
                <Combobox
                  options={receiptOptions}
                  value={field.value}
                  onValueChange={field.onChange}
                  placeholder="Select a receipt..."
                  searchPlaceholder="Search receipts..."
                  emptyMessage="No receipts found."
                  disabled={mode === "edit"}
                  loading={receiptsLoading}
                  aria-required="true"
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="receiptItemCode"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Item Code</FormLabel>
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
              <FormLabel>Description</FormLabel>
              <Popover
                open={autocompleteOpen && suggestions.length > 0}
                onOpenChange={setAutocompleteOpen}
              >
                <PopoverAnchor asChild>
                  <FormControl>
                    <Input
                      aria-required="true"
                      {...field}
                      value={descriptionInput}
                      onChange={(e) => {
                        const val = e.target.value;
                        setDescriptionInput(val);
                        field.onChange(val);
                        setAutocompleteOpen(val.length >= 2);
                      }}
                      onFocus={() => {
                        if (descriptionInput.length >= 2) {
                          setAutocompleteOpen(true);
                        }
                      }}
                      autoComplete="off"
                    />
                  </FormControl>
                </PopoverAnchor>
                <PopoverContent
                  className="w-[--radix-popover-trigger-width] p-0"
                  align="start"
                  onOpenAutoFocus={(e) => e.preventDefault()}
                  onInteractOutside={() => setAutocompleteOpen(false)}
                >
                  <Command>
                    <CommandList>
                      <CommandEmpty>No templates found.</CommandEmpty>
                      <CommandGroup heading="Item Templates">
                        {suggestions.map((template) => (
                          <CommandItem
                            key={template.id}
                            value={template.name}
                            onSelect={() => applyTemplate(template)}
                          >
                            <div className="flex flex-col">
                              <span className="font-medium">
                                {template.name}
                              </span>
                              {template.defaultCategory && (
                                <span className="text-xs text-muted-foreground">
                                  {template.defaultCategory}
                                  {template.defaultSubcategory
                                    ? ` / ${template.defaultSubcategory}`
                                    : ""}
                                </span>
                              )}
                            </div>
                          </CommandItem>
                        ))}
                      </CommandGroup>
                    </CommandList>
                  </Command>
                </PopoverContent>
              </Popover>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="pricingMode"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Pricing Mode</FormLabel>
              <FormControl>
                <Combobox
                  options={[
                    { value: "quantity", label: "Qty x Unit Price" },
                    { value: "flat", label: "Flat Price" },
                  ]}
                  value={field.value}
                  onValueChange={field.onChange}
                  placeholder="Select pricing mode..."
                  searchPlaceholder="Search modes..."
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid grid-cols-2 gap-4">
          {!isFlat && (
            <FormField
              control={form.control}
              name="quantity"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Quantity</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      step="1"
                      aria-required="true"
                      {...field}
                      onChange={(e) => field.onChange(e.target.valueAsNumber)}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          )}

          <FormField
            control={form.control}
            name="unitPrice"
            render={({ field }) => (
              <FormItem>
                <FormLabel>{isFlat ? "Price" : "Unit Price"}</FormLabel>
                <FormControl>
                  <CurrencyInput {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="text-sm text-muted-foreground">
          Total: {formatCurrency(computedTotal)}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <FormField
            control={form.control}
            name="category"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Category</FormLabel>
                <FormControl>
                  <Combobox
                    options={categoryOptions}
                    value={field.value}
                    onValueChange={field.onChange}
                    placeholder="Select category..."
                    searchPlaceholder="Search categories..."
                    allowCustom
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="subcategory"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Subcategory</FormLabel>
                <FormControl>
                  <Combobox
                    options={subcategoryOptions}
                    value={field.value}
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

        <div className="flex justify-end gap-2 pt-4">
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting && <Spinner size="sm" />}
            {isSubmitting
              ? "Saving..."
              : mode === "create"
                ? "Create Item"
                : "Update Item"}
          </Button>
        </div>
      </form>
    </Form>
  );
}

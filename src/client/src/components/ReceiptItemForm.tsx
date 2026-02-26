import { useMemo, useRef, useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useFormShortcuts } from "@/hooks/useFormShortcuts";
import { useReceipts } from "@/hooks/useReceipts";
import { useCategories } from "@/hooks/useCategories";
import { useSubcategoriesByCategoryId } from "@/hooks/useSubcategories";
import { receiptToOption } from "@/lib/combobox-options";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Combobox } from "@/components/ui/combobox";
import { CurrencyInput } from "@/components/ui/currency-input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
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
  const { data: categories } = useCategories();

  const receiptOptions = useMemo(
    () =>
      (
        (receipts as
          | {
              id: string;
              description?: string | null;
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
        (categories as { id: string; name: string }[] | undefined) ?? []
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
    return (categories as { id: string; name: string }[])
      .find((c) => c.name === watchedCategory)?.id ?? null;
  }, [categories, watchedCategory]);

  const { data: subcategoriesData } =
    useSubcategoriesByCategoryId(selectedCategoryId);

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

  const prevCategoryRef = useRef(watchedCategory);
  useEffect(() => {
    if (prevCategoryRef.current !== watchedCategory) {
      prevCategoryRef.current = watchedCategory;
      form.setValue("subcategory", "");
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

  const computedTotal = (watchedQuantity ?? 0) * (watchedUnitPrice ?? 0);

  return (
    <Form {...form}>
      <form
        ref={formRef}
        onSubmit={form.handleSubmit(onSubmit)}
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
              <FormControl>
                <Input aria-required="true" {...field} />
              </FormControl>
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
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Select pricing mode" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="quantity">Qty x Unit Price</SelectItem>
                    <SelectItem value="flat">Flat Price</SelectItem>
                  </SelectContent>
                </Select>
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

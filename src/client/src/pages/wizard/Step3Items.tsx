import { useState, useMemo, useCallback } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useCategories } from "@/hooks/useCategories";
import {
  useSubcategoriesByCategoryId,
  useCreateSubcategory,
} from "@/hooks/useSubcategories";
import { formatCurrency } from "@/lib/format";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Combobox } from "@/components/ui/combobox";
import { CurrencyInput } from "@/components/ui/currency-input";
import { Badge } from "@/components/ui/badge";
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
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Plus, Trash2 } from "lucide-react";
import type { WizardReceiptItem } from "./wizardReducer";

const itemSchema = z
  .object({
    receiptItemCode: z.string().optional().default(""),
    description: z.string().min(1, "Description is required"),
    pricingMode: z.enum(["quantity", "flat"]),
    quantity: z.number().positive("Quantity must be positive"),
    unitPrice: z.number().min(0, "Unit price must be non-negative"),
    category: z.string().min(1, "Category is required"),
    subcategory: z.string().optional().default(""),
  })
  .refine((data) => data.pricingMode !== "flat" || data.quantity === 1, {
    message: "Quantity must be 1 for flat pricing",
    path: ["quantity"],
  });

type ItemFormValues = z.output<typeof itemSchema>;

interface Step3Props {
  data: WizardReceiptItem[];
  taxAmount: number;
  transactionTotal: number;
  onNext: (data: WizardReceiptItem[]) => void;
  onBack: () => void;
}

export function Step3Items({
  data,
  taxAmount,
  transactionTotal,
  onNext,
  onBack,
}: Step3Props) {
  const [items, setItems] = useState<WizardReceiptItem[]>(data);
  const { data: categories } = useCategories();

  const categoryOptions = useMemo(
    () =>
      (
        (categories?.data as
          | { id: string; name: string }[]
          | undefined) ?? []
      ).map((c) => ({ value: c.name, label: c.name })),
    [categories],
  );

  const form = useForm<ItemFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(itemSchema) as any,
    defaultValues: {
      receiptItemCode: "",
      description: "",
      pricingMode: "quantity",
      quantity: 1,
      unitPrice: 0,
      category: "",
      subcategory: "",
    },
  });

  const selectedCategory = form.watch("category");
  const selectedCategoryObj = useMemo(
    () =>
      (
        (categories?.data as
          | { id: string; name: string }[]
          | undefined) ?? []
      ).find((c) => c.name === selectedCategory),
    [categories, selectedCategory],
  );

  const { data: subcategories } = useSubcategoriesByCategoryId(
    selectedCategoryObj?.id ?? "",
  );
  const createSubcategory = useCreateSubcategory();

  const subcategoryOptions = useMemo(
    () =>
      (
        (subcategories as { id: string; name: string }[] | undefined) ?? []
      ).map((s) => ({ value: s.name, label: s.name })),
    [subcategories],
  );

  const pricingMode = form.watch("pricingMode");

  const subtotal = useMemo(
    () => items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0),
    [items],
  );

  const expectedTotal = subtotal + taxAmount;
  const balanceDiff = Math.abs(expectedTotal - transactionTotal);
  const isBalanced = balanceDiff < 0.01;

  const handleAdd = useCallback(
    (values: ItemFormValues) => {
      const newItem: WizardReceiptItem = {
        id: crypto.randomUUID(),
        receiptItemCode: values.receiptItemCode ?? "",
        description: values.description,
        pricingMode: values.pricingMode,
        quantity: values.quantity,
        unitPrice: values.unitPrice,
        category: values.category,
        subcategory: values.subcategory ?? "",
      };
      setItems((prev) => [...prev, newItem]);
      form.reset({
        receiptItemCode: "",
        description: "",
        pricingMode: "quantity",
        quantity: 1,
        unitPrice: 0,
        category: values.category,
        subcategory: "",
      });
    },
    [form],
  );

  const handleRemove = useCallback((id: string) => {
    setItems((prev) => prev.filter((item) => item.id !== id));
  }, []);

  const handleNext = useCallback(() => {
    onNext(items);
  }, [onNext, items]);

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>Line Items</CardTitle>
          <div className="flex items-center gap-2">
            <span className="text-sm text-muted-foreground">
              Subtotal: {formatCurrency(subtotal)}
            </span>
            <Badge variant={isBalanced ? "default" : "secondary"}>
              {isBalanced
                ? "Balanced"
                : `Unbalanced (${formatCurrency(balanceDiff)})`}
            </Badge>
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-6">
        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(handleAdd)}
            className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3"
          >
            <FormField
              control={form.control}
              name="receiptItemCode"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Item Code (optional)</FormLabel>
                  <FormControl>
                    <Input placeholder="e.g. MILK-GAL" {...field} />
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
                    <Input
                      placeholder="Item description"
                      aria-required="true"
                      {...field}
                    />
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
                  <Select
                    onValueChange={(v) => {
                      field.onChange(v);
                      if (v === "flat") form.setValue("quantity", 1);
                    }}
                    value={field.value}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectItem value="quantity">Quantity</SelectItem>
                      <SelectItem value="flat">Flat</SelectItem>
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="quantity"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Quantity</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      step="any"
                      min="0.01"
                      disabled={pricingMode === "flat"}
                      {...field}
                      onChange={(e) => field.onChange(Number(e.target.value))}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="unitPrice"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>
                    {pricingMode === "flat" ? "Price" : "Unit Price"}
                  </FormLabel>
                  <FormControl>
                    <CurrencyInput {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

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
                      onValueChange={(v) => {
                        field.onChange(v);
                        form.setValue("subcategory", "");
                      }}
                      placeholder="Select category..."
                      searchPlaceholder="Search categories..."
                      emptyMessage="No categories found."
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
              name="subcategory"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Subcategory (optional)</FormLabel>
                  <FormControl>
                    <Combobox
                      options={subcategoryOptions}
                      value={field.value ?? ""}
                      onValueChange={field.onChange}
                      placeholder="Select subcategory..."
                      searchPlaceholder="Search subcategories..."
                      emptyMessage="No subcategories found."
                      allowCustom
                      onCustomCreate={async (name) => {
                        if (selectedCategoryObj?.id) {
                          await createSubcategory.mutateAsync({
                            categoryId: selectedCategoryObj.id,
                            name,
                          });
                        }
                      }}
                      disabled={!selectedCategory}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="sm:col-span-2 lg:col-span-3 flex justify-end">
              <Button type="submit" variant="secondary" size="sm">
                <Plus className="mr-1 h-4 w-4" />
                Add Item
              </Button>
            </div>
          </form>
        </Form>

        {items.length > 0 && (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Description</TableHead>
                <TableHead>Qty</TableHead>
                <TableHead>Unit Price</TableHead>
                <TableHead>Line Total</TableHead>
                <TableHead>Category</TableHead>
                <TableHead className="w-12" />
              </TableRow>
            </TableHeader>
            <TableBody>
              {items.map((item) => (
                <TableRow key={item.id}>
                  <TableCell>{item.description}</TableCell>
                  <TableCell>{item.quantity}</TableCell>
                  <TableCell>{formatCurrency(item.unitPrice)}</TableCell>
                  <TableCell>
                    {formatCurrency(item.quantity * item.unitPrice)}
                  </TableCell>
                  <TableCell>
                    {item.category}
                    {item.subcategory ? ` / ${item.subcategory}` : ""}
                  </TableCell>
                  <TableCell>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => handleRemove(item.id)}
                    >
                      <Trash2 className="h-4 w-4" />
                      <span className="sr-only">Remove</span>
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}

        <div className="flex justify-between pt-4">
          <Button variant="outline" onClick={onBack}>
            Back
          </Button>
          <Button onClick={handleNext} disabled={items.length === 0}>
            Next
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

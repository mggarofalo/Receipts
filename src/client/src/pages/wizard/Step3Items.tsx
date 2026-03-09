import { useState, useMemo, useCallback, useRef } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useCategories } from "@/hooks/useCategories";
import {
  useSubcategoriesByCategoryId,
  useCreateSubcategory,
} from "@/hooks/useSubcategories";
import {
  useSimilarItems,
  useCategoryRecommendations,
} from "@/hooks/useSimilarItems";
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
import { Plus, Trash2, Loader2, Sparkles } from "lucide-react";
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
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [selectedSuggestionIdx, setSelectedSuggestionIdx] = useState(-1);
  const suggestionsRef = useRef<HTMLDivElement>(null);
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

  const description = form.watch("description");
  const selectedCategory = form.watch("category");

  const { data: similarItems, isFetching: isFetchingSimilar } =
    useSimilarItems(description, { enabled: showSuggestions });

  const { data: categoryRecs } = useCategoryRecommendations(description, {
    enabled: description.length >= 2 && !selectedCategory,
  });

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
  const pendingSubcategories = useRef(new Set<string>());

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

  const applySuggestion = useCallback(
    (suggestion: NonNullable<typeof similarItems>[number]) => {
      form.setValue("description", suggestion.name);
      if (suggestion.defaultCategory) {
        form.setValue("category", suggestion.defaultCategory);
      }
      if (suggestion.defaultSubcategory) {
        form.setValue("subcategory", suggestion.defaultSubcategory);
      }
      if (suggestion.defaultUnitPrice != null) {
        form.setValue("unitPrice", suggestion.defaultUnitPrice);
      }
      if (
        suggestion.defaultPricingMode === "quantity" ||
        suggestion.defaultPricingMode === "flat"
      ) {
        form.setValue("pricingMode", suggestion.defaultPricingMode);
        if (suggestion.defaultPricingMode === "flat") {
          form.setValue("quantity", 1);
        }
      }
      if (suggestion.defaultItemCode) {
        form.setValue("receiptItemCode", suggestion.defaultItemCode);
      }
      setShowSuggestions(false);
      setSelectedSuggestionIdx(-1);
    },
    [form],
  );

  const applyCategoryRec = useCallback(
    (rec: NonNullable<typeof categoryRecs>[number]) => {
      form.setValue("category", rec.category);
      if (rec.subcategory) {
        form.setValue("subcategory", rec.subcategory);
      }
    },
    [form],
  );

  const handleDescriptionKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (!showSuggestions || !similarItems?.length) return;

      if (e.key === "ArrowDown") {
        e.preventDefault();
        setSelectedSuggestionIdx((prev) =>
          prev < similarItems.length - 1 ? prev + 1 : 0,
        );
      } else if (e.key === "ArrowUp") {
        e.preventDefault();
        setSelectedSuggestionIdx((prev) =>
          prev > 0 ? prev - 1 : similarItems.length - 1,
        );
      } else if (e.key === "Enter" && selectedSuggestionIdx >= 0) {
        e.preventDefault();
        applySuggestion(similarItems[selectedSuggestionIdx]);
      } else if (e.key === "Escape") {
        setShowSuggestions(false);
        setSelectedSuggestionIdx(-1);
      }
    },
    [showSuggestions, similarItems, selectedSuggestionIdx, applySuggestion],
  );

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
      setShowSuggestions(false);
      setSelectedSuggestionIdx(-1);
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
                <FormItem className="relative">
                  <FormLabel>Description</FormLabel>
                  <FormControl>
                    <div className="relative">
                      <Input
                        placeholder="Item description"
                        aria-required="true"
                        autoComplete="off"
                        {...field}
                        onFocus={() => setShowSuggestions(true)}
                        onBlur={() => {
                          // Delay to allow click on suggestion
                          setTimeout(() => setShowSuggestions(false), 200);
                        }}
                        onKeyDown={handleDescriptionKeyDown}
                      />
                      {isFetchingSimilar && description.length >= 2 && (
                        <Loader2 className="absolute right-2 top-1/2 h-4 w-4 -translate-y-1/2 animate-spin text-muted-foreground" />
                      )}
                    </div>
                  </FormControl>
                  {showSuggestions &&
                    similarItems &&
                    similarItems.length > 0 && (
                      <div
                        ref={suggestionsRef}
                        className="absolute z-50 mt-1 w-full rounded-md border bg-popover shadow-lg"
                        role="listbox"
                      >
                        {similarItems.map((item, idx) => (
                          <button
                            key={`${item.name}-${item.source}`}
                            type="button"
                            role="option"
                            aria-selected={idx === selectedSuggestionIdx}
                            className={`flex w-full items-center justify-between px-3 py-2 text-left text-sm hover:bg-accent ${
                              idx === selectedSuggestionIdx ? "bg-accent" : ""
                            } ${idx === 0 ? "rounded-t-md" : ""} ${
                              idx === similarItems.length - 1
                                ? "rounded-b-md"
                                : ""
                            }`}
                            onMouseDown={(e) => {
                              e.preventDefault();
                              applySuggestion(item);
                            }}
                          >
                            <div className="flex flex-col gap-0.5">
                              <span className="font-medium">{item.name}</span>
                              {item.defaultCategory && (
                                <span className="text-xs text-muted-foreground">
                                  {item.defaultCategory}
                                  {item.defaultSubcategory
                                    ? ` / ${item.defaultSubcategory}`
                                    : ""}
                                  {item.defaultUnitPrice != null
                                    ? ` · ${formatCurrency(item.defaultUnitPrice)}`
                                    : ""}
                                </span>
                              )}
                            </div>
                            <div className="flex items-center gap-1.5">
                              <Badge
                                variant="outline"
                                className="text-[10px] px-1.5 py-0"
                              >
                                {item.source === "template"
                                  ? "Template"
                                  : "History"}
                              </Badge>
                              <span className="text-[10px] text-muted-foreground">
                                {Math.round(item.combinedScore * 100)}%
                              </span>
                            </div>
                          </button>
                        ))}
                      </div>
                    )}
                  {showSuggestions &&
                    description.length >= 2 &&
                    !isFetchingSimilar &&
                    similarItems &&
                    similarItems.length === 0 && (
                      <div className="absolute z-50 mt-1 w-full rounded-md border bg-popover p-3 text-center text-sm text-muted-foreground shadow-lg">
                        No similar items found
                      </div>
                    )}
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
                  {categoryRecs &&
                    categoryRecs.length > 0 &&
                    !selectedCategory && (
                      <div className="flex flex-wrap gap-1 pt-1">
                        <Sparkles className="h-3 w-3 text-muted-foreground mt-0.5" />
                        {categoryRecs.map((rec) => (
                          <button
                            key={`${rec.category}-${rec.subcategory ?? ""}`}
                            type="button"
                            className="inline-flex items-center rounded-full border px-2 py-0.5 text-[10px] text-muted-foreground hover:bg-accent hover:text-accent-foreground transition-colors"
                            onClick={() => applyCategoryRec(rec)}
                          >
                            {rec.category}
                            {rec.subcategory ? ` / ${rec.subcategory}` : ""}
                          </button>
                        ))}
                      </div>
                    )}
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
                      onValueChange={(v: string) => {
                        field.onChange(v);
                        const isExisting = subcategoryOptions.some(
                          (o) => o.value === v,
                        );
                        if (
                          !isExisting &&
                          v &&
                          selectedCategoryObj?.id &&
                          !pendingSubcategories.current.has(v)
                        ) {
                          pendingSubcategories.current.add(v);
                          createSubcategory.mutate(
                            {
                              categoryId: selectedCategoryObj.id,
                              name: v,
                            },
                            {
                              onSettled: () => {
                                pendingSubcategories.current.delete(v);
                              },
                            },
                          );
                        }
                      }}
                      placeholder="Select subcategory..."
                      searchPlaceholder="Search subcategories..."
                      emptyMessage="No subcategories found."
                      allowCustom
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

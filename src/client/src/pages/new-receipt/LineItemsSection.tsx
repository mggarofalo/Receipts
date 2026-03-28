import { useState, useMemo, useCallback, useRef } from "react";
import { generateId } from "@/lib/id";
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
import {
  Popover,
  PopoverContent,
  PopoverAnchor,
} from "@/components/ui/popover";
import {
  Command,
  CommandEmpty,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import { Combobox } from "@/components/ui/combobox";
import { CurrencyInput } from "@/components/ui/currency-input";
import { Badge } from "@/components/ui/badge";
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

export interface ReceiptLineItem {
  id: string;
  receiptItemCode: string;
  description: string;
  pricingMode: "quantity" | "flat";
  quantity: number;
  unitPrice: number;
  category: string;
  subcategory: string;
}

interface LineItemsSectionProps {
  items: ReceiptLineItem[];
  onChange: (items: ReceiptLineItem[]) => void;
}

export function LineItemsSection({ items, onChange }: LineItemsSectionProps) {
  const [showSuggestions, setShowSuggestions] = useState(false);
  const suggestionsListId = "new-receipt-suggestions-list";
  const { data: categories } = useCategories();

  const categoryOptions = useMemo(
    () =>
      (
        (categories as { id: string; name: string }[] | undefined) ?? []
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
  const pricingMode = form.watch("pricingMode");

  const { data: similarItems, isFetching: isFetchingSimilar } =
    useSimilarItems(description, { enabled: showSuggestions });

  const hasResults = similarItems && similarItems.length > 0;
  const hasNoResultsMessage =
    description.length >= 2 &&
    !isFetchingSimilar &&
    similarItems &&
    similarItems.length === 0;
  const isSuggestionsOpen =
    showSuggestions && (hasResults || hasNoResultsMessage);

  const { data: categoryRecs } = useCategoryRecommendations(description, {
    enabled: description.length >= 2 && !selectedCategory,
  });

  const selectedCategoryObj = useMemo(
    () =>
      (
        (categories as { id: string; name: string }[] | undefined) ?? []
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

  const subtotal = useMemo(
    () => items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0),
    [items],
  );

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
    (e: React.KeyboardEvent<HTMLInputElement>) => {
      if (e.key === "Escape") {
        setShowSuggestions(false);
      } else if (e.key === "ArrowDown" && isSuggestionsOpen) {
        e.preventDefault();
        const list = document.getElementById(suggestionsListId);
        const firstItem = list?.querySelector(
          "[cmdk-item]",
        ) as HTMLElement | null;
        firstItem?.focus();
      }
    },
    [isSuggestionsOpen, suggestionsListId],
  );

  const handleAdd = useCallback(
    (values: ItemFormValues) => {
      const newItem: ReceiptLineItem = {
        id: generateId(),
        receiptItemCode: values.receiptItemCode ?? "",
        description: values.description,
        pricingMode: values.pricingMode,
        quantity: values.quantity,
        unitPrice: values.unitPrice,
        category: values.category,
        subcategory: values.subcategory ?? "",
      };
      onChange([...items, newItem]);
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
    },
    [form, items, onChange],
  );

  const handleRemove = useCallback(
    (id: string) => {
      onChange(items.filter((item) => item.id !== id));
    },
    [items, onChange],
  );

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <CardTitle className="text-lg">Line Items</CardTitle>
          <span className="text-sm text-muted-foreground">
            Subtotal: {formatCurrency(subtotal)}
          </span>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(handleAdd)}
            className="space-y-4"
          >
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
              <FormField
                control={form.control}
                name="description"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Description</FormLabel>
                    <Popover
                      open={isSuggestionsOpen}
                      onOpenChange={setShowSuggestions}
                    >
                      <PopoverAnchor asChild>
                        <FormControl>
                          <div className="relative">
                            <Input
                              role="combobox"
                              placeholder="Item description"
                              aria-required="true"
                              aria-autocomplete="list"
                              aria-expanded={isSuggestionsOpen}
                              aria-controls={suggestionsListId}
                              autoComplete="off"
                              {...field}
                              onFocus={() => setShowSuggestions(true)}
                              onKeyDown={handleDescriptionKeyDown}
                            />
                            {isFetchingSimilar && description.length >= 2 && (
                              <Loader2 className="absolute right-2 top-1/2 h-4 w-4 -translate-y-1/2 animate-spin text-muted-foreground" />
                            )}
                          </div>
                        </FormControl>
                      </PopoverAnchor>
                      <PopoverContent
                        className="w-[--radix-popover-trigger-width] p-0"
                        align="start"
                        onOpenAutoFocus={(e) => e.preventDefault()}
                        onInteractOutside={() => setShowSuggestions(false)}
                      >
                        <Command shouldFilter={false}>
                          <CommandList id={suggestionsListId}>
                            <CommandEmpty>No similar items found</CommandEmpty>
                            {similarItems?.map((item) => (
                              <CommandItem
                                key={`${item.name}-${item.source}`}
                                value={`${item.name} ${item.source}`}
                                onSelect={() => applySuggestion(item)}
                              >
                                <div className="flex w-full items-center justify-between">
                                  <div className="flex flex-col gap-0.5">
                                    <span className="font-medium">
                                      {item.name}
                                    </span>
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
                                </div>
                              </CommandItem>
                            ))}
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
            </div>

            <div className="grid grid-cols-2 gap-4 sm:grid-cols-[auto_auto_auto_auto_1fr] sm:items-end">
              <FormField
                control={form.control}
                name="receiptItemCode"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Item Code</FormLabel>
                    <FormControl>
                      <Input placeholder="e.g. MILK-GAL" {...field} />
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
                    <FormLabel>Pricing</FormLabel>
                    <FormControl>
                      <Combobox
                        options={[
                          { value: "quantity", label: "Quantity" },
                          { value: "flat", label: "Flat" },
                        ]}
                        value={field.value}
                        onValueChange={(v) => {
                          field.onChange(v);
                          if (v === "flat") form.setValue("quantity", 1);
                        }}
                        placeholder="Mode..."
                        searchPlaceholder="Search modes..."
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="quantity"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Qty</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        step="any"
                        min="0.01"
                        className="w-20"
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

              <div className="flex justify-end sm:mb-0.5">
                <Button type="submit" variant="secondary" size="sm">
                  <Plus className="mr-1 h-4 w-4" />
                  Add Item
                </Button>
              </div>
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
      </CardContent>
    </Card>
  );
}

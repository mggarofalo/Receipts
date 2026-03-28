import { useState, useCallback, useMemo, useEffect, useRef } from "react";
import { useNavigate } from "react-router";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useCreateCompleteReceipt } from "@/hooks/useReceipts";
import { useLocationHistory } from "@/hooks/useLocationHistory";
import { formatCurrency } from "@/lib/format";
import {
  TransactionsSection,
  type ReceiptTransaction,
} from "./TransactionsSection";
import { LineItemsSection, type ReceiptLineItem } from "./LineItemsSection";
import { BalanceSidebar } from "./BalanceSidebar";
import { Combobox } from "@/components/ui/combobox";
import { DateInput } from "@/components/ui/date-input";
import { CurrencyInput } from "@/components/ui/currency-input";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { AlertTriangle } from "lucide-react";
import { toast } from "sonner";

const headerSchema = z.object({
  location: z
    .string()
    .min(1, "Location is required")
    .max(200, "Location must be 200 characters or fewer"),
  date: z.string().min(1, "Date is required"),
  taxAmount: z.number().min(0, "Tax amount must be non-negative"),
});

type HeaderFormValues = z.output<typeof headerSchema>;

export default function NewReceiptPage() {
  usePageTitle("New Receipt");
  const navigate = useNavigate();
  const locationRef = useRef<HTMLButtonElement>(null);
  const { options: locationOptions, add: addLocation } = useLocationHistory();

  const [transactions, setTransactions] = useState<ReceiptTransaction[]>([]);
  const [items, setItems] = useState<ReceiptLineItem[]>([]);
  const [showDiscard, setShowDiscard] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { mutateAsync: createCompleteReceiptAsync } =
    useCreateCompleteReceipt();

  const form = useForm<HeaderFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(headerSchema) as any,
    defaultValues: {
      location: "",
      date: "",
      taxAmount: 0,
    },
  });

  const taxAmount = form.watch("taxAmount");
  const receiptDate = form.watch("date");

  // Auto-focus location on mount
  useEffect(() => {
    locationRef.current?.focus();
  }, []);

  const subtotal = useMemo(
    () => items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0),
    [items],
  );

  const transactionTotal = useMemo(
    () => transactions.reduce((sum, t) => sum + t.amount, 0),
    [transactions],
  );

  const hasData =
    form.getValues("location") !== "" ||
    form.getValues("date") !== "" ||
    form.getValues("taxAmount") !== 0 ||
    transactions.length > 0 ||
    items.length > 0;

  const handleCancel = useCallback(() => {
    if (hasData) {
      setShowDiscard(true);
    } else {
      navigate("/receipts");
    }
  }, [hasData, navigate]);

  const handleDiscard = useCallback(() => {
    setShowDiscard(false);
    navigate("/receipts");
  }, [navigate]);

  const handleSubmit = useCallback(async () => {
    // Validate header form first
    const valid = await form.trigger();
    if (!valid) return;

    const headerValues = form.getValues();

    if (transactions.length === 0) {
      toast.error("Add at least one transaction.");
      return;
    }

    if (items.length === 0) {
      toast.error("Add at least one line item.");
      return;
    }

    setIsSubmitting(true);
    try {
      addLocation(headerValues.location);

      const result = await createCompleteReceiptAsync(
        {
          receipt: {
            location: headerValues.location,
            date: headerValues.date,
            taxAmount: headerValues.taxAmount,
          },
          transactions: transactions.map((txn) => ({
            accountId: txn.accountId,
            amount: txn.amount,
            date: txn.date,
          })),
          items: items.map((item) => ({
            receiptItemCode: item.receiptItemCode,
            description: item.description,
            quantity: item.quantity,
            unitPrice: item.unitPrice,
            category: item.category,
            subcategory: item.subcategory,
            pricingMode: item.pricingMode,
          })),
        },
        { onSuccess: undefined, onError: undefined },
      );

      const receiptId = (result as { receipt: { id: string } }).receipt.id;

      toast.success("Receipt created successfully!");
      navigate(`/receipt-detail?id=${receiptId}&highlight=${receiptId}`);
    } catch {
      toast.error("Failed to create receipt.");
    } finally {
      setIsSubmitting(false);
    }
  }, [
    form,
    transactions,
    items,
    createCompleteReceiptAsync,
    addLocation,
    navigate,
  ]);

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold tracking-tight">New Receipt</h1>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-[1fr_280px]">
        {/* Left column — form sections */}
        <div className="space-y-6">
          {/* Receipt Header */}
          <Form {...form}>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
              <FormField
                control={form.control}
                name="location"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Location</FormLabel>
                    <FormControl>
                      <Combobox
                        ref={locationRef}
                        options={locationOptions}
                        value={field.value}
                        onValueChange={field.onChange}
                        placeholder="e.g. Walmart, Target, Costco"
                        searchPlaceholder="Search locations..."
                        emptyMessage="No saved locations."
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
                name="date"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Date</FormLabel>
                    <FormControl>
                      <DateInput
                        aria-required="true"
                        max={new Date().toISOString().split("T")[0]}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="taxAmount"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Tax Amount</FormLabel>
                    <FormControl>
                      <CurrencyInput {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
          </Form>

          {/* Transactions */}
          <TransactionsSection
            transactions={transactions}
            receiptDate={receiptDate}
            onChange={setTransactions}
          />

          {/* Balance warning between sections */}
          {transactions.length > 0 &&
            Math.abs(transactionTotal) < 0.01 &&
            taxAmount > 0 && (
              <Alert variant="destructive">
                <AlertTriangle className="h-4 w-4" />
                <AlertDescription>
                  Transaction total is {formatCurrency(transactionTotal)} but
                  tax is {formatCurrency(taxAmount)}. The receipt will be
                  unbalanced.
                </AlertDescription>
              </Alert>
            )}

          {/* Line Items */}
          <LineItemsSection items={items} onChange={setItems} />
        </div>

        {/* Right column — sticky balance sidebar */}
        <div className="hidden lg:block">
          <BalanceSidebar
            subtotal={subtotal}
            taxAmount={taxAmount}
            transactionTotal={transactionTotal}
            isSubmitting={isSubmitting}
            onSubmit={handleSubmit}
            onCancel={handleCancel}
          />
        </div>
      </div>

      {/* Mobile-only bottom bar (visible on small screens) */}
      <div className="lg:hidden">
        <BalanceSidebar
          subtotal={subtotal}
          taxAmount={taxAmount}
          transactionTotal={transactionTotal}
          isSubmitting={isSubmitting}
          onSubmit={handleSubmit}
          onCancel={handleCancel}
        />
      </div>

      <AlertDialog open={showDiscard} onOpenChange={setShowDiscard}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Discard receipt?</AlertDialogTitle>
            <AlertDialogDescription>
              You have unsaved receipt data. Are you sure you want to discard it?
              This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Continue editing</AlertDialogCancel>
            <AlertDialogAction onClick={handleDiscard}>
              Discard
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

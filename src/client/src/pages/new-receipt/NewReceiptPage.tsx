import { useState, useCallback, useMemo, useEffect, useRef } from "react";
import { useNavigate } from "react-router";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useCreateCompleteReceipt } from "@/hooks/useReceipts";
import { useLocationHistory } from "@/hooks/useLocationHistory";
import { generateId } from "@/lib/id";
import {
  TransactionsSection,
  type ReceiptTransaction,
} from "./TransactionsSection";
import { LineItemsSection, type ReceiptLineItem } from "./LineItemsSection";
import { PaymentsSection, type ReceiptPayment } from "./PaymentsSection";
import { BalanceSidebar } from "./BalanceSidebar";
import { ConfidenceIndicator } from "@/pages/scan-receipt/ConfidenceIndicator";
import type {
  ScanInitialValues,
  ReceiptConfidenceMap,
} from "@/pages/scan-receipt/types";
import { Combobox } from "@/components/ui/combobox";
import { DateInput } from "@/components/ui/date-input";
import { CurrencyInput } from "@/components/ui/currency-input";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { Button } from "@/components/ui/button";
import { ChevronDown } from "lucide-react";
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
import { toast } from "sonner";

// US phone format: 7 to 15 digits (lenient — covers international too).
const PHONE_PATTERN = /^[\d\s+()\-.]{7,20}$/;

const headerSchema = z.object({
  location: z
    .string()
    .min(1, "Location is required")
    .max(200, "Location must be 200 characters or fewer"),
  date: z.string().min(1, "Date is required"),
  taxAmount: z.number().min(0, "Tax amount must be non-negative"),
  storeAddress: z
    .string()
    .max(500, "Store address must be 500 characters or fewer")
    .optional()
    .default(""),
  storePhone: z
    .string()
    .optional()
    .default("")
    .refine((v) => !v || PHONE_PATTERN.test(v), {
      message: "Store phone is not in a recognised format",
    }),
});

type HeaderFormValues = z.output<typeof headerSchema>;

interface NewReceiptPageProps {
  initialValues?: ScanInitialValues;
  confidenceMap?: ReceiptConfidenceMap;
  pageTitle?: string;
}

export default function NewReceiptPage({
  initialValues,
  confidenceMap,
  pageTitle,
}: NewReceiptPageProps = {}) {
  usePageTitle(pageTitle ?? "New Receipt");
  const navigate = useNavigate();
  const locationRef = useRef<HTMLButtonElement>(null);
  const { options: locationOptions, add: addLocation } = useLocationHistory();

  const [transactions, setTransactions] = useState<ReceiptTransaction[]>([]);
  const [items, setItems] = useState<ReceiptLineItem[]>(() =>
    initialValues?.items.map((item) => ({
      id: generateId(),
      ...item,
    })) ?? [],
  );
  const [payments, setPayments] = useState<ReceiptPayment[]>(() =>
    initialValues?.payments.map((p) => ({
      id: generateId(),
      method: p.method,
      amount: p.amount,
      lastFour: p.lastFour,
    })) ?? [],
  );
  const [showDiscard, setShowDiscard] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { mutateAsync: createCompleteReceiptAsync } =
    useCreateCompleteReceipt();

  const form = useForm<HeaderFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(headerSchema) as any,
    defaultValues: {
      location: initialValues?.header.location ?? "",
      date: initialValues?.header.date ?? "",
      taxAmount: initialValues?.header.taxAmount ?? 0,
      storeAddress: initialValues?.header.storeAddress ?? "",
      storePhone: initialValues?.header.storePhone ?? "",
    },
  });

  const location = form.watch("location");
  const taxAmount = form.watch("taxAmount");
  const receiptDate = form.watch("date");
  const storeAddress = form.watch("storeAddress");
  const storePhone = form.watch("storePhone");

  // Auto-focus location on mount
  useEffect(() => {
    locationRef.current?.focus();
  }, []);

  const subtotal = useMemo(
    () =>
      items.reduce(
        (sum, item) =>
          sum + Math.round(item.quantity * item.unitPrice * 100) / 100,
        0,
      ),
    [items],
  );

  const transactionTotal = useMemo(
    () => transactions.reduce((sum, t) => sum + t.amount, 0),
    [transactions],
  );

  // Derived: should we show the optional sections?
  const metadata = initialValues?.metadata;
  const hasMetadata =
    !!metadata &&
    (metadata.receiptId !== "" ||
      metadata.storeNumber !== "" ||
      metadata.terminalId !== "");
  const showPaymentsSection = payments.length > 0;

  const hasData =
    location !== "" ||
    receiptDate !== "" ||
    taxAmount !== 0 ||
    transactions.length > 0 ||
    items.length > 0 ||
    payments.length > 0 ||
    (storeAddress ?? "") !== "" ||
    (storePhone ?? "") !== "";

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

      // NOTE: storeAddress/storePhone/payments/metadata/taxCode are accepted
      // and reviewable in the UI but not yet persisted by the
      // CreateCompleteReceipt API. They will round-trip once the backend is
      // extended (tracked by separate issues under the VLM epic).
      const result = await createCompleteReceiptAsync({
        receipt: {
          location: headerValues.location,
          date: headerValues.date,
          taxAmount: headerValues.taxAmount,
        },
        transactions: transactions.map((txn) => ({
          cardId: txn.cardId,
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
      });

      const receiptId = (result as { receipt: { id: string } }).receipt.id;

      toast.success("Receipt created successfully!");
      navigate(`/receipts/${receiptId}`);
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
            <div className="space-y-4">
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
                <FormField
                  control={form.control}
                  name="location"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel required className="flex items-center gap-2">
                        Location
                        <ConfidenceIndicator confidence={confidenceMap?.location} />
                      </FormLabel>
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
                      <FormLabel required className="flex items-center gap-2">
                        Date
                        <ConfidenceIndicator confidence={confidenceMap?.date} />
                      </FormLabel>
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
                      <FormLabel className="flex items-center gap-2">
                        Tax Amount
                        <ConfidenceIndicator
                          confidence={confidenceMap?.taxAmount}
                        />
                      </FormLabel>
                      <FormControl>
                        <CurrencyInput {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              {/* Optional store contact details */}
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <FormField
                  control={form.control}
                  name="storeAddress"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="flex items-center gap-2">
                        Store Address
                        <ConfidenceIndicator
                          confidence={confidenceMap?.storeAddress}
                        />
                      </FormLabel>
                      <FormControl>
                        <Input
                          placeholder="123 Main St, Springfield, IL 62701"
                          autoComplete="street-address"
                          {...field}
                          value={field.value ?? ""}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="storePhone"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="flex items-center gap-2">
                        Store Phone
                        <ConfidenceIndicator
                          confidence={confidenceMap?.storePhone}
                        />
                      </FormLabel>
                      <FormControl>
                        <Input
                          type="tel"
                          inputMode="tel"
                          placeholder="(555) 123-4567"
                          autoComplete="tel"
                          {...field}
                          value={field.value ?? ""}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
            </div>
          </Form>

          {/* Receipt Details — read-only metadata, only shown when populated */}
          {hasMetadata && (
            <ReceiptDetailsPanel
              metadata={metadata!}
              confidenceMap={confidenceMap}
            />
          )}

          {/* Detected payments — only shown when populated from a scan */}
          {showPaymentsSection && (
            <PaymentsSection
              payments={payments}
              onChange={setPayments}
              confidence={confidenceMap?.payments}
            />
          )}

          {/* Transactions */}
          <TransactionsSection
            transactions={transactions}
            defaultDate={receiptDate}
            onChange={setTransactions}
          />

          {/* Line Items */}
          <LineItemsSection
            items={items}
            onChange={setItems}
            location={location}
            itemConfidence={confidenceMap?.items}
          />
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

interface ReceiptDetailsPanelProps {
  metadata: { receiptId: string; storeNumber: string; terminalId: string };
  confidenceMap?: ReceiptConfidenceMap;
}

function ReceiptDetailsPanel({
  metadata,
  confidenceMap,
}: ReceiptDetailsPanelProps) {
  const [isOpen, setIsOpen] = useState(false);
  const rows: Array<{
    label: string;
    value: string;
    confidence?: ReceiptConfidenceMap[keyof ReceiptConfidenceMap];
  }> = [];

  if (metadata.receiptId) {
    rows.push({
      label: "Receipt ID",
      value: metadata.receiptId,
      confidence: confidenceMap?.receiptId,
    });
  }
  if (metadata.storeNumber) {
    rows.push({
      label: "Store Number",
      value: metadata.storeNumber,
      confidence: confidenceMap?.storeNumber,
    });
  }
  if (metadata.terminalId) {
    rows.push({
      label: "Terminal ID",
      value: metadata.terminalId,
      confidence: confidenceMap?.terminalId,
    });
  }

  return (
    <Card>
      <Collapsible open={isOpen} onOpenChange={setIsOpen}>
        <CardHeader className="pb-3">
          <div className="flex items-center justify-between">
            <CardTitle className="text-lg">Receipt Details</CardTitle>
            <CollapsibleTrigger asChild>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                aria-expanded={isOpen}
                aria-controls="receipt-details-content"
                aria-label={
                  isOpen ? "Collapse receipt details" : "Expand receipt details"
                }
              >
                <ChevronDown
                  className={`h-4 w-4 transition-transform ${
                    isOpen ? "rotate-180" : ""
                  }`}
                  aria-hidden="true"
                />
              </Button>
            </CollapsibleTrigger>
          </div>
        </CardHeader>
        <CollapsibleContent id="receipt-details-content">
          <CardContent>
            <dl className="grid grid-cols-[max-content_1fr] gap-x-4 gap-y-2 text-sm">
              {rows.map((row) => (
                <div key={row.label} className="contents">
                  <dt className="text-muted-foreground">{row.label}</dt>
                  <dd className="flex items-center gap-2 font-mono">
                    <span>{row.value}</span>
                    <ConfidenceIndicator
                      confidence={
                        row.confidence as
                          | ReceiptConfidenceMap[
                              | "receiptId"
                              | "storeNumber"
                              | "terminalId"]
                          | undefined
                      }
                    />
                  </dd>
                </div>
              ))}
            </dl>
          </CardContent>
        </CollapsibleContent>
      </Collapsible>
    </Card>
  );
}

import { useState, useCallback, useMemo, useEffect, useRef } from "react";
import { useNavigate } from "react-router";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useLocationHistory } from "@/hooks/useLocationHistory";
import {
  TransactionsSection,
  type ReceiptTransaction,
} from "./TransactionsSection";
import { LineItemsSection } from "./LineItemsSection";
import { PaymentsSection } from "./PaymentsSection";
import { BalanceSidebar } from "./BalanceSidebar";
import { HeaderSection } from "./HeaderSection";
import { headerSchema, type HeaderFormValues } from "./headerSchema";
import { ReceiptDetailsPanel } from "./ReceiptDetailsPanel";
import { DiscardReceiptDialog } from "./DiscardReceiptDialog";
import { useReceiptSubmit } from "./useReceiptSubmit";
import type {
  ScanInitialValues,
  ReceiptConfidenceMap,
} from "@/pages/scan-receipt/types";
import {
  initialItemsAndConfidence,
  initialPaymentsAndConfidence,
} from "@/pages/scan-receipt/proposalMappers";

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
  const { options: locationOptions } = useLocationHistory();

  const [transactions, setTransactions] = useState<ReceiptTransaction[]>([]);

  // Items, payments, and their confidence-by-id maps are initialised together
  // from the scan proposal so confidence stays correctly paired with rows
  // after additions or deletions. Building the bundle inside `useMemo` with
  // an empty dep array constructs the Map exactly once on mount; the items
  // setter then drives the editable list while the confidence map stays
  // immutable (a stale entry for a deleted row is harmless because no row
  // will ever look it up again).
  const initialItemsBundle = useMemo(
    () => initialItemsAndConfidence(initialValues, confidenceMap),
    // Build once on mount — initial scan data is captured at construction time.
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [],
  );
  const [items, setItems] = useState(initialItemsBundle.items);
  const itemConfidenceById = initialItemsBundle.itemConfidenceById;

  const initialPaymentsBundle = useMemo(
    () => initialPaymentsAndConfidence(initialValues, confidenceMap),
    // Build once on mount — initial scan data is captured at construction time.
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [],
  );
  const [payments, setPayments] = useState(initialPaymentsBundle.payments);
  const paymentConfidenceById = initialPaymentsBundle.paymentConfidenceById;

  const [showDiscard, setShowDiscard] = useState(false);

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

  const { isSubmitting, submit: handleSubmit } = useReceiptSubmit({
    form,
    transactions,
    items,
  });

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

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold tracking-tight">New Receipt</h1>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-[1fr_280px]">
        {/* Left column — form sections */}
        <div className="space-y-6">
          <HeaderSection
            form={form}
            locationOptions={locationOptions}
            locationRef={locationRef}
            confidenceMap={confidenceMap}
          />

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
              confidenceById={paymentConfidenceById}
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
            itemConfidenceById={itemConfidenceById}
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

      <DiscardReceiptDialog
        open={showDiscard}
        onOpenChange={setShowDiscard}
        onDiscard={handleDiscard}
      />
    </div>
  );
}

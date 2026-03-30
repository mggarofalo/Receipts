import { useState, useCallback, useMemo } from "react";
import { useNavigate } from "react-router";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useCreateCompleteReceipt } from "@/hooks/useReceipts";
import { useWizard } from "./useWizard";
import { WizardStepper } from "./WizardStepper";
import { Step1TripDetails } from "./Step1TripDetails";
import { Step2Transactions } from "./Step2Transactions";
import { Step3Items } from "./Step3Items";
import { Step4Review } from "./Step4Review";
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
import { Button } from "@/components/ui/button";
import { X } from "lucide-react";
import { toast } from "sonner";

export default function NewReceipt() {
  usePageTitle("New Receipt");
  const navigate = useNavigate();
  const {
    state,
    goToStep,
    goNext,
    goBack,
    setReceipt,
    setTransactions,
    setItems,
    reset,
    canGoToStep,
  } = useWizard();

  const [showDiscard, setShowDiscard] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { mutateAsync: createCompleteReceiptAsync } = useCreateCompleteReceipt();

  const hasData =
    state.receipt.location !== "" ||
    state.receipt.date !== "" ||
    state.receipt.taxAmount !== 0 ||
    state.transactions.length > 0 ||
    state.items.length > 0;

  const transactionTotal = useMemo(
    () => state.transactions.reduce((sum, t) => sum + t.amount, 0),
    [state.transactions],
  );

  const handleCancel = useCallback(() => {
    if (hasData) {
      setShowDiscard(true);
    } else {
      navigate("/receipts");
    }
  }, [hasData, navigate]);

  const handleDiscard = useCallback(() => {
    reset();
    setShowDiscard(false);
    navigate("/receipts");
  }, [reset, navigate]);

  const handleSubmit = useCallback(async () => {
    setIsSubmitting(true);
    try {
      const result = await createCompleteReceiptAsync(
        {
          receipt: {
            location: state.receipt.location,
            date: state.receipt.date,
            taxAmount: state.receipt.taxAmount,
          },
          transactions: state.transactions.map((txn) => ({
            accountId: txn.accountId,
            amount: txn.amount,
            date: txn.date,
          })),
          items: state.items.map((item) => ({
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
      reset();
      navigate(`/receipts/${receiptId}`);
    } catch {
      toast.error("Failed to create receipt.");
    } finally {
      setIsSubmitting(false);
    }
  }, [state, createCompleteReceiptAsync, reset, navigate]);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">New Receipt</h1>
        <Button variant="ghost" size="icon" onClick={handleCancel}>
          <X className="h-5 w-5" />
          <span className="sr-only">Cancel</span>
        </Button>
      </div>

      <WizardStepper
        currentStep={state.currentStep}
        completedSteps={state.completedSteps}
        onStepClick={goToStep}
        canGoToStep={canGoToStep}
      />

      {state.currentStep === 0 && (
        <Step1TripDetails
          data={state.receipt}
          onNext={(data) => {
            setReceipt(data);
            goNext();
          }}
        />
      )}
      {state.currentStep === 1 && (
        <Step2Transactions
          data={state.transactions}
          receiptDate={state.receipt.date}
          taxAmount={state.receipt.taxAmount}
          onNext={(data) => {
            setTransactions(data);
            goNext();
          }}
          onBack={goBack}
        />
      )}
      {state.currentStep === 2 && (
        <Step3Items
          data={state.items}
          taxAmount={state.receipt.taxAmount}
          transactionTotal={transactionTotal}
          onNext={(data) => {
            setItems(data);
            goNext();
          }}
          onBack={goBack}
          location={state.receipt.location}
        />
      )}
      {state.currentStep === 3 && (
        <Step4Review
          state={state}
          onBack={goBack}
          onEditStep={goToStep}
          onSubmit={handleSubmit}
          isSubmitting={isSubmitting}
        />
      )}

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

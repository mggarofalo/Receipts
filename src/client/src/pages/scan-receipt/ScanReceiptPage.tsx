import { useState, useCallback } from "react";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useReceiptScan } from "@/hooks/useReceiptScan";
import { isTimeoutError } from "@/lib/api-client";
import { ReceiptImageUpload } from "./ReceiptImageUpload";
import { OcrTextPanel } from "./OcrTextPanel";
import NewReceiptPage from "@/pages/new-receipt/NewReceiptPage";
import type { components } from "@/generated/api";
import type { ScanInitialValues, ReceiptConfidenceMap } from "./types";

type ProposedReceiptResponse = components["schemas"]["ProposedReceiptResponse"];
type ConfidenceLevel = components["schemas"]["ConfidenceLevel"];

type Status = "idle" | "uploading" | "success" | "error";

function mapProposalToInitialValues(
  proposal: ProposedReceiptResponse,
): ScanInitialValues {
  const taxAmount = Number(proposal.taxLines[0]?.amount ?? 0);

  let date = "";
  if (proposal.date) {
    // The API returns a DateOnly (YYYY-MM-DD). If it comes as ISO datetime, extract the date part.
    date = proposal.date.split("T")[0];
  }

  return {
    header: {
      location: proposal.storeName ?? "",
      date,
      taxAmount,
    },
    items: proposal.items.map((item) => ({
      receiptItemCode: item.code ?? "",
      description: item.description ?? "",
      pricingMode: "quantity" as const,
      quantity: Number(item.quantity ?? 1),
      unitPrice: Number(item.unitPrice ?? 0),
      category: "",
      subcategory: "",
    })),
  };
}

function mapProposalToConfidenceMap(
  proposal: ProposedReceiptResponse,
): ReceiptConfidenceMap {
  const map: ReceiptConfidenceMap = {};

  const addIfNotHigh = (
    key: keyof ReceiptConfidenceMap,
    confidence: ConfidenceLevel,
  ) => {
    if (confidence !== "high") {
      map[key] = confidence;
    }
  };

  addIfNotHigh("location", proposal.storeNameConfidence);
  addIfNotHigh("date", proposal.dateConfidence);

  // Use the first tax line's confidence, or the subtotal confidence as fallback
  const taxConfidence =
    proposal.taxLines[0]?.amountConfidence ?? proposal.subtotalConfidence;
  addIfNotHigh("taxAmount", taxConfidence);

  return map;
}

function getErrorMessage(error: unknown): string {
  if (isTimeoutError(error)) {
    return "Scan timed out. Please try again with a clearer image.";
  }

  if (error && typeof error === "object" && "status" in error) {
    const status = (error as { status: number }).status;
    if (status === 400 || status === 415) {
      return "Could not read the file. Please use a JPEG, PNG, or PDF.";
    }
    if (status === 422) {
      return "Could not read the file. The receipt text was not recognized.";
    }
  }

  return "Scan failed. Please try again.";
}

export default function ScanReceiptPage() {
  usePageTitle("Scan Receipt");
  const scanMutation = useReceiptScan();
  const [status, setStatus] = useState<Status>("idle");
  const [proposal, setProposal] = useState<ProposedReceiptResponse | null>(
    null,
  );
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleScan = useCallback(
    (file: File) => {
      setStatus("uploading");
      setErrorMessage(null);
      scanMutation.mutate(file, {
        onSuccess: (data) => {
          setProposal(data ?? null);
          setStatus("success");
        },
        onError: (error) => {
          setErrorMessage(getErrorMessage(error));
          setStatus("error");
        },
      });
    },
    // scanMutation.mutate is referentially stable in TanStack Query v5
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [],
  );

  if (status === "success" && proposal) {
    const initialValues = mapProposalToInitialValues(proposal);
    const confidenceMap = mapProposalToConfidenceMap(proposal);

    return (
      <div className="space-y-6">
        <OcrTextPanel
          rawText={proposal.rawOcrText}
          ocrConfidence={Number(proposal.ocrConfidence ?? 0)}
        />
        <NewReceiptPage
          initialValues={initialValues}
          confidenceMap={confidenceMap}
          pageTitle="Scan Receipt"
        />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold tracking-tight">Scan Receipt</h1>
      <div className="mx-auto max-w-lg">
        <ReceiptImageUpload
          onScan={handleScan}
          isLoading={status === "uploading"}
          error={errorMessage}
        />
      </div>
    </div>
  );
}

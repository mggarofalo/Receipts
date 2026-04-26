import { useState, useCallback } from "react";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useReceiptScan } from "@/hooks/useReceiptScan";
import { isTimeoutError } from "@/lib/api-client";
import { ReceiptImageUpload } from "./ReceiptImageUpload";
import NewReceiptPage from "@/pages/new-receipt/NewReceiptPage";
import type { components } from "@/generated/api";
import {
  mapProposalToInitialValues,
  mapProposalToConfidenceMap,
} from "./proposalMappers";

type ProposedReceiptResponse = components["schemas"]["ProposedReceiptResponse"];

type Status = "idle" | "uploading" | "success" | "error";

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
      <NewReceiptPage
        initialValues={initialValues}
        confidenceMap={confidenceMap}
        pageTitle="Scan Receipt"
      />
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

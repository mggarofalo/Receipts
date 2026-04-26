import { useState } from "react";
import { ChevronDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { ConfidenceIndicator } from "@/pages/scan-receipt/ConfidenceIndicator";
import type {
  ConfidenceLevel,
  ReceiptConfidenceMap,
} from "@/pages/scan-receipt/types";

interface ReceiptDetailsPanelProps {
  metadata: { receiptId: string; storeNumber: string; terminalId: string };
  confidenceMap?: ReceiptConfidenceMap;
}

export function ReceiptDetailsPanel({
  metadata,
  confidenceMap,
}: ReceiptDetailsPanelProps) {
  const [isOpen, setIsOpen] = useState(false);
  // The three metadata fields all have scalar `ConfidenceLevel | undefined`
  // shapes on `ReceiptConfidenceMap`. Typing the row directly as
  // `ConfidenceLevel | undefined` avoids the indexed-access widening
  // (which would let in the array shapes for `payments`/`items`) and
  // removes the downstream cast where we forward to `ConfidenceIndicator`.
  const rows: Array<{
    label: string;
    value: string;
    confidence?: ConfidenceLevel;
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
                    <ConfidenceIndicator confidence={row.confidence} />
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

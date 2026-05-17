import { useId, useState } from "react";
import {
  Collapsible,
  CollapsibleTrigger,
  CollapsibleContent,
} from "@/components/ui/collapsible";
import { Button } from "@/components/ui/button";
import { ChevronDown, ChevronRight } from "lucide-react";

interface OcrTextPanelProps {
  rawText: string;
  ocrConfidence: number;
}

export function OcrTextPanel({ rawText, ocrConfidence }: OcrTextPanelProps) {
  const [open, setOpen] = useState(false);
  const uid = useId();
  const triggerId = `ocr-text-trigger-${uid}`;
  const confidencePercent = Math.round(ocrConfidence * 100);

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <CollapsibleTrigger asChild>
        <Button
          id={triggerId}
          variant="ghost"
          className="flex w-full justify-start gap-2"
        >
          {open ? (
            <ChevronDown className="h-4 w-4" />
          ) : (
            <ChevronRight className="h-4 w-4" />
          )}
          Raw OCR Text ({confidencePercent}% confidence)
        </Button>
      </CollapsibleTrigger>
      <CollapsibleContent>
        <section aria-labelledby={triggerId}>
          <div className="rounded-md border bg-muted/50 p-4">
            <pre className="whitespace-pre-wrap text-xs font-mono">
              {rawText}
            </pre>
          </div>
        </section>
      </CollapsibleContent>
    </Collapsible>
  );
}

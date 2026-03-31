import { useState } from "react";
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
  const confidencePercent = Math.round(ocrConfidence * 100);

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <CollapsibleTrigger asChild>
        <Button variant="ghost" className="flex w-full justify-start gap-2">
          {open ? (
            <ChevronDown className="h-4 w-4" />
          ) : (
            <ChevronRight className="h-4 w-4" />
          )}
          Raw OCR Text ({confidencePercent}% confidence)
        </Button>
      </CollapsibleTrigger>
      <CollapsibleContent>
        <div className="rounded-md border bg-muted/50 p-4">
          <pre className="whitespace-pre-wrap text-xs font-mono">
            {rawText}
          </pre>
        </div>
      </CollapsibleContent>
    </Collapsible>
  );
}

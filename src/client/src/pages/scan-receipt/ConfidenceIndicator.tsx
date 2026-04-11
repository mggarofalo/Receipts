import { Badge } from "@/components/ui/badge";
import type { ConfidenceLevel } from "./types";

interface ConfidenceIndicatorProps {
  confidence: ConfidenceLevel | undefined;
  className?: string;
}

export function ConfidenceIndicator({
  confidence,
  className,
}: ConfidenceIndicatorProps) {
  if (!confidence || confidence === "High") {
    return null;
  }

  if (confidence === "Low") {
    return (
      <Badge
        variant="outline"
        className={`border-amber-500 bg-amber-50 text-amber-700 dark:bg-amber-950 dark:text-amber-400 ${className ?? ""}`}
      >
        Low confidence
      </Badge>
    );
  }

  return (
    <Badge variant="secondary" className={className}>
      Review
    </Badge>
  );
}

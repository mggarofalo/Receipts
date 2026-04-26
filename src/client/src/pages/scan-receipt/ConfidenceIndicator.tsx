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
  // "none" means the source receipt did not contain this field at all — there is no
  // value to indicate confidence in. Treat the same as "high" / undefined: render nothing.
  if (!confidence || confidence === "high" || confidence === "none") {
    return null;
  }

  if (confidence === "low") {
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

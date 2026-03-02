import { AlertTriangle, Info } from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";

interface ValidationWarning {
  property: string;
  message: string;
  severity?: number;
}

interface ValidationWarningsProps {
  warnings: ValidationWarning[];
}

export function ValidationWarnings({ warnings }: ValidationWarningsProps) {
  if (warnings.length === 0) return null;

  return (
    <div className="space-y-2">
      {warnings.map((w, i) => (
        <Alert key={`${w.property}-${i}`} variant="default" className="border-amber-500/50 bg-amber-500/5">
          {w.severity === 1 ? (
            <AlertTriangle className="h-4 w-4 text-amber-500" />
          ) : (
            <Info className="h-4 w-4 text-blue-500" />
          )}
          <AlertTitle className="text-sm">
            {w.severity === 1 ? "Warning" : "Info"}: {w.property}
          </AlertTitle>
          <AlertDescription>{w.message}</AlertDescription>
        </Alert>
      ))}
    </div>
  );
}

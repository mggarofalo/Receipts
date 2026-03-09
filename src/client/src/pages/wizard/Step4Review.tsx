import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { WizardState } from "./wizardReducer";

interface Step4Props {
  state: WizardState;
  onBack: () => void;
  onSubmit: () => void;
}

export function Step4Review({ state, onBack, onSubmit }: Step4Props) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Review & Submit</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-muted-foreground mb-4">
          Review your receipt details before submitting.
        </p>
        <div className="space-y-2 text-sm">
          <p>
            <strong>Location:</strong> {state.receipt.location || "(not set)"}
          </p>
          <p>
            <strong>Date:</strong> {state.receipt.date || "(not set)"}
          </p>
          <p>
            <strong>Tax:</strong> ${state.receipt.taxAmount.toFixed(2)}
          </p>
          <p>
            <strong>Transactions:</strong> {state.transactions.length}
          </p>
          <p>
            <strong>Items:</strong> {state.items.length}
          </p>
        </div>
        <div className="mt-6 flex justify-between">
          <Button variant="outline" onClick={onBack}>
            Back
          </Button>
          <Button onClick={onSubmit}>Submit Receipt</Button>
        </div>
      </CardContent>
    </Card>
  );
}

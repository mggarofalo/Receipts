import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { WizardTransaction } from "./wizardReducer";

interface Step2Props {
  data: WizardTransaction[];
  onNext: (data: WizardTransaction[]) => void;
  onBack: () => void;
}

export function Step2Transactions({ data, onNext, onBack }: Step2Props) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Transactions</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-muted-foreground mb-4">
          Add one or more payment methods used for this receipt.
        </p>
        <p className="text-sm text-muted-foreground">
          {data.length} transaction(s) entered
        </p>
        <div className="mt-6 flex justify-between">
          <Button variant="outline" onClick={onBack}>
            Back
          </Button>
          <Button onClick={() => onNext(data)}>Next</Button>
        </div>
      </CardContent>
    </Card>
  );
}

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { WizardReceiptItem } from "./wizardReducer";

interface Step3Props {
  data: WizardReceiptItem[];
  onNext: (data: WizardReceiptItem[]) => void;
  onBack: () => void;
}

export function Step3Items({ data, onNext, onBack }: Step3Props) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Line Items</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-muted-foreground mb-4">
          Add items from the receipt with descriptions, quantities, and prices.
        </p>
        <p className="text-sm text-muted-foreground">
          {data.length} item(s) entered
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

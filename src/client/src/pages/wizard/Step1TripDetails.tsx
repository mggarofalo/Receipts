import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { WizardReceiptData } from "./wizardReducer";

interface Step1Props {
  data: WizardReceiptData;
  onNext: (data: WizardReceiptData) => void;
}

export function Step1TripDetails({ data, onNext }: Step1Props) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Trip Details</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-muted-foreground mb-4">
          Enter the location, date, and tax amount from the receipt.
        </p>
        <p className="text-sm text-muted-foreground">
          Location: {data.location || "(not set)"} | Date:{" "}
          {data.date || "(not set)"} | Tax: ${data.taxAmount.toFixed(2)}
        </p>
        <div className="mt-6 flex justify-end">
          <Button onClick={() => onNext(data)}>Next</Button>
        </div>
      </CardContent>
    </Card>
  );
}

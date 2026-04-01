import { useEffect, useRef } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { useLocationHistory } from "@/hooks/useLocationHistory";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Combobox } from "@/components/ui/combobox";
import { DateInput } from "@/components/ui/date-input";
import { CurrencyInput } from "@/components/ui/currency-input";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import type { WizardReceiptData } from "./wizardReducer";

const tripSchema = z.object({
  location: z
    .string()
    .min(1, "Location is required")
    .max(200, "Location must be 200 characters or fewer"),
  date: z.string().min(1, "Date is required"),
  taxAmount: z.number().min(0, "Tax amount must be non-negative"),
});

type TripFormValues = z.output<typeof tripSchema>;

interface Step1Props {
  data: WizardReceiptData;
  onNext: (data: WizardReceiptData) => void;
}

export function Step1TripDetails({ data, onNext }: Step1Props) {
  const locationRef = useRef<HTMLButtonElement>(null);
  const { options: locationOptions, add: addLocation } = useLocationHistory();

  // Auto-focus the location combobox when the wizard step mounts
  useEffect(() => {
    locationRef.current?.focus();
  }, []);

  const form = useForm<TripFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(tripSchema) as any,
    defaultValues: {
      location: data.location,
      date: data.date,
      taxAmount: data.taxAmount,
    },
  });

  const handleSubmit = (values: TripFormValues) => {
    // Persist location before calling onNext intentionally: the location string
    // is valid user input regardless of whether the server mutation succeeds.
    // The user typed a real location name; saving it for future autocomplete
    // suggestions is correct even if the receipt save ultimately fails.
    addLocation(values.location);
    onNext(values);
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Trip Details</CardTitle>
      </CardHeader>
      <CardContent>
        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(handleSubmit)}
            className="space-y-4"
          >
            <FormField
              control={form.control}
              name="location"
              render={({ field }) => (
                <FormItem>
                  <FormLabel required>Location</FormLabel>
                  <FormControl>
                    <Combobox
                      ref={locationRef}
                      options={locationOptions}
                      value={field.value}
                      onValueChange={field.onChange}
                      placeholder="e.g. Walmart, Target, Costco"
                      searchPlaceholder="Search locations..."
                      emptyMessage="No saved locations."
                      allowCustom
                      aria-required="true"
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="date"
              render={({ field }) => (
                <FormItem>
                  <FormLabel required>Date</FormLabel>
                  <FormControl>
                    <DateInput
                      aria-required="true"
                      max={new Date().toISOString().split("T")[0]}
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="taxAmount"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Tax Amount</FormLabel>
                  <FormControl>
                    <CurrencyInput {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="flex justify-end pt-4">
              <Button type="submit">Next</Button>
            </div>
          </form>
        </Form>
      </CardContent>
    </Card>
  );
}

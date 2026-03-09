import { useRef, useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod/v4";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
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
  const locationRef = useRef<HTMLInputElement>(null);

  const form = useForm<TripFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(tripSchema) as any,
    defaultValues: {
      location: data.location,
      date: data.date,
      taxAmount: data.taxAmount,
    },
  });

  useEffect(() => {
    locationRef.current?.focus();
  }, []);

  const handleSubmit = (values: TripFormValues) => {
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
                  <FormLabel>Location</FormLabel>
                  <FormControl>
                    <Input
                      aria-required="true"
                      placeholder="e.g. Walmart, Target, Costco"
                      {...field}
                      ref={(el) => {
                        field.ref(el);
                        (
                          locationRef as React.MutableRefObject<HTMLInputElement | null>
                        ).current = el;
                      }}
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
                  <FormLabel>Date</FormLabel>
                  <FormControl>
                    <Input
                      type="date"
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

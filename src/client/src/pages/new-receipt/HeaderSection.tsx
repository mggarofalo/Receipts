import type { Ref } from "react";
import type { UseFormReturn } from "react-hook-form";
import { Combobox } from "@/components/ui/combobox";
import { CurrencyInput } from "@/components/ui/currency-input";
import { DateInput } from "@/components/ui/date-input";
import { Input } from "@/components/ui/input";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { ConfidenceIndicator } from "@/pages/scan-receipt/ConfidenceIndicator";
import type { ReceiptConfidenceMap } from "@/pages/scan-receipt/types";
import type { HeaderFormValues } from "./headerSchema";

interface HeaderSectionProps {
  form: UseFormReturn<HeaderFormValues>;
  locationOptions: Array<{ value: string; label: string }>;
  locationRef: Ref<HTMLButtonElement>;
  confidenceMap?: ReceiptConfidenceMap;
}

export function HeaderSection({
  form,
  locationOptions,
  locationRef,
  confidenceMap,
}: HeaderSectionProps) {
  return (
    <Form {...form}>
      <div className="space-y-4">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <FormField
            control={form.control}
            name="location"
            render={({ field }) => (
              <FormItem>
                <FormLabel required className="flex items-center gap-2">
                  Location
                  <ConfidenceIndicator confidence={confidenceMap?.location} />
                </FormLabel>
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
                <FormLabel required className="flex items-center gap-2">
                  Date
                  <ConfidenceIndicator confidence={confidenceMap?.date} />
                </FormLabel>
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
                <FormLabel className="flex items-center gap-2">
                  Tax Amount
                  <ConfidenceIndicator confidence={confidenceMap?.taxAmount} />
                </FormLabel>
                <FormControl>
                  <CurrencyInput {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        {/* Optional store contact details */}
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="storeAddress"
            render={({ field }) => (
              <FormItem>
                <FormLabel className="flex items-center gap-2">
                  Store Address
                  <ConfidenceIndicator
                    confidence={confidenceMap?.storeAddress}
                  />
                </FormLabel>
                <FormControl>
                  <Input
                    placeholder="123 Main St, Springfield, IL 62701"
                    autoComplete="street-address"
                    {...field}
                    value={field.value ?? ""}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="storePhone"
            render={({ field }) => (
              <FormItem>
                <FormLabel className="flex items-center gap-2">
                  Store Phone
                  <ConfidenceIndicator
                    confidence={confidenceMap?.storePhone}
                  />
                </FormLabel>
                <FormControl>
                  <Input
                    type="tel"
                    inputMode="tel"
                    placeholder="(555) 123-4567"
                    autoComplete="tel"
                    {...field}
                    value={field.value ?? ""}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>
      </div>
    </Form>
  );
}

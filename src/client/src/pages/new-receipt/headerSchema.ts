import { z } from "zod/v4";

// US phone format: 7 to 15 digits (lenient — covers international too).
const PHONE_PATTERN = /^[\d\s+()\-.]{7,20}$/;

export const headerSchema = z.object({
  location: z
    .string()
    .min(1, "Location is required")
    .max(200, "Location must be 200 characters or fewer"),
  date: z.string().min(1, "Date is required"),
  taxAmount: z.number().min(0, "Tax amount must be non-negative"),
  storeAddress: z
    .string()
    .max(500, "Store address must be 500 characters or fewer")
    .optional()
    .default(""),
  storePhone: z
    .string()
    .optional()
    .default("")
    .refine((v) => !v || PHONE_PATTERN.test(v), {
      message: "Store phone is not in a recognised format",
    }),
});

export type HeaderFormValues = z.output<typeof headerSchema>;

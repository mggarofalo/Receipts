import type { components } from "@/generated/api";

type EnumMetadataResponse = components["schemas"]["EnumMetadataResponse"];

export const enumMetadata: EnumMetadataResponse = {
  adjustmentTypes: [
    { value: "discount", label: "Discount" },
    { value: "surcharge", label: "Surcharge" },
  ],
  authEventTypes: [{ value: "login", label: "Login" }],
  pricingModes: [
    { value: "quantity", label: "Quantity" },
    { value: "flat", label: "Flat" },
  ],
  auditActions: [{ value: "created", label: "Created" }],
  entityTypes: [{ value: "receipt", label: "Receipt" }],
};

import type { components } from "@/generated/api";

type EnumMetadataResponse = components["schemas"]["EnumMetadataResponse"];

export const enumMetadata: EnumMetadataResponse = {
  adjustmentTypes: [
    { value: "discount", label: "Discount" },
    { value: "surcharge", label: "Surcharge" },
  ],
  authEventTypes: [{ value: "login", label: "Login" }],
  auditActions: [{ value: "created", label: "Created" }],
  entityTypes: [{ value: "receipt", label: "Receipt" }],
};

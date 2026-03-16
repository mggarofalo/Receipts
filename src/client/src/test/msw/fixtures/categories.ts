import type { components } from "@/generated/api";

type CategoryResponse = components["schemas"]["CategoryResponse"];

export const categories: CategoryResponse[] = [
  {
    id: "dddd1111-1111-1111-1111-111111111111",
    name: "Groceries",
    description: "Food and household items",
  },
  {
    id: "dddd2222-2222-2222-2222-222222222222",
    name: "Tools",
    description: "Hardware and tools",
  },
  {
    id: "dddd3333-3333-3333-3333-333333333333",
    name: "Electronics",
    description: null,
  },
];

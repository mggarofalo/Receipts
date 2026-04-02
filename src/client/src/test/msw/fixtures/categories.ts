import type { components } from "@/generated/api";

type CategoryResponse = components["schemas"]["CategoryResponse"];

export const categories: CategoryResponse[] = [
  {
    id: "dddd1111-1111-1111-1111-111111111111",
    name: "Groceries",
    description: "Food and household items",
    isActive: true,
  },
  {
    id: "dddd2222-2222-2222-2222-222222222222",
    name: "Tools",
    description: "Hardware and tools",
    isActive: true,
  },
  {
    id: "dddd3333-3333-3333-3333-333333333333",
    name: "Electronics",
    description: null,
    isActive: true,
  },
  {
    id: "dddd4444-4444-4444-4444-444444444444",
    name: "Clothing",
    description: "Archived clothing category",
    isActive: false,
  },
];

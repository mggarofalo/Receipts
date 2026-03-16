import type { components } from "@/generated/api";

type SubcategoryResponse = components["schemas"]["SubcategoryResponse"];

export const subcategories: SubcategoryResponse[] = [
  {
    id: "eeee1111-1111-1111-1111-111111111111",
    name: "Dairy",
    categoryId: "dddd1111-1111-1111-1111-111111111111",
    description: "Milk, cheese, yogurt",
  },
  {
    id: "eeee2222-2222-2222-2222-222222222222",
    name: "Bakery",
    categoryId: "dddd1111-1111-1111-1111-111111111111",
    description: "Bread, pastries",
  },
  {
    id: "eeee3333-3333-3333-3333-333333333333",
    name: "Power Tools",
    categoryId: "dddd2222-2222-2222-2222-222222222222",
    description: null,
  },
];

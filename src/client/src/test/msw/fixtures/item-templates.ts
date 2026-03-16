import type { components } from "@/generated/api";

type ItemTemplateResponse = components["schemas"]["ItemTemplateResponse"];

export const itemTemplates: ItemTemplateResponse[] = [
  {
    id: "ffff1111-1111-1111-1111-111111111111",
    name: "Whole Milk",
    description: "1 gallon whole milk",
    defaultCategory: "Groceries",
    defaultSubcategory: "Dairy",
    defaultUnitPrice: 3.99,
    defaultUnitPriceCurrency: "USD",
    defaultPricingMode: "quantity",
    defaultItemCode: "MILK-001",
  },
  {
    id: "ffff2222-2222-2222-2222-222222222222",
    name: "Sourdough Bread",
    description: null,
    defaultCategory: "Groceries",
    defaultSubcategory: "Bakery",
    defaultUnitPrice: 4.5,
    defaultUnitPriceCurrency: "USD",
    defaultPricingMode: "flat",
    defaultItemCode: null,
  },
  {
    id: "ffff3333-3333-3333-3333-333333333333",
    name: "Drill Bit Set",
    description: "10-piece HSS drill bit set",
    defaultCategory: "Tools",
    defaultSubcategory: "Power Tools",
    defaultUnitPrice: 19.99,
    defaultUnitPriceCurrency: "USD",
    defaultPricingMode: "flat",
    defaultItemCode: "HW-200",
  },
];

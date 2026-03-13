import type { components } from "@/generated/api";

type AccountResponse = components["schemas"]["AccountResponse"];

export const accounts: AccountResponse[] = [
  {
    id: "11111111-1111-1111-1111-111111111111",
    accountCode: "1000",
    name: "Cash",
    isActive: true,
  },
  {
    id: "22222222-2222-2222-2222-222222222222",
    accountCode: "2000",
    name: "Credit Card",
    isActive: true,
  },
  {
    id: "33333333-3333-3333-3333-333333333333",
    accountCode: "3000",
    name: "Savings",
    isActive: false,
  },
];

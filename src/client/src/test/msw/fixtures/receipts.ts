export const receipts = [
  {
    id: "bbbb1111-1111-1111-1111-111111111111",
    location: "Walmart",
    date: "2024-06-15",
    taxAmount: 5.25,
  },
  {
    id: "bbbb2222-2222-2222-2222-222222222222",
    location: "Target",
    date: "2024-06-20",
    taxAmount: 3.50,
  },
];

export const tripResponse = {
  receipt: {
    receipt: {
      id: "bbbb1111-1111-1111-1111-111111111111",
      location: "Walmart",
      date: "2024-06-15",
      taxAmount: 5.25,
    },
    items: [
      {
        id: "item-1",
        description: "Whole Milk",
        quantity: 2,
        unitPrice: 3.99,
        totalPrice: 7.98,
        pricingMode: "quantity",
      },
    ],
    subtotal: 7.98,
    adjustmentTotal: -1.0,
    adjustments: [
      {
        id: "adj-1",
        type: "Discount",
        description: "Store coupon",
        amount: -1.0,
      },
    ],
    expectedTotal: 12.23,
    warnings: [],
  },
  transactions: [
    {
      transaction: {
        id: "txn-1",
        amount: 12.23,
        date: "2024-06-15",
      },
      account: {
        accountCode: "1000",
        name: "Cash",
        isActive: true,
      },
    },
  ],
  warnings: [],
};

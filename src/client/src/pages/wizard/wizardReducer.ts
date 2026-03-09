export interface WizardReceiptData {
  location: string;
  date: string;
  taxAmount: number;
}

export interface WizardTransaction {
  id: string;
  accountId: string;
  amount: number;
  date: string;
}

export interface WizardReceiptItem {
  id: string;
  receiptItemCode: string;
  description: string;
  pricingMode: "quantity" | "flat";
  quantity: number;
  unitPrice: number;
  category: string;
  subcategory: string;
}

export interface WizardState {
  currentStep: number;
  receipt: WizardReceiptData;
  transactions: WizardTransaction[];
  items: WizardReceiptItem[];
  completedSteps: Set<number>;
}

export type WizardAction =
  | { type: "SET_STEP"; step: number }
  | { type: "SET_RECEIPT"; data: WizardReceiptData }
  | { type: "SET_TRANSACTIONS"; data: WizardTransaction[] }
  | { type: "SET_ITEMS"; data: WizardReceiptItem[] }
  | { type: "MARK_STEP_COMPLETE"; step: number }
  | { type: "NEXT" }
  | { type: "BACK" }
  | { type: "RESET" };

export const STEP_LABELS = [
  "Trip Details",
  "Transactions",
  "Line Items",
  "Review",
] as const;

export const INITIAL_STATE: WizardState = {
  currentStep: 0,
  receipt: { location: "", date: "", taxAmount: 0 },
  transactions: [],
  items: [],
  completedSteps: new Set<number>(),
};

export function wizardReducer(
  state: WizardState,
  action: WizardAction,
): WizardState {
  switch (action.type) {
    case "SET_STEP":
      return { ...state, currentStep: action.step };
    case "SET_RECEIPT":
      return { ...state, receipt: action.data };
    case "SET_TRANSACTIONS":
      return { ...state, transactions: action.data };
    case "SET_ITEMS":
      return { ...state, items: action.data };
    case "MARK_STEP_COMPLETE": {
      if (state.completedSteps.has(action.step)) return state;
      const next = new Set(state.completedSteps);
      next.add(action.step);
      return { ...state, completedSteps: next };
    }
    case "NEXT": {
      const next = new Set(state.completedSteps);
      next.add(state.currentStep);
      return { ...state, completedSteps: next, currentStep: state.currentStep + 1 };
    }
    case "BACK":
      return { ...state, currentStep: Math.max(0, state.currentStep - 1) };
    case "RESET":
      return { ...INITIAL_STATE, completedSteps: new Set<number>() };
    default:
      return state;
  }
}

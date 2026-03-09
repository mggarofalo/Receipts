import { useReducer, useCallback, useMemo } from "react";
import {
  wizardReducer,
  INITIAL_STATE,
  STEP_LABELS,
  type WizardState,
  type WizardReceiptData,
  type WizardTransaction,
  type WizardReceiptItem,
} from "./wizardReducer";

export function useWizard() {
  const [state, dispatch] = useReducer(wizardReducer, INITIAL_STATE);

  const goToStep = useCallback((step: number) => {
    dispatch({ type: "SET_STEP", step });
  }, []);

  const goNext = useCallback(() => {
    dispatch({ type: "NEXT" });
  }, []);

  const goBack = useCallback(() => {
    dispatch({ type: "BACK" });
  }, []);

  const setReceipt = useCallback((data: WizardReceiptData) => {
    dispatch({ type: "SET_RECEIPT", data });
  }, []);

  const setTransactions = useCallback((data: WizardTransaction[]) => {
    dispatch({ type: "SET_TRANSACTIONS", data });
  }, []);

  const setItems = useCallback((data: WizardReceiptItem[]) => {
    dispatch({ type: "SET_ITEMS", data });
  }, []);

  const reset = useCallback(() => {
    dispatch({ type: "RESET" });
  }, []);

  const canGoToStep = useCallback(
    (step: number) => {
      if (step === 0) return true;
      for (let i = 0; i < step; i++) {
        if (!state.completedSteps.has(i)) return false;
      }
      return true;
    },
    [state.completedSteps],
  );

  const isLastStep = state.currentStep === STEP_LABELS.length - 1;
  const isFirstStep = state.currentStep === 0;

  const result = useMemo(
    () => ({
      state,
      goToStep,
      goNext,
      goBack,
      setReceipt,
      setTransactions,
      setItems,
      reset,
      canGoToStep,
      isLastStep,
      isFirstStep,
    }),
    [
      state,
      goToStep,
      goNext,
      goBack,
      setReceipt,
      setTransactions,
      setItems,
      reset,
      canGoToStep,
      isLastStep,
      isFirstStep,
    ],
  );

  return result;
}

export type { WizardState, WizardReceiptData, WizardTransaction, WizardReceiptItem };

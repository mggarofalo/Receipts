import { useEffect, useState } from "react";

const WARNING_DELAY_MS = 5_000;

export function useSubmitTimeout(isSubmitting: boolean): {
  isWarning: boolean;
} {
  const [isWarning, setIsWarning] = useState(false);

  useEffect(() => {
    if (!isSubmitting) {
      setIsWarning(false);
      return;
    }

    const timer = setTimeout(() => setIsWarning(true), WARNING_DELAY_MS);
    return () => clearTimeout(timer);
  }, [isSubmitting]);

  return { isWarning };
}

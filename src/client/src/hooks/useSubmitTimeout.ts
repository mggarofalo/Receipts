import { useEffect, useState } from "react";

const WARNING_DELAY_MS = 5_000;

export function useSubmitTimeout(isSubmitting: boolean): {
  isWarning: boolean;
} {
  const [warningGeneration, setWarningGeneration] = useState<number | null>(
    null,
  );
  const [submitGeneration, setSubmitGeneration] = useState(0);

  // Track submission transitions: increment generation each time isSubmitting becomes true
  const [wasSubmitting, setWasSubmitting] = useState(false);
  if (isSubmitting && !wasSubmitting) {
    setSubmitGeneration((g) => g + 1);
    setWasSubmitting(true);
  } else if (!isSubmitting && wasSubmitting) {
    setWasSubmitting(false);
  }

  useEffect(() => {
    if (!isSubmitting) {
      return;
    }

    const timer = setTimeout(
      () => setWarningGeneration(submitGeneration),
      WARNING_DELAY_MS,
    );
    return () => clearTimeout(timer);
  }, [isSubmitting, submitGeneration]);

  return { isWarning: isSubmitting && warningGeneration === submitGeneration };
}

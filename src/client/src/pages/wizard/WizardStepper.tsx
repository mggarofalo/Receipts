import { STEP_LABELS } from "./wizardReducer";
import { cn } from "@/lib/utils";
import { Check } from "lucide-react";

interface WizardStepperProps {
  currentStep: number;
  completedSteps: Set<number>;
  onStepClick: (step: number) => void;
  canGoToStep: (step: number) => boolean;
}

export function WizardStepper({
  currentStep,
  completedSteps,
  onStepClick,
  canGoToStep,
}: WizardStepperProps) {
  return (
    <nav aria-label="Receipt entry progress" className="mb-8">
      <ol className="flex items-center gap-2">
        {STEP_LABELS.map((label, index) => {
          const isCompleted = completedSteps.has(index);
          const isCurrent = index === currentStep;
          const isClickable = canGoToStep(index);

          return (
            <li key={label} className="flex flex-1 items-center">
              <button
                type="button"
                onClick={() => isClickable && onStepClick(index)}
                disabled={!isClickable}
                className={cn(
                  "flex w-full flex-col items-center gap-1.5",
                  isClickable
                    ? "cursor-pointer"
                    : "cursor-not-allowed opacity-50",
                )}
                aria-current={isCurrent ? "step" : undefined}
              >
                <div
                  className={cn(
                    "flex h-8 w-8 items-center justify-center rounded-full border-2 text-sm font-medium transition-colors",
                    isCurrent &&
                      "border-primary bg-primary text-primary-foreground",
                    isCompleted &&
                      !isCurrent &&
                      "border-primary bg-primary/10 text-primary",
                    !isCurrent &&
                      !isCompleted &&
                      "border-muted-foreground/30 text-muted-foreground",
                  )}
                >
                  {isCompleted && !isCurrent ? (
                    <Check className="h-4 w-4" />
                  ) : (
                    index + 1
                  )}
                </div>
                <span
                  className={cn(
                    "text-xs font-medium",
                    isCurrent && "text-foreground",
                    !isCurrent && "text-muted-foreground",
                  )}
                >
                  {label}
                </span>
              </button>
              {index < STEP_LABELS.length - 1 && (
                <div
                  className={cn(
                    "mx-2 mb-5 h-0.5 flex-1",
                    completedSteps.has(index)
                      ? "bg-primary"
                      : "bg-muted-foreground/20",
                  )}
                />
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}

import { TriangleAlertIcon } from "lucide-react";
import { useSubmitTimeout } from "@/hooks/useSubmitTimeout";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";

interface SubmitButtonProps
  extends Omit<React.ComponentProps<typeof Button>, "type" | "children"> {
  isSubmitting: boolean;
  label: string;
  loadingLabel: string;
}

export function SubmitButton({
  isSubmitting,
  label,
  loadingLabel,
  disabled,
  ...props
}: SubmitButtonProps) {
  const { isWarning } = useSubmitTimeout(isSubmitting);

  return (
    <Button type="submit" disabled={disabled || isSubmitting} {...props}>
      {isSubmitting &&
        (isWarning ? (
          <TriangleAlertIcon className="size-4 text-amber-500" />
        ) : (
          <Spinner size="sm" />
        ))}
      {isSubmitting ? (isWarning ? "Still working..." : loadingLabel) : label}
    </Button>
  );
}

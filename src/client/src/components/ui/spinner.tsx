import { Loader2Icon } from "lucide-react";
import { cn } from "@/lib/utils";

const sizeMap = {
  sm: "size-4",
  md: "size-6",
  lg: "size-8",
} as const;

interface SpinnerProps {
  size?: keyof typeof sizeMap;
  className?: string;
}

export function Spinner({ size = "md", className }: SpinnerProps) {
  return (
    <Loader2Icon
      className={cn("animate-spin", sizeMap[size], className)}
      aria-hidden="true"
    />
  );
}

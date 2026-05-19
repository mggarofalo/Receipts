import { forwardRef, type ComponentPropsWithoutRef } from "react";
import { cn } from "@/lib/utils";

export type YnabStatus = "synced" | "pending" | "error" | "none";

const STATUS: Record<YnabStatus, { label: string; dot: string }> = {
  synced: { label: "YNAB", dot: "var(--pos)" },
  pending: { label: "Pending", dot: "var(--warn)" },
  error: { label: "Error", dot: "var(--neg)" },
  none: { label: "—", dot: "var(--mute-2)" },
};

export interface YnabChipProps extends ComponentPropsWithoutRef<"span"> {
  status: YnabStatus;
}

/**
 * Indicates a receipt's YNAB sync state with a coloured dot.
 *
 * @example
 * <YnabChip status="synced" />
 */
export const YnabChip = forwardRef<HTMLSpanElement, YnabChipProps>(
  ({ status, className, ...props }, ref) => {
    const { label, dot } = STATUS[status];
    return (
      <span
        ref={ref}
        className={cn(
          "ynab-chip inline-flex items-center gap-1.5 rounded-full border border-[var(--line-2)] bg-[var(--surface-2)] px-2 py-0.5 font-mono text-[10.5px] uppercase leading-none tracking-[0.04em] text-[var(--ink-2)]",
          className,
        )}
        {...props}
      >
        <span
          aria-hidden
          className="size-1.5 shrink-0 rounded-full"
          style={{ backgroundColor: dot }}
        />
        {label}
      </span>
    );
  },
);
YnabChip.displayName = "YnabChip";

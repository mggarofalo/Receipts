import { forwardRef, type ComponentPropsWithoutRef } from "react";
import { cn } from "@/lib/utils";

export type YnabStatus = "synced" | "pending" | "error" | "none";

const STATUS: Record<
  YnabStatus,
  { label: string; chip: string; ariaLabel: string }
> = {
  synced: { label: "YNAB", chip: "chip pos", ariaLabel: "YNAB: synced" },
  pending: { label: "Pending", chip: "chip warn", ariaLabel: "YNAB: pending" },
  error: { label: "Error", chip: "chip neg", ariaLabel: "YNAB: error" },
  none: { label: "—", chip: "chip ghost", ariaLabel: "YNAB: not synced" },
};

export interface YnabChipProps extends ComponentPropsWithoutRef<"span"> {
  status: YnabStatus;
}

/**
 * Indicates a receipt's YNAB sync state — a coloured chip with a dot.
 *
 * @example
 * <YnabChip status="synced" />
 */
export const YnabChip = forwardRef<HTMLSpanElement, YnabChipProps>(
  ({ status, className, style, ...props }, ref) => {
    const { label, chip, ariaLabel } = STATUS[status];
    return (
      <span
        ref={ref}
        className={cn(chip, "ynab-chip", className)}
        style={status === "none" ? { opacity: 0.5, ...style } : style}
        aria-label={ariaLabel}
        {...props}
      >
        <span
          className="dot"
          aria-hidden="true"
          style={
            status === "none" ? { background: "var(--mute-2)" } : undefined
          }
        />
        <span aria-hidden={status === "none" ? "true" : undefined}>{label}</span>
      </span>
    );
  },
);
YnabChip.displayName = "YnabChip";

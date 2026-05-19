import { forwardRef, type ComponentPropsWithoutRef } from "react";
import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "@/lib/utils";

// Variant tints use color-mix against the semantic tokens so a single accent
// value drives both the fill and the text colour.
const chipVariants = cva(
  "inline-flex items-center gap-1.5 rounded-full border px-2 py-0.5 font-mono text-[10.5px] uppercase leading-none tracking-[0.04em]",
  {
    variants: {
      variant: {
        default:
          "border-[var(--line-2)] bg-[var(--surface-2)] text-[var(--ink-2)]",
        pos: "border-transparent bg-[color-mix(in_srgb,var(--pos)_16%,transparent)] text-[var(--pos)]",
        neg: "border-transparent bg-[color-mix(in_srgb,var(--neg)_16%,transparent)] text-[var(--neg)]",
        warn: "border-transparent bg-[color-mix(in_srgb,var(--warn)_18%,transparent)] text-[var(--warn)]",
        solid: "border-transparent bg-[var(--accent)] text-[var(--accent-ink)]",
        ghost: "border-transparent bg-transparent text-[var(--mute)]",
      },
    },
    defaultVariants: { variant: "default" },
  },
);

export interface ChipProps
  extends ComponentPropsWithoutRef<"span">, VariantProps<typeof chipVariants> {}

/**
 * A compact status pill.
 *
 * @example
 * <Chip variant="pos">Reconciled</Chip>
 */
export const Chip = forwardRef<HTMLSpanElement, ChipProps>(
  ({ className, variant, ...props }, ref) => (
    <span
      ref={ref}
      className={cn(chipVariants({ variant }), className)}
      {...props}
    />
  ),
);
Chip.displayName = "Chip";

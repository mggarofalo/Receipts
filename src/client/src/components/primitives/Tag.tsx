import { forwardRef, type ComponentPropsWithoutRef } from "react";
import { cn } from "@/lib/utils";

/**
 * A small inline label for categories, stores, and similar metadata.
 *
 * @example
 * <Tag>Groceries</Tag>
 */
export const Tag = forwardRef<
  HTMLSpanElement,
  ComponentPropsWithoutRef<"span">
>(({ className, ...props }, ref) => (
  <span
    ref={ref}
    className={cn(
      "inline-flex items-center rounded-md border border-[var(--line)] bg-[var(--bg-2)] px-2 py-[3px] text-[11.5px] text-[var(--ink-2)]",
      className,
    )}
    {...props}
  />
));
Tag.displayName = "Tag";

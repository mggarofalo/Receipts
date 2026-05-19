import { forwardRef, type ComponentPropsWithoutRef } from "react";
import { cn } from "@/lib/utils";

export interface CheckboxProps extends Omit<
  ComponentPropsWithoutRef<"button">,
  "type"
> {
  /** Whether the box is checked. */
  on: boolean;
}

/**
 * The design-system checkbox: a 15×15 box that fills with the accent when on.
 * For richer form integration prefer `@/components/ui/checkbox`.
 *
 * @example
 * <Checkbox on={selected} onClick={() => setSelected((v) => !v)} />
 */
export const Checkbox = forwardRef<HTMLButtonElement, CheckboxProps>(
  ({ on, className, ...props }, ref) => (
    <button
      ref={ref}
      type="button"
      role="checkbox"
      aria-checked={on}
      className={cn(
        "inline-flex size-[15px] shrink-0 items-center justify-center rounded-[3px] border transition-colors",
        on
          ? "border-[var(--accent)] bg-[var(--accent)]"
          : "border-[var(--line-2)] bg-transparent",
        className,
      )}
      {...props}
    >
      {on && (
        <span
          aria-hidden
          className="mb-px h-1 w-[7px] -rotate-45 border-b-2 border-l-2"
          style={{ borderColor: "var(--accent-ink)" }}
        />
      )}
    </button>
  ),
);
Checkbox.displayName = "Checkbox";

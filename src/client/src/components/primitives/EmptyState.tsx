import {
  forwardRef,
  type ComponentType,
  type ComponentPropsWithoutRef,
  type ReactNode,
} from "react";
import type { LucideProps } from "lucide-react";
import { cn } from "@/lib/utils";

export interface EmptyStateProps extends ComponentPropsWithoutRef<"div"> {
  /** An `Icon.*` member, or any lucide-compatible icon component. */
  icon: ComponentType<LucideProps>;
  title: string;
  body?: ReactNode;
  /** Action buttons rendered below the copy. */
  actions?: ReactNode;
}

/**
 * Shown wherever a list or surface has no data.
 *
 * @example
 * <EmptyState
 *   icon={Icon.Inbox}
 *   title="No receipts yet"
 *   body="Add your first receipt to get started."
 *   actions={<Button>New receipt</Button>}
 * />
 */
export const EmptyState = forwardRef<HTMLDivElement, EmptyStateProps>(
  ({ icon: IconComponent, title, body, actions, className, ...props }, ref) => (
    <div
      ref={ref}
      className={cn(
        "flex flex-col items-center gap-3 px-6 py-12 text-center",
        className,
      )}
      {...props}
    >
      <span className="flex size-[60px] items-center justify-center rounded-full border border-[var(--line)] bg-[var(--surface)] text-[var(--mute)]">
        <IconComponent size={24} aria-hidden />
      </span>
      <h3 className="font-serif text-[22px] leading-tight text-[var(--ink)]">
        {title}
      </h3>
      {body && (
        <p className="max-w-sm text-[13px] text-[var(--mute)]">{body}</p>
      )}
      {actions && (
        <div className="mt-1 flex flex-wrap items-center justify-center gap-2">
          {actions}
        </div>
      )}
    </div>
  ),
);
EmptyState.displayName = "EmptyState";

import {
  forwardRef,
  type ComponentPropsWithoutRef,
  type ReactNode,
} from "react";
import { cn } from "@/lib/utils";

export interface PageHeadProps
  extends Omit<ComponentPropsWithoutRef<"div">, "title"> {
  title: ReactNode;
  /** Mono uppercase eyebrow rendered above the title. */
  sub?: ReactNode;
  /** Action cluster; wraps below the title under 760px. */
  actions?: ReactNode;
}

/**
 * Standard page header: serif title, optional eyebrow, optional action cluster.
 *
 * @example
 * <PageHead title="Receipts" sub="42 total" actions={<Button>New</Button>} />
 */
export const PageHead = forwardRef<HTMLDivElement, PageHeadProps>(
  ({ title, sub, actions, className, ...props }, ref) => (
    <div
      ref={ref}
      className={cn(
        "flex flex-wrap items-end justify-between gap-x-6 gap-y-3",
        className,
      )}
      {...props}
    >
      <div className="flex flex-col gap-1">
        {sub && <span className="page-sub">{sub}</span>}
        <h1 className="page-title text-[var(--ink)]">{title}</h1>
      </div>
      {actions && (
        <div className="flex flex-wrap items-center gap-2">{actions}</div>
      )}
    </div>
  ),
);
PageHead.displayName = "PageHead";

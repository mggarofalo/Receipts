import { Skeleton } from "@/components/ui/skeleton";
import { Card, CardContent, CardHeader } from "@/components/ui/card";

interface CardSkeletonProps {
  lines?: number;
  /**
   * When true, omits the role="status" live region wrapper. Use this when
   * the CardSkeleton is rendered inside an existing live region (e.g. a parent
   * that already has role="status" aria-live="polite") to avoid redundant or
   * repeated announcements.
   */
  silent?: boolean;
}

export function CardSkeleton({ lines = 3, silent = false }: CardSkeletonProps) {
  const card = (
    <Card aria-hidden={!silent ? true : undefined}>
      <CardHeader>
        <Skeleton className="h-5 w-40" />
        <Skeleton className="h-4 w-64" />
      </CardHeader>
      <CardContent className="space-y-3">
        {Array.from({ length: lines }).map((_, i) => (
          <Skeleton key={i} className="h-4 w-full" />
        ))}
      </CardContent>
    </Card>
  );

  if (silent) {
    return card;
  }

  return (
    <div role="status" aria-live="polite" aria-busy="true">
      <span className="sr-only">Loading…</span>
      {card}
    </div>
  );
}

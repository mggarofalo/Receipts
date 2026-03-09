import { ChartCard } from "@/components/charts";
import type { DateRange } from "@/hooks/useDashboard";

interface SpendingByCategoryWidgetProps {
  dateRange: DateRange;
  className?: string;
}

export function SpendingByCategoryWidget({
  dateRange: _dateRange,
  className,
}: SpendingByCategoryWidgetProps) {
  return (
    <ChartCard title="Spending by Category" className={className}>
      <p className="text-muted-foreground text-sm">Category breakdown chart will appear here.</p>
    </ChartCard>
  );
}

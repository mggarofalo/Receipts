import { ChartCard } from "@/components/charts";
import type { DateRange } from "@/hooks/useDashboard";

interface SpendingByAccountWidgetProps {
  dateRange: DateRange;
  className?: string;
}

export function SpendingByAccountWidget({
  dateRange: _dateRange,
  className,
}: SpendingByAccountWidgetProps) {
  return (
    <ChartCard title="Spending by Account" className={className}>
      <p className="text-muted-foreground text-sm">Account breakdown chart will appear here.</p>
    </ChartCard>
  );
}

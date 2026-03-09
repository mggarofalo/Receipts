import { ChartCard } from "@/components/charts";
import type { DateRange } from "@/hooks/useDashboard";

interface SpendingOverTimeWidgetProps {
  dateRange: DateRange;
  className?: string;
}

export function SpendingOverTimeWidget({
  dateRange: _dateRange,
  className,
}: SpendingOverTimeWidgetProps) {
  return (
    <ChartCard title="Spending Over Time" className={className}>
      <p className="text-muted-foreground text-sm">Spending over time chart will appear here.</p>
    </ChartCard>
  );
}

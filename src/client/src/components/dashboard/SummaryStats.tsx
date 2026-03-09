import { ChartCard } from "@/components/charts";
import type { DateRange } from "@/hooks/useDashboard";

interface SummaryStatsProps {
  dateRange: DateRange;
  className?: string;
}

export function SummaryStats({ dateRange: _dateRange, className }: SummaryStatsProps) {
  return (
    <ChartCard title="Summary" className={className}>
      <p className="text-muted-foreground text-sm">Summary statistics will appear here.</p>
    </ChartCard>
  );
}

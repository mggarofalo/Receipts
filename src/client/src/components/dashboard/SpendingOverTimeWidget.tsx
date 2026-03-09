import { useState, useCallback, useMemo } from "react";
import { ChartCard, AreaTimeChart } from "@/components/charts";
import { useDashboardSpendingOverTime } from "@/hooks/useDashboard";
import type { DateRange } from "@/hooks/useDashboard";
import { Button } from "@/components/ui/button";

type Granularity = "daily" | "weekly" | "monthly";

interface SpendingOverTimeWidgetProps {
  dateRange: DateRange;
  className?: string;
}

const granularityOptions: { value: Granularity; label: string }[] = [
  { value: "daily", label: "Day" },
  { value: "weekly", label: "Week" },
  { value: "monthly", label: "Month" },
];

function formatCurrency(value: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
}

export function SpendingOverTimeWidget({
  dateRange,
  className,
}: SpendingOverTimeWidgetProps) {
  const [granularity, setGranularity] = useState<Granularity>("monthly");
  const { data, isLoading } = useDashboardSpendingOverTime(
    dateRange,
    granularity,
  );

  const handleGranularity = useCallback((g: Granularity) => {
    setGranularity(g);
  }, []);

  const chartData = useMemo(
    () =>
      (data?.buckets ?? []).map((b) => ({
        period: b.period,
        amount: Number(b.amount ?? 0),
      })),
    [data?.buckets],
  );

  return (
    <ChartCard
      title="Spending Over Time"
      loading={isLoading}
      empty={chartData.length === 0 && !isLoading}
      className={className}
      action={
        <div className="flex gap-1">
          {granularityOptions.map(({ value, label }) => (
            <Button
              key={value}
              variant={granularity === value ? "default" : "outline"}
              size="sm"
              onClick={() => handleGranularity(value)}
            >
              {label}
            </Button>
          ))}
        </div>
      }
    >
      <AreaTimeChart data={chartData} formatValue={formatCurrency} />
    </ChartCard>
  );
}

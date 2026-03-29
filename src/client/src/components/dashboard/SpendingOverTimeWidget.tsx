import { useState, useCallback, useMemo } from "react";
import { ChartCard, AreaTimeChart } from "@/components/charts";
import { useDashboardSpendingOverTime } from "@/hooks/useDashboard";
import type { DateRange } from "@/hooks/useDashboard";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { computeRollingAverage } from "@/lib/rolling-average";

type Granularity = "monthly" | "quarterly" | "ytd" | "yearly";
type WindowSize = "3" | "6" | "12";

interface SpendingOverTimeWidgetProps {
  dateRange: DateRange;
  className?: string;
}

const granularityOptions: { value: Granularity; label: string }[] = [
  { value: "monthly", label: "Month" },
  { value: "quarterly", label: "Quarter" },
  { value: "ytd", label: "YTD" },
  { value: "yearly", label: "Year" },
];

const windowSizeOptions: { value: WindowSize; label: string }[] = [
  { value: "3", label: "3-period" },
  { value: "6", label: "6-period" },
  { value: "12", label: "12-period" },
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
  const [showTrendline, setShowTrendline] = useState(false);
  const [windowSize, setWindowSize] = useState<WindowSize>("3");
  const { data, isLoading } = useDashboardSpendingOverTime(
    dateRange,
    granularity,
  );

  const handleGranularity = useCallback((g: Granularity) => {
    setGranularity(g);
  }, []);

  const handleToggleTrendline = useCallback(() => {
    setShowTrendline((prev) => !prev);
  }, []);

  const handleWindowSizeChange = useCallback((value: string) => {
    setWindowSize(value as WindowSize);
  }, []);

  const chartData = useMemo(
    () =>
      (data?.buckets ?? []).map((b) => ({
        period: b.period,
        amount: Number(b.amount ?? 0),
      })),
    [data?.buckets],
  );

  const trendlineData = useMemo(
    () =>
      showTrendline
        ? computeRollingAverage(chartData, Number(windowSize))
        : undefined,
    [showTrendline, chartData, windowSize],
  );

  return (
    <ChartCard
      title="Spending Over Time"
      loading={isLoading}
      empty={chartData.length === 0 && !isLoading}
      className={className}
      action={
        <div className="flex items-center gap-2">
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
          <div className="flex items-center gap-1.5">
            <Button
              variant={showTrendline ? "default" : "outline"}
              size="sm"
              onClick={handleToggleTrendline}
              aria-pressed={showTrendline}
            >
              Trendline
            </Button>
            {showTrendline && (
              <Select value={windowSize} onValueChange={handleWindowSizeChange}>
                <SelectTrigger size="sm" aria-label="Rolling average window size">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {windowSizeOptions.map(({ value, label }) => (
                    <SelectItem key={value} value={value}>
                      {label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
          </div>
        </div>
      }
    >
      <AreaTimeChart
        data={chartData}
        trendlineData={trendlineData}
        formatValue={formatCurrency}
      />
    </ChartCard>
  );
}

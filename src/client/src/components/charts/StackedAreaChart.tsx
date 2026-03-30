import { useId, useMemo } from "react";
import {
  ResponsiveContainer,
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
} from "recharts";
import { CHART_COLORS } from "./chart-colors";

interface StackedAreaChartProps {
  categories: string[];
  buckets: Array<{ period: string; amounts: number[] }>;
  height?: number;
  formatValue?: (value: number) => string;
}

export function StackedAreaChart({
  categories,
  buckets,
  height = 350,
  formatValue = (v) => v.toLocaleString(),
}: StackedAreaChartProps) {
  const baseId = useId();

  const chartData = useMemo(
    () =>
      buckets.map((bucket) => {
        const point: Record<string, string | number> = {
          period: bucket.period,
        };
        categories.forEach((cat, i) => {
          point[cat] = bucket.amounts[i] ?? 0;
        });
        return point;
      }),
    [buckets, categories],
  );

  return (
    <ResponsiveContainer width="100%" height={height}>
      <AreaChart
        data={chartData}
        margin={{ top: 5, right: 20, bottom: 5, left: 0 }}
      >
        <defs>
          {categories.map((cat, i) => (
            <linearGradient
              key={cat}
              id={`${baseId}-gradient-${i}`}
              x1="0"
              y1="0"
              x2="0"
              y2="1"
            >
              <stop
                offset="5%"
                stopColor={CHART_COLORS[i % CHART_COLORS.length]}
                stopOpacity={0.4}
              />
              <stop
                offset="95%"
                stopColor={CHART_COLORS[i % CHART_COLORS.length]}
                stopOpacity={0.05}
              />
            </linearGradient>
          ))}
        </defs>
        <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
        <XAxis
          dataKey="period"
          type="category"
          tick={{ fontSize: 12 }}
          className="fill-muted-foreground"
        />
        <YAxis
          tick={{ fontSize: 12 }}
          className="fill-muted-foreground"
          tickFormatter={formatValue}
        />
        <Tooltip
          formatter={(value, name) => [
            formatValue(Number(value)),
            String(name),
          ]}
          contentStyle={{
            backgroundColor: "hsl(var(--popover))",
            border: "1px solid hsl(var(--border))",
            borderRadius: "var(--radius)",
            color: "hsl(var(--popover-foreground))",
          }}
        />
        <Legend
          verticalAlign="bottom"
          iconType="circle"
          iconSize={8}
          wrapperStyle={{ fontSize: "12px" }}
        />
        {categories.map((cat, i) => (
          <Area
            key={cat}
            type="monotone"
            dataKey={cat}
            stackId="1"
            stroke={CHART_COLORS[i % CHART_COLORS.length]}
            fill={`url(#${baseId}-gradient-${i})`}
            fillOpacity={0.3}
            strokeWidth={2}
            strokeOpacity={0.8}
          />
        ))}
      </AreaChart>
    </ResponsiveContainer>
  );
}

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
  /** ARIA label used as the chart's accessible name when no labelledby id is available. */
  "aria-label"?: string;
  /** ID of an element that labels this chart (e.g. ChartCard's title id). */
  "aria-labelledby"?: string;
}

export function StackedAreaChart({
  categories,
  buckets,
  height = 350,
  formatValue = (v) => v.toLocaleString(),
  "aria-label": ariaLabel,
  "aria-labelledby": ariaLabelledBy,
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
    <div>
      {/* Visually-hidden data table — provides a text alternative (WCAG 1.1.1)
          and resolves the color-only series identification concern (WCAG 1.4.1) */}
      <table className="sr-only">
        <caption>{ariaLabel ?? "Stacked area chart data"}</caption>
        <thead>
          <tr>
            <th scope="col">Period</th>
            {categories.map((cat) => (
              <th key={cat} scope="col">
                {cat}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {buckets.map((bucket) => (
            <tr key={bucket.period}>
              <td>{bucket.period}</td>
              {categories.map((cat, i) => (
                <td key={cat}>{formatValue(bucket.amounts[i] ?? 0)}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>

      <div
        role="img"
        aria-label={ariaLabelledBy ? undefined : (ariaLabel ?? "Stacked area chart")}
        aria-labelledby={ariaLabelledBy}
      >
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
      </div>
    </div>
  );
}

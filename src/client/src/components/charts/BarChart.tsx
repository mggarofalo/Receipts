import {
  ResponsiveContainer,
  BarChart as RechartsBarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
} from "recharts";
import { CHART_COLORS } from "./chart-colors";

interface BarChartProps {
  data: Array<{ name: string; value: number }>;
  height?: number;
  color?: string;
  layout?: "horizontal" | "vertical";
  formatValue?: (value: number) => string;
}

export function BarChart({
  data,
  height = 300,
  color = CHART_COLORS[0],
  layout = "vertical",
  formatValue = (v) => v.toLocaleString(),
}: BarChartProps) {
  const isHorizontal = layout === "horizontal";

  return (
    <ResponsiveContainer width="100%" height={height}>
      <RechartsBarChart
        data={data}
        layout={isHorizontal ? "vertical" : "horizontal"}
        margin={{ top: 5, right: 20, bottom: 5, left: isHorizontal ? 80 : 0 }}
      >
        <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
        {isHorizontal ? (
          <>
            <XAxis
              type="number"
              tick={{ fontSize: 12 }}
              className="fill-muted-foreground"
              tickFormatter={formatValue}
            />
            <YAxis
              type="category"
              dataKey="name"
              tick={{ fontSize: 12 }}
              className="fill-muted-foreground"
              width={75}
            />
          </>
        ) : (
          <>
            <XAxis
              dataKey="name"
              tick={{ fontSize: 12 }}
              className="fill-muted-foreground"
            />
            <YAxis
              tick={{ fontSize: 12 }}
              className="fill-muted-foreground"
              tickFormatter={formatValue}
            />
          </>
        )}
        <Tooltip
          formatter={(value: number) => [formatValue(value), "Amount"]}
          contentStyle={{
            backgroundColor: "hsl(var(--popover))",
            border: "1px solid hsl(var(--border))",
            borderRadius: "var(--radius)",
            color: "hsl(var(--popover-foreground))",
          }}
        />
        <Bar dataKey="value" fill={color} radius={[4, 4, 0, 0]} />
      </RechartsBarChart>
    </ResponsiveContainer>
  );
}

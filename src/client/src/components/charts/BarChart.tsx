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
  /** ARIA label used as the chart's accessible name when no labelledby id is available. */
  "aria-label"?: string;
  /** ID of an element that labels this chart (e.g. ChartCard's title id). */
  "aria-labelledby"?: string;
}

export function BarChart({
  data,
  height = 300,
  color = CHART_COLORS[0],
  layout = "vertical",
  formatValue = (v) => v.toLocaleString(),
  "aria-label": ariaLabel,
  "aria-labelledby": ariaLabelledBy,
}: BarChartProps) {
  const isHorizontal = layout === "horizontal";

  return (
    <div>
      {/* Visually-hidden data table — provides a text alternative (WCAG 1.1.1)
          and resolves the color-only concern (WCAG 1.4.1) */}
      <table className="sr-only">
        <caption>{ariaLabel ?? "Bar chart data"}</caption>
        <thead>
          <tr>
            <th scope="col">Name</th>
            <th scope="col">Value</th>
          </tr>
        </thead>
        <tbody>
          {data.map((row) => (
            <tr key={row.name}>
              <td>{row.name}</td>
              <td>{formatValue(row.value)}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <div
        role="img"
        aria-label={ariaLabelledBy ? undefined : (ariaLabel ?? "Bar chart")}
        aria-labelledby={ariaLabelledBy}
        aria-hidden={false}
      >
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
              formatter={(value) => [formatValue(Number(value)), "Amount"]}
              contentStyle={{
                backgroundColor: "hsl(var(--popover))",
                border: "1px solid hsl(var(--border))",
                borderRadius: "var(--radius)",
                color: "hsl(var(--popover-foreground))",
              }}
            />
            <Bar dataKey="value" fill={color} radius={isHorizontal ? [0, 4, 4, 0] : [4, 4, 0, 0]} />
          </RechartsBarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}

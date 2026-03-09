import {
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Tooltip,
  Legend,
} from "recharts";
import { CHART_COLORS } from "./chart-colors";

interface DonutChartItem {
  name: string;
  value: number;
}

interface DonutChartProps {
  data: DonutChartItem[];
  height?: number;
  centerLabel?: string;
  formatValue?: (value: number) => string;
}

export function DonutChart({
  data,
  height = 300,
  centerLabel,
  formatValue = (v) => v.toLocaleString(),
}: DonutChartProps) {
  return (
    <ResponsiveContainer width="100%" height={height}>
      <PieChart>
        <Pie
          data={data}
          cx="50%"
          cy="50%"
          innerRadius="55%"
          outerRadius="80%"
          paddingAngle={2}
          dataKey="value"
          nameKey="name"
          label={false}
        >
          {data.map((_entry, index) => (
            <Cell
              key={`cell-${index}`}
              fill={CHART_COLORS[index % CHART_COLORS.length]}
            />
          ))}
        </Pie>
        {centerLabel && (
          <text
            x="50%"
            y="50%"
            textAnchor="middle"
            dominantBaseline="middle"
            className="fill-foreground text-lg font-semibold"
          >
            {centerLabel}
          </text>
        )}
        <Tooltip
          formatter={(value) => [formatValue(Number(value))]}
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
      </PieChart>
    </ResponsiveContainer>
  );
}

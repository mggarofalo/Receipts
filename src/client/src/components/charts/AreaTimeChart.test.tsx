import { render, screen } from "@testing-library/react";
import { AreaTimeChart } from "./AreaTimeChart";

vi.mock("recharts", () => ({
  ResponsiveContainer: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="responsive-container">{children}</div>
  ),
  AreaChart: ({
    children,
    data,
  }: {
    children: React.ReactNode;
    data: unknown[];
  }) => (
    <div data-testid="area-chart" data-count={data.length}>
      {children}
    </div>
  ),
  Area: ({ dataKey, strokeDasharray }: { dataKey: string; strokeDasharray?: string }) => (
    <div
      data-testid={dataKey === "trendline" ? "trendline-area" : "area"}
      data-dasharray={strokeDasharray ?? ""}
    />
  ),
  XAxis: () => <div data-testid="x-axis" />,
  YAxis: () => <div data-testid="y-axis" />,
  CartesianGrid: () => <div data-testid="cartesian-grid" />,
  Tooltip: () => <div data-testid="tooltip" />,
}));

const mockData = [
  { period: "Jan", amount: 100 },
  { period: "Feb", amount: 200 },
  { period: "Mar", amount: 150 },
];

const mockTrendline = [
  { period: "Jan", amount: 100 },
  { period: "Feb", amount: 150 },
  { period: "Mar", amount: 150 },
];

describe("AreaTimeChart", () => {
  it("renders the chart with data", () => {
    render(<AreaTimeChart data={mockData} />);
    expect(screen.getByTestId("responsive-container")).toBeInTheDocument();
    expect(screen.getByTestId("area-chart")).toHaveAttribute("data-count", "3");
    expect(screen.getByTestId("area")).toBeInTheDocument();
  });

  it("renders axis and grid elements", () => {
    render(<AreaTimeChart data={mockData} />);
    expect(screen.getByTestId("x-axis")).toBeInTheDocument();
    expect(screen.getByTestId("y-axis")).toBeInTheDocument();
    expect(screen.getByTestId("cartesian-grid")).toBeInTheDocument();
  });

  it("renders with empty data", () => {
    render(<AreaTimeChart data={[]} />);
    expect(screen.getByTestId("area-chart")).toHaveAttribute("data-count", "0");
  });

  it("does not render trendline when trendlineData is not provided", () => {
    render(<AreaTimeChart data={mockData} />);
    expect(screen.queryByTestId("trendline-area")).not.toBeInTheDocument();
  });

  it("renders trendline area when trendlineData is provided", () => {
    render(<AreaTimeChart data={mockData} trendlineData={mockTrendline} />);
    expect(screen.getByTestId("trendline-area")).toBeInTheDocument();
  });

  it("renders trendline with dashed stroke", () => {
    render(<AreaTimeChart data={mockData} trendlineData={mockTrendline} />);
    expect(screen.getByTestId("trendline-area")).toHaveAttribute(
      "data-dasharray",
      "5 5",
    );
  });

  it("does not render trendline when trendlineData is empty", () => {
    render(<AreaTimeChart data={mockData} trendlineData={[]} />);
    expect(screen.queryByTestId("trendline-area")).not.toBeInTheDocument();
  });
});

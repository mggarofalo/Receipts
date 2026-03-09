import { render, screen } from "@testing-library/react";
import { BarChart } from "./BarChart";

vi.mock("recharts", () => ({
  ResponsiveContainer: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="responsive-container">{children}</div>
  ),
  BarChart: ({
    children,
    data,
    layout,
  }: {
    children: React.ReactNode;
    data: unknown[];
    layout?: string;
  }) => (
    <div data-testid="bar-chart" data-count={data.length} data-layout={layout}>
      {children}
    </div>
  ),
  Bar: () => <div data-testid="bar" />,
  XAxis: ({ type }: { type?: string }) => (
    <div data-testid="x-axis" data-type={type} />
  ),
  YAxis: ({ type }: { type?: string }) => (
    <div data-testid="y-axis" data-type={type} />
  ),
  CartesianGrid: () => <div data-testid="cartesian-grid" />,
  Tooltip: () => <div data-testid="tooltip" />,
}));

const mockData = [
  { name: "Account A", value: 500 },
  { name: "Account B", value: 300 },
];

describe("BarChart", () => {
  it("renders the chart with data", () => {
    render(<BarChart data={mockData} />);
    expect(screen.getByTestId("responsive-container")).toBeInTheDocument();
    expect(screen.getByTestId("bar-chart")).toHaveAttribute("data-count", "2");
    expect(screen.getByTestId("bar")).toBeInTheDocument();
  });

  it("passes horizontal recharts layout for default vertical bar orientation", () => {
    render(<BarChart data={mockData} />);
    expect(screen.getByTestId("bar-chart")).toHaveAttribute(
      "data-layout",
      "horizontal",
    );
  });

  it("passes vertical recharts layout for horizontal bar orientation", () => {
    render(<BarChart data={mockData} layout="horizontal" />);
    expect(screen.getByTestId("bar-chart")).toHaveAttribute(
      "data-layout",
      "vertical",
    );
    expect(screen.getByTestId("x-axis")).toHaveAttribute("data-type", "number");
    expect(screen.getByTestId("y-axis")).toHaveAttribute(
      "data-type",
      "category",
    );
  });

  it("renders with empty data", () => {
    render(<BarChart data={[]} />);
    expect(screen.getByTestId("bar-chart")).toHaveAttribute("data-count", "0");
  });
});

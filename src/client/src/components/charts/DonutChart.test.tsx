import { render, screen } from "@testing-library/react";
import { DonutChart } from "./DonutChart";

vi.mock("recharts", () => ({
  ResponsiveContainer: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="responsive-container">{children}</div>
  ),
  PieChart: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="pie-chart">{children}</div>
  ),
  Pie: ({
    children,
    data,
  }: {
    children: React.ReactNode;
    data: unknown[];
  }) => (
    <div data-testid="pie" data-count={data.length}>
      {children}
    </div>
  ),
  Cell: ({ fill }: { fill: string }) => (
    <div data-testid="cell" data-fill={fill} />
  ),
  Tooltip: () => <div data-testid="tooltip" />,
  Legend: () => <div data-testid="legend" />,
}));

const mockData = [
  { name: "Food", value: 300 },
  { name: "Transport", value: 200 },
  { name: "Entertainment", value: 100 },
];

describe("DonutChart", () => {
  it("renders the chart with data", () => {
    render(<DonutChart data={mockData} />);
    expect(screen.getByTestId("responsive-container")).toBeInTheDocument();
    expect(screen.getByTestId("pie-chart")).toBeInTheDocument();
    expect(screen.getByTestId("pie")).toHaveAttribute("data-count", "3");
  });

  it("renders cells for each data item", () => {
    render(<DonutChart data={mockData} />);
    expect(screen.getAllByTestId("cell")).toHaveLength(3);
  });

  it("renders legend", () => {
    render(<DonutChart data={mockData} />);
    expect(screen.getByTestId("legend")).toBeInTheDocument();
  });

  it("renders with empty data", () => {
    render(<DonutChart data={[]} />);
    expect(screen.getByTestId("pie")).toHaveAttribute("data-count", "0");
  });
});

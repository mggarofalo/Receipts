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

  // Accessibility tests
  it("renders a visually-hidden data table with all data rows", () => {
    render(<BarChart data={mockData} />);
    const table = document.querySelector("table.sr-only");
    expect(table).toBeInTheDocument();
    expect(screen.getByText("Account A")).toBeInTheDocument();
    expect(screen.getByText("Account B")).toBeInTheDocument();
  });

  it("renders chart container with role=img", () => {
    render(<BarChart data={mockData} aria-label="Spending by account" />);
    expect(screen.getByRole("img", { name: "Spending by account" })).toBeInTheDocument();
  });

  it("uses aria-labelledby when provided", () => {
    render(
      <div>
        <h2 id="chart-title">Accounts</h2>
        <BarChart data={mockData} aria-labelledby="chart-title" />
      </div>,
    );
    const img = document.querySelector("[role='img']");
    expect(img).toHaveAttribute("aria-labelledby", "chart-title");
    expect(img).not.toHaveAttribute("aria-label");
  });

  it("falls back to a default aria-label when neither aria-label nor aria-labelledby is provided", () => {
    render(<BarChart data={mockData} />);
    expect(screen.getByRole("img", { name: "Bar chart" })).toBeInTheDocument();
  });

  it("renders formatted values in the data table", () => {
    render(
      <BarChart
        data={mockData}
        formatValue={(v) => `$${v.toFixed(2)}`}
      />,
    );
    expect(screen.getByText("$500.00")).toBeInTheDocument();
    expect(screen.getByText("$300.00")).toBeInTheDocument();
  });
});

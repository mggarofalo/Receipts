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

  // Accessibility tests
  it("renders a visually-hidden data table with all category rows", () => {
    render(<DonutChart data={mockData} />);
    const table = document.querySelector("table.sr-only");
    expect(table).toBeInTheDocument();
    expect(screen.getByText("Food")).toBeInTheDocument();
    expect(screen.getByText("Transport")).toBeInTheDocument();
    expect(screen.getByText("Entertainment")).toBeInTheDocument();
  });

  it("renders chart container with role=img and accessible name", () => {
    render(<DonutChart data={mockData} aria-label="Spending by category" />);
    expect(screen.getByRole("img", { name: "Spending by category" })).toBeInTheDocument();
  });

  it("uses aria-labelledby when provided", () => {
    render(
      <div>
        <h2 id="donut-title">Categories</h2>
        <DonutChart data={mockData} aria-labelledby="donut-title" />
      </div>,
    );
    const img = document.querySelector("[role='img']");
    expect(img).toHaveAttribute("aria-labelledby", "donut-title");
    expect(img).not.toHaveAttribute("aria-label");
  });

  it("falls back to a default aria-label when neither aria-label nor aria-labelledby is provided", () => {
    render(<DonutChart data={mockData} />);
    expect(screen.getByRole("img", { name: "Donut chart" })).toBeInTheDocument();
  });

  it("renders formatted values in the data table", () => {
    render(
      <DonutChart
        data={mockData}
        formatValue={(v) => `$${v.toFixed(2)}`}
      />,
    );
    expect(screen.getByText("$300.00")).toBeInTheDocument();
    expect(screen.getByText("$200.00")).toBeInTheDocument();
    expect(screen.getByText("$100.00")).toBeInTheDocument();
  });

  it("data table identifies each category by name, not color alone", () => {
    render(<DonutChart data={mockData} />);
    const table = document.querySelector("table.sr-only");
    const headers = table?.querySelectorAll("th");
    expect(headers?.[0]?.textContent).toBe("Category");
    expect(headers?.[1]?.textContent).toBe("Value");
  });
});

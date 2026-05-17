import { render, screen } from "@testing-library/react";
import { StackedAreaChart } from "./StackedAreaChart";

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
  Area: ({ dataKey }: { dataKey: string }) => (
    <div data-testid="area" data-key={dataKey} />
  ),
  XAxis: () => <div data-testid="x-axis" />,
  YAxis: () => <div data-testid="y-axis" />,
  CartesianGrid: () => <div data-testid="cartesian-grid" />,
  Tooltip: () => <div data-testid="tooltip" />,
  Legend: () => <div data-testid="legend" />,
}));

const mockCategories = ["Food", "Transport", "Entertainment"];
const mockBuckets = [
  { period: "Jan", amounts: [100, 50, 30] },
  { period: "Feb", amounts: [120, 60, 40] },
  { period: "Mar", amounts: [90, 45, 25] },
];

describe("StackedAreaChart", () => {
  it("renders the chart with data", () => {
    render(<StackedAreaChart categories={mockCategories} buckets={mockBuckets} />);
    expect(screen.getByTestId("responsive-container")).toBeInTheDocument();
    expect(screen.getByTestId("area-chart")).toHaveAttribute("data-count", "3");
  });

  it("renders one Area element per category", () => {
    render(<StackedAreaChart categories={mockCategories} buckets={mockBuckets} />);
    expect(screen.getAllByTestId("area")).toHaveLength(3);
  });

  it("renders axis, grid, legend, and tooltip elements", () => {
    render(<StackedAreaChart categories={mockCategories} buckets={mockBuckets} />);
    expect(screen.getByTestId("x-axis")).toBeInTheDocument();
    expect(screen.getByTestId("y-axis")).toBeInTheDocument();
    expect(screen.getByTestId("cartesian-grid")).toBeInTheDocument();
    expect(screen.getByTestId("legend")).toBeInTheDocument();
    expect(screen.getByTestId("tooltip")).toBeInTheDocument();
  });

  it("renders with empty categories and buckets", () => {
    render(<StackedAreaChart categories={[]} buckets={[]} />);
    expect(screen.getByTestId("area-chart")).toHaveAttribute("data-count", "0");
    expect(screen.queryAllByTestId("area")).toHaveLength(0);
  });

  // Accessibility tests
  it("renders a visually-hidden data table with period and category columns", () => {
    render(<StackedAreaChart categories={mockCategories} buckets={mockBuckets} />);
    const table = document.querySelector("table.sr-only");
    expect(table).toBeInTheDocument();
    expect(screen.getByText("Jan")).toBeInTheDocument();
    expect(screen.getByText("Feb")).toBeInTheDocument();
    expect(screen.getByText("Mar")).toBeInTheDocument();
  });

  it("renders category names as column headers in the data table", () => {
    render(<StackedAreaChart categories={mockCategories} buckets={mockBuckets} />);
    const table = document.querySelector("table.sr-only");
    const headers = table?.querySelectorAll("th");
    const headerTexts = Array.from(headers ?? []).map((h) => h.textContent);
    expect(headerTexts).toContain("Food");
    expect(headerTexts).toContain("Transport");
    expect(headerTexts).toContain("Entertainment");
  });

  it("renders chart container with role=img and accessible name", () => {
    render(
      <StackedAreaChart
        categories={mockCategories}
        buckets={mockBuckets}
        aria-label="Category spending trends"
      />,
    );
    expect(screen.getByRole("img", { name: "Category spending trends" })).toBeInTheDocument();
  });

  it("uses aria-labelledby when provided", () => {
    render(
      <div>
        <h2 id="stacked-title">Spending by Category</h2>
        <StackedAreaChart
          categories={mockCategories}
          buckets={mockBuckets}
          aria-labelledby="stacked-title"
        />
      </div>,
    );
    const img = document.querySelector("[role='img']");
    expect(img).toHaveAttribute("aria-labelledby", "stacked-title");
    expect(img).not.toHaveAttribute("aria-label");
  });

  it("falls back to a default aria-label when neither aria-label nor aria-labelledby is provided", () => {
    render(<StackedAreaChart categories={mockCategories} buckets={mockBuckets} />);
    expect(screen.getByRole("img", { name: "Stacked area chart" })).toBeInTheDocument();
  });

  it("renders formatted values in the data table for each category", () => {
    render(
      <StackedAreaChart
        categories={mockCategories}
        buckets={mockBuckets}
        formatValue={(v) => `$${v}`}
      />,
    );
    expect(screen.getByText("$100")).toBeInTheDocument();
    expect(screen.getByText("$50")).toBeInTheDocument();
    expect(screen.getByText("$30")).toBeInTheDocument();
  });

  it("data table identifies each series by name, not color alone", () => {
    render(<StackedAreaChart categories={mockCategories} buckets={mockBuckets} />);
    const table = document.querySelector("table.sr-only");
    const headers = table?.querySelectorAll("th");
    // First column is Period, rest are category names
    expect(headers?.[0]?.textContent).toBe("Period");
    expect(headers?.[1]?.textContent).toBe("Food");
  });
});

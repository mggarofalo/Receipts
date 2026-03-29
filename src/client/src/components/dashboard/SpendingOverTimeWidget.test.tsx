import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderWithQueryClient } from "@/test/test-utils";
import { SpendingOverTimeWidget } from "./SpendingOverTimeWidget";

vi.mock("recharts", () => ({
  ResponsiveContainer: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="responsive-container">{children}</div>
  ),
  AreaChart: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="area-chart">{children}</div>
  ),
  Area: ({
    dataKey,
    strokeDasharray,
  }: {
    dataKey: string;
    strokeDasharray?: string;
  }) => (
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

vi.mock("@/hooks/useDashboard", () => ({
  useDashboardSpendingOverTime: vi.fn(),
}));

import { useDashboardSpendingOverTime } from "@/hooks/useDashboard";
const mockHook = vi.mocked(useDashboardSpendingOverTime);

const dateRange = { startDate: "2024-01-01", endDate: "2024-01-31" };

const bucketData = [
  { period: "2024-01", amount: 100 },
  { period: "2024-02", amount: 200 },
  { period: "2024-03", amount: 300 },
  { period: "2024-04", amount: 400 },
];

describe("SpendingOverTimeWidget", () => {
  it("renders granularity toggle buttons", () => {
    mockHook.mockReturnValue({
      data: { buckets: [] },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingOverTime>);

    renderWithQueryClient(<SpendingOverTimeWidget dateRange={dateRange} />);
    expect(screen.getByRole("button", { name: "Month" })).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Quarter" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "YTD" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Year" })).toBeInTheDocument();
  });

  it("renders chart with data", () => {
    mockHook.mockReturnValue({
      data: { buckets: bucketData },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingOverTime>);

    renderWithQueryClient(<SpendingOverTimeWidget dateRange={dateRange} />);
    expect(screen.getByTestId("area-chart")).toBeInTheDocument();
  });

  it("shows loading state", () => {
    mockHook.mockReturnValue({
      data: undefined,
      isLoading: true,
    } as unknown as ReturnType<typeof useDashboardSpendingOverTime>);

    renderWithQueryClient(<SpendingOverTimeWidget dateRange={dateRange} />);
    expect(screen.getByLabelText("Loading")).toBeInTheDocument();
  });

  it("changes granularity on button click", async () => {
    const user = userEvent.setup();
    mockHook.mockReturnValue({
      data: { buckets: [] },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingOverTime>);

    renderWithQueryClient(<SpendingOverTimeWidget dateRange={dateRange} />);
    await user.click(screen.getByRole("button", { name: "Quarter" }));
    expect(mockHook).toHaveBeenCalledWith(dateRange, "quarterly");
  });

  it("renders trendline toggle button", () => {
    mockHook.mockReturnValue({
      data: { buckets: [] },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingOverTime>);

    renderWithQueryClient(<SpendingOverTimeWidget dateRange={dateRange} />);
    expect(
      screen.getByRole("button", { name: "Trendline" }),
    ).toBeInTheDocument();
  });

  it("does not show trendline by default", () => {
    mockHook.mockReturnValue({
      data: { buckets: bucketData },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingOverTime>);

    renderWithQueryClient(<SpendingOverTimeWidget dateRange={dateRange} />);
    expect(screen.queryByTestId("trendline-area")).not.toBeInTheDocument();
  });

  it("shows trendline after clicking toggle", async () => {
    const user = userEvent.setup();
    mockHook.mockReturnValue({
      data: { buckets: bucketData },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingOverTime>);

    renderWithQueryClient(<SpendingOverTimeWidget dateRange={dateRange} />);
    await user.click(screen.getByRole("button", { name: "Trendline" }));
    expect(screen.getByTestId("trendline-area")).toBeInTheDocument();
  });

  it("hides trendline after toggling off", async () => {
    const user = userEvent.setup();
    mockHook.mockReturnValue({
      data: { buckets: bucketData },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingOverTime>);

    renderWithQueryClient(<SpendingOverTimeWidget dateRange={dateRange} />);
    const trendlineBtn = screen.getByRole("button", { name: "Trendline" });
    await user.click(trendlineBtn);
    expect(screen.getByTestId("trendline-area")).toBeInTheDocument();
    await user.click(trendlineBtn);
    expect(screen.queryByTestId("trendline-area")).not.toBeInTheDocument();
  });

  it("shows window size selector only when trendline is enabled", async () => {
    const user = userEvent.setup();
    mockHook.mockReturnValue({
      data: { buckets: bucketData },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingOverTime>);

    renderWithQueryClient(<SpendingOverTimeWidget dateRange={dateRange} />);
    expect(
      screen.queryByLabelText("Rolling average window size"),
    ).not.toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Trendline" }));
    expect(
      screen.getByLabelText("Rolling average window size"),
    ).toBeInTheDocument();
  });

  it("trendline button has aria-pressed attribute", async () => {
    const user = userEvent.setup();
    mockHook.mockReturnValue({
      data: { buckets: bucketData },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingOverTime>);

    renderWithQueryClient(<SpendingOverTimeWidget dateRange={dateRange} />);
    const btn = screen.getByRole("button", { name: "Trendline" });
    expect(btn).toHaveAttribute("aria-pressed", "false");
    await user.click(btn);
    expect(btn).toHaveAttribute("aria-pressed", "true");
  });
});

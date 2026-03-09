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
  Area: () => <div data-testid="area" />,
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

describe("SpendingOverTimeWidget", () => {
  it("renders granularity toggle buttons", () => {
    mockHook.mockReturnValue({
      data: { buckets: [] },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingOverTime>);

    renderWithQueryClient(<SpendingOverTimeWidget dateRange={dateRange} />);
    expect(screen.getByRole("button", { name: "Day" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Week" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Month" })).toBeInTheDocument();
  });

  it("renders chart with data", () => {
    mockHook.mockReturnValue({
      data: {
        buckets: [
          { period: "2024-01", amount: 100 },
          { period: "2024-02", amount: 200 },
        ],
      },
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
    await user.click(screen.getByRole("button", { name: "Day" }));
    expect(mockHook).toHaveBeenCalledWith(dateRange, "daily");
  });
});

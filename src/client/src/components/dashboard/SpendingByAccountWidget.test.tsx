import { screen } from "@testing-library/react";
import { renderWithQueryClient } from "@/test/test-utils";
import { SpendingByAccountWidget } from "./SpendingByAccountWidget";

vi.mock("recharts", () => ({
  ResponsiveContainer: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="responsive-container">{children}</div>
  ),
  BarChart: ({
    children,
    data,
  }: {
    children: React.ReactNode;
    data: unknown[];
  }) => (
    <div data-testid="bar-chart" data-count={data.length}>
      {children}
    </div>
  ),
  Bar: () => <div data-testid="bar" />,
  XAxis: () => <div data-testid="x-axis" />,
  YAxis: () => <div data-testid="y-axis" />,
  CartesianGrid: () => <div data-testid="cartesian-grid" />,
  Tooltip: () => <div data-testid="tooltip" />,
}));

vi.mock("@/hooks/useDashboard", () => ({
  useDashboardSpendingByAccount: vi.fn(),
}));

import { useDashboardSpendingByAccount } from "@/hooks/useDashboard";
const mockHook = vi.mocked(useDashboardSpendingByAccount);

const dateRange = { startDate: "2024-01-01", endDate: "2024-01-31" };

describe("SpendingByAccountWidget", () => {
  it("renders chart with account data sorted descending", () => {
    mockHook.mockReturnValue({
      data: {
        items: [
          { accountId: "1", accountName: "Visa", amount: 300, percentage: 60 },
          {
            accountId: "2",
            accountName: "Amex",
            amount: 500,
            percentage: 40,
          },
        ],
      },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingByAccount>);

    renderWithQueryClient(
      <SpendingByAccountWidget dateRange={dateRange} />,
    );
    expect(screen.getByTestId("bar-chart")).toBeInTheDocument();
    expect(screen.getByTestId("bar-chart")).toHaveAttribute("data-count", "2");
  });

  it("shows loading state", () => {
    mockHook.mockReturnValue({
      data: undefined,
      isLoading: true,
    } as unknown as ReturnType<typeof useDashboardSpendingByAccount>);

    renderWithQueryClient(
      <SpendingByAccountWidget dateRange={dateRange} />,
    );
    expect(screen.getByLabelText("Loading")).toBeInTheDocument();
  });

  it("shows empty state when no data", () => {
    mockHook.mockReturnValue({
      data: { items: [] },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingByAccount>);

    renderWithQueryClient(
      <SpendingByAccountWidget dateRange={dateRange} />,
    );
    expect(screen.getByText("No data available")).toBeInTheDocument();
  });
});

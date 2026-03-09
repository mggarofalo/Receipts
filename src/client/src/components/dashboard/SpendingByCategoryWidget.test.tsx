import { screen } from "@testing-library/react";
import { renderWithQueryClient } from "@/test/test-utils";
import { SpendingByCategoryWidget } from "./SpendingByCategoryWidget";

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
  Cell: () => <div data-testid="cell" />,
  Tooltip: () => <div data-testid="tooltip" />,
  Legend: () => <div data-testid="legend" />,
}));

vi.mock("@/hooks/useDashboard", () => ({
  useDashboardSpendingByCategory: vi.fn(),
}));

import { useDashboardSpendingByCategory } from "@/hooks/useDashboard";
const mockHook = vi.mocked(useDashboardSpendingByCategory);

const dateRange = { startDate: "2024-01-01", endDate: "2024-01-31" };

describe("SpendingByCategoryWidget", () => {
  it("renders chart with category data", () => {
    mockHook.mockReturnValue({
      data: {
        items: [
          { categoryName: "Food", amount: 300, percentage: 60 },
          { categoryName: "Transport", amount: 200, percentage: 40 },
        ],
      },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingByCategory>);

    renderWithQueryClient(
      <SpendingByCategoryWidget dateRange={dateRange} />,
    );
    expect(screen.getByTestId("pie-chart")).toBeInTheDocument();
    expect(screen.getByTestId("pie")).toHaveAttribute("data-count", "2");
  });

  it("groups excess categories into Other", () => {
    const items = Array.from({ length: 7 }, (_, i) => ({
      categoryName: `Cat${i}`,
      amount: 100 - i * 10,
      percentage: 14,
    }));
    mockHook.mockReturnValue({
      data: { items },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingByCategory>);

    renderWithQueryClient(
      <SpendingByCategoryWidget dateRange={dateRange} />,
    );
    // 4 top + 1 "Other" = 5
    expect(screen.getByTestId("pie")).toHaveAttribute("data-count", "5");
  });

  it("shows loading state", () => {
    mockHook.mockReturnValue({
      data: undefined,
      isLoading: true,
    } as unknown as ReturnType<typeof useDashboardSpendingByCategory>);

    renderWithQueryClient(
      <SpendingByCategoryWidget dateRange={dateRange} />,
    );
    expect(screen.getByLabelText("Loading")).toBeInTheDocument();
  });

  it("shows empty state when no data", () => {
    mockHook.mockReturnValue({
      data: { items: [] },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingByCategory>);

    renderWithQueryClient(
      <SpendingByCategoryWidget dateRange={dateRange} />,
    );
    expect(screen.getByText("No data available")).toBeInTheDocument();
  });
});

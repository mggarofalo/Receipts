import { screen } from "@testing-library/react";
import { renderWithQueryClient } from "@/test/test-utils";
import { SpendingByStoreWidget } from "./SpendingByStoreWidget";

vi.mock("recharts", () => ({
  ResponsiveContainer: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="responsive-container">{children}</div>
  ),
  BarChart: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="bar-chart">{children}</div>
  ),
  Bar: () => <div data-testid="bar" />,
  XAxis: () => <div data-testid="x-axis" />,
  YAxis: () => <div data-testid="y-axis" />,
  CartesianGrid: () => <div data-testid="cartesian-grid" />,
  Tooltip: () => <div data-testid="tooltip" />,
}));

vi.mock("@/hooks/useDashboard", () => ({
  useDashboardSpendingByStore: vi.fn(),
}));

import { useDashboardSpendingByStore } from "@/hooks/useDashboard";
const mockHook = vi.mocked(useDashboardSpendingByStore);

const dateRange = { startDate: "2024-01-01", endDate: "2024-01-31" };

const storeData = [
  { location: "Walmart", visitCount: 5, totalAmount: 500, averagePerVisit: 100 },
  { location: "Target", visitCount: 3, totalAmount: 300, averagePerVisit: 100 },
];

describe("SpendingByStoreWidget", () => {
  it("renders chart and table with data", () => {
    mockHook.mockReturnValue({
      data: { items: storeData },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingByStore>);

    renderWithQueryClient(<SpendingByStoreWidget dateRange={dateRange} />);
    expect(screen.getByTestId("bar-chart")).toBeInTheDocument();
    expect(screen.getByText("Walmart")).toBeInTheDocument();
    expect(screen.getByText("Target")).toBeInTheDocument();
  });

  it("shows loading state", () => {
    mockHook.mockReturnValue({
      data: undefined,
      isLoading: true,
    } as unknown as ReturnType<typeof useDashboardSpendingByStore>);

    renderWithQueryClient(<SpendingByStoreWidget dateRange={dateRange} />);
    expect(screen.getByLabelText("Loading")).toBeInTheDocument();
  });

  it("shows empty state when no data", () => {
    mockHook.mockReturnValue({
      data: { items: [] },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingByStore>);

    renderWithQueryClient(<SpendingByStoreWidget dateRange={dateRange} />);
    expect(screen.getByText("No store data")).toBeInTheDocument();
  });

  it("renders table headers", () => {
    mockHook.mockReturnValue({
      data: { items: storeData },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingByStore>);

    renderWithQueryClient(<SpendingByStoreWidget dateRange={dateRange} />);
    expect(screen.getByText("Location")).toBeInTheDocument();
    expect(screen.getByText("Visits")).toBeInTheDocument();
    expect(screen.getByText("Total")).toBeInTheDocument();
    expect(screen.getByText("Avg/Visit")).toBeInTheDocument();
  });

  it("displays visit counts in table", () => {
    mockHook.mockReturnValue({
      data: { items: storeData },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingByStore>);

    renderWithQueryClient(<SpendingByStoreWidget dateRange={dateRange} />);
    expect(screen.getByText("5")).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
  });

  it("has correct title", () => {
    mockHook.mockReturnValue({
      data: { items: storeData },
      isLoading: false,
    } as unknown as ReturnType<typeof useDashboardSpendingByStore>);

    renderWithQueryClient(<SpendingByStoreWidget dateRange={dateRange} />);
    expect(screen.getByText("Spending by Store")).toBeInTheDocument();
  });
});

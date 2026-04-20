import { screen } from "@testing-library/react";
import { renderWithQueryClient } from "@/test/test-utils";
import SpendingByNormalizedDescription from "./SpendingByNormalizedDescription";

vi.mock("@/hooks/useSpendingByNormalizedDescription", () => ({
  useSpendingByNormalizedDescription: vi.fn(),
}));

vi.mock("@/components/dashboard/DateRangeSelector", () => ({
  DateRangeSelector: () => <div data-testid="date-range-selector" />,
}));

vi.mock("@/components/charts", () => ({
  ChartCard: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="chart-card">{children}</div>
  ),
  BarChart: () => <div data-testid="bar-chart" />,
}));

import { useSpendingByNormalizedDescription } from "@/hooks/useSpendingByNormalizedDescription";
const mockHook = vi.mocked(useSpendingByNormalizedDescription);

const sampleItems = [
  {
    canonicalName: "Apples",
    totalAmount: 12.5,
    currency: "USD",
    itemCount: 3,
    firstSeen: null,
    lastSeen: null,
  },
  {
    canonicalName: "Bananas",
    totalAmount: 40,
    currency: "USD",
    itemCount: 5,
    firstSeen: null,
    lastSeen: null,
  },
];

function setupMock(overrides: Record<string, unknown> = {}) {
  mockHook.mockReturnValue({
    data: { items: sampleItems },
    isLoading: false,
    isError: false,
    ...overrides,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
  } as any);
}

describe("SpendingByNormalizedDescription", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("shows loading skeleton", () => {
    setupMock({ isLoading: true, data: undefined });
    renderWithQueryClient(<SpendingByNormalizedDescription />);
    const skeletons = document.querySelectorAll("[data-slot='skeleton']");
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it("shows error state", () => {
    setupMock({ isError: true, data: undefined });
    renderWithQueryClient(<SpendingByNormalizedDescription />);
    expect(
      screen.getByText(/failed to load spending by normalized description/i),
    ).toBeInTheDocument();
  });

  it("shows empty state when no items", () => {
    setupMock({ data: { items: [] } });
    renderWithQueryClient(<SpendingByNormalizedDescription />);
    expect(screen.getByText("No Data")).toBeInTheDocument();
  });

  it("renders items sorted by total desc", () => {
    setupMock();
    renderWithQueryClient(<SpendingByNormalizedDescription />);
    const table = screen.getByRole("table");
    const rows = table.querySelectorAll("tbody tr");
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain("Bananas");
    expect(rows[1].textContent).toContain("Apples");
  });

  it("formats totals as currency and sums grand total", () => {
    setupMock();
    renderWithQueryClient(<SpendingByNormalizedDescription />);
    expect(screen.getByText("$52.50")).toBeInTheDocument();
  });
});

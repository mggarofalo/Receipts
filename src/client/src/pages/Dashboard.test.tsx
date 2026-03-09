import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import Dashboard from "./Dashboard";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/components/dashboard/DateRangeSelector", () => ({
  DateRangeSelector: () => <div data-testid="date-range-selector" />,
}));

vi.mock("@/components/dashboard/SummaryStats", () => ({
  SummaryStats: () => <div data-testid="summary-stats" />,
}));

vi.mock("@/components/dashboard/SpendingOverTimeWidget", () => ({
  SpendingOverTimeWidget: () => <div data-testid="spending-over-time" />,
}));

vi.mock("@/components/dashboard/SpendingByCategoryWidget", () => ({
  SpendingByCategoryWidget: () => <div data-testid="spending-by-category" />,
}));

vi.mock("@/components/dashboard/SpendingByAccountWidget", () => ({
  SpendingByAccountWidget: () => <div data-testid="spending-by-account" />,
}));

vi.mock("@/components/dashboard/RecentReceiptsWidget", () => ({
  RecentReceiptsWidget: () => <div data-testid="recent-receipts" />,
}));

describe("Dashboard", () => {
  it("renders the page heading", () => {
    renderWithProviders(<Dashboard />);
    expect(
      screen.getByRole("heading", { name: /dashboard/i }),
    ).toBeInTheDocument();
  });

  it("renders the date range selector", () => {
    renderWithProviders(<Dashboard />);
    expect(screen.getByTestId("date-range-selector")).toBeInTheDocument();
  });

  it("renders all widget sections", () => {
    renderWithProviders(<Dashboard />);
    expect(screen.getByTestId("summary-stats")).toBeInTheDocument();
    expect(screen.getByTestId("spending-over-time")).toBeInTheDocument();
    expect(screen.getByTestId("spending-by-category")).toBeInTheDocument();
    expect(screen.getByTestId("spending-by-account")).toBeInTheDocument();
    expect(screen.getByTestId("recent-receipts")).toBeInTheDocument();
  });

  it("calls usePageTitle with Dashboard", async () => {
    const { usePageTitle } = await import("@/hooks/usePageTitle");
    renderWithProviders(<Dashboard />);
    expect(usePageTitle).toHaveBeenCalledWith("Dashboard");
  });
});

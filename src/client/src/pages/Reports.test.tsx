import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderWithProviders } from "@/test/test-utils";
import Reports, { REPORTS, DEFAULT_REPORT } from "./Reports";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/components/reports/OutOfBalance", () => ({
  default: () => <div data-testid="report-out-of-balance">Out of Balance</div>,
}));

vi.mock("@/components/reports/ItemSimilarity", () => ({
  default: () => (
    <div data-testid="report-item-similarity">Item Similarity</div>
  ),
}));

vi.mock("@/components/reports/ItemCostOverTime", () => ({
  default: () => (
    <div data-testid="report-item-cost-over-time">Item Cost Over Time</div>
  ),
}));

vi.mock("@/components/reports/SpendingByLocation", () => ({
  default: () => (
    <div data-testid="report-spending-by-location">Spending by Location</div>
  ),
}));

vi.mock("@/components/reports/CategoryTrends", () => ({
  default: () => (
    <div data-testid="report-category-trends">Category Trends</div>
  ),
}));

vi.mock("@/components/reports/DuplicateDetection", () => ({
  default: () => (
    <div data-testid="report-duplicate-detection">Duplicate Detection</div>
  ),
}));

vi.mock("@/components/reports/UncategorizedItems", () => ({
  default: () => (
    <div data-testid="report-uncategorized-items">Uncategorized Items</div>
  ),
}));

describe("Reports", () => {
  it("renders the page heading", () => {
    renderWithProviders(<Reports />, { route: "/reports" });
    expect(
      screen.getByRole("heading", { name: /reports/i }),
    ).toBeInTheDocument();
  });

  it("renders all report tabs", () => {
    renderWithProviders(<Reports />, { route: "/reports" });
    const tablist = screen.getByRole("tablist");
    for (const report of REPORTS) {
      expect(
        within(tablist).getByRole("tab", { name: report.name }),
      ).toBeInTheDocument();
    }
  });

  it("defaults to the first report when no query param", async () => {
    renderWithProviders(<Reports />, { route: "/reports" });
    expect(
      await screen.findByTestId("report-out-of-balance"),
    ).toBeInTheDocument();
  });

  it("selects the report specified by query param", async () => {
    renderWithProviders(<Reports />, {
      route: "/reports?report=item-similarity",
    });
    expect(
      await screen.findByTestId("report-item-similarity"),
    ).toBeInTheDocument();
  });

  it("falls back to default report for invalid query param", async () => {
    renderWithProviders(<Reports />, {
      route: "/reports?report=nonexistent",
    });
    expect(
      await screen.findByTestId("report-out-of-balance"),
    ).toBeInTheDocument();
  });

  it("switches report when a tab is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<Reports />, { route: "/reports" });

    await user.click(screen.getByRole("tab", { name: "Category Trends" }));

    expect(
      await screen.findByTestId("report-category-trends"),
    ).toBeInTheDocument();
  });

  it("calls usePageTitle with the active report name", async () => {
    const { usePageTitle } = await import("@/hooks/usePageTitle");
    renderWithProviders(<Reports />, {
      route: "/reports?report=duplicate-detection",
    });
    expect(usePageTitle).toHaveBeenCalledWith(
      "Reports - Duplicate Detection",
    );
  });

  it("exports REPORTS config with correct number of reports", () => {
    expect(REPORTS).toHaveLength(7);
  });

  it("exports DEFAULT_REPORT as out-of-balance", () => {
    expect(DEFAULT_REPORT).toBe("out-of-balance");
  });

  it("has the default tab marked as selected", () => {
    renderWithProviders(<Reports />, { route: "/reports" });
    const tab = screen.getByRole("tab", { name: "Out of Balance" });
    expect(tab).toHaveAttribute("data-state", "active");
  });

  it("marks the correct tab as selected from query param", () => {
    renderWithProviders(<Reports />, {
      route: "/reports?report=spending-by-location",
    });
    const tab = screen.getByRole("tab", { name: "Spending by Location" });
    expect(tab).toHaveAttribute("data-state", "active");
  });
});

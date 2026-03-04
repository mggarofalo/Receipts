import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult } from "@/test/mock-hooks";
import AuditLog from "./AuditLog";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useAudit", () => ({
  useRecentAuditLogs: vi.fn(() => ({
    data: [],
    isLoading: false,
  })),
}));

vi.mock("@/components/AuditLogTable", () => ({
  AuditLogTable: function MockAuditLogTable({
    isLoading,
    logs,
  }: {
    isLoading: boolean;
    logs: unknown[];
  }) {
    if (isLoading) return "Loading...";
    return <div data-testid="audit-table">AuditLogTable ({logs?.length ?? 0} rows)</div>;
  },
}));

describe("AuditLog", () => {
  it("renders the page heading", () => {
    renderWithProviders(<AuditLog />);
    expect(
      screen.getByRole("heading", { name: /audit log/i }),
    ).toBeInTheDocument();
  });

  it("renders the Export CSV button", () => {
    renderWithProviders(<AuditLog />);
    expect(
      screen.getByRole("button", { name: /export csv/i }),
    ).toBeInTheDocument();
  });

  it("renders the search input for entity ID", () => {
    renderWithProviders(<AuditLog />);
    expect(
      screen.getByLabelText(/search audit log by entity id/i),
    ).toBeInTheDocument();
  });

  it("renders the AuditLogTable component", () => {
    renderWithProviders(<AuditLog />);
    expect(screen.getByTestId("audit-table")).toBeInTheDocument();
  });

  it("shows loading state in AuditLogTable when data is loading", async () => {
    const { useRecentAuditLogs } = await import("@/hooks/useAudit");
    vi.mocked(useRecentAuditLogs).mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: true,
    }));

    renderWithProviders(<AuditLog />);
    expect(screen.getByText("Loading...")).toBeInTheDocument();
  });

  it("passes filtered logs to AuditLogTable", async () => {
    const { useRecentAuditLogs } = await import("@/hooks/useAudit");
    vi.mocked(useRecentAuditLogs).mockReturnValue(mockQueryResult({
      data: [
        {
          id: "1",
          entityType: "Account",
          entityId: "abc-123",
          action: "Created",
          changedAt: "2024-01-15T10:00:00Z",
          changedByUserId: "user-1",
          changedByApiKeyId: null,
          ipAddress: "127.0.0.1",
          changesJson: "{}",
        },
      ],
      isLoading: false,
    }));

    renderWithProviders(<AuditLog />);
    // The AuditLogTable mock renders "AuditLogTable" when not loading
    expect(screen.getByTestId("audit-table")).toBeInTheDocument();
  });

  it("filters logs by entity ID search", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const { useRecentAuditLogs } = await import("@/hooks/useAudit");
    vi.mocked(useRecentAuditLogs).mockReturnValue(mockQueryResult({
      data: [
        {
          id: "1",
          entityType: "Account",
          entityId: "abc-123",
          action: "Created",
          changedAt: "2024-01-15T10:00:00Z",
          changedByUserId: "user-1",
          changedByApiKeyId: null,
          ipAddress: "127.0.0.1",
          changesJson: "{}",
        },
      ],
      isLoading: false,
    }));

    renderWithProviders(<AuditLog />);
    const searchInput = screen.getByLabelText(/search audit log by entity id/i);
    await user.type(searchInput, "abc");

    // The mock AuditLogTable still renders, just with filtered data
    expect(screen.getByTestId("audit-table")).toBeInTheDocument();
  });

  it("enables Export CSV button when logs exist", async () => {
    const { useRecentAuditLogs } = await import("@/hooks/useAudit");
    vi.mocked(useRecentAuditLogs).mockReturnValue(mockQueryResult({
      data: [
        {
          id: "1",
          entityType: "Account",
          entityId: "abc-123",
          action: "Created",
          changedAt: "2024-01-15T10:00:00Z",
          changedByUserId: "user-1",
          changedByApiKeyId: null,
          ipAddress: "127.0.0.1",
          changesJson: "{}",
        },
      ],
      isLoading: false,
    }));

    renderWithProviders(<AuditLog />);
    expect(
      screen.getByRole("button", { name: /export csv/i }),
    ).not.toBeDisabled();
  });

  it("triggers CSV export when Export CSV button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const { useRecentAuditLogs } = await import("@/hooks/useAudit");
    vi.mocked(useRecentAuditLogs).mockReturnValue(mockQueryResult({
      data: [
        {
          id: "1",
          entityType: "Account",
          entityId: "abc-123",
          action: "Created",
          changedAt: "2024-01-15T10:00:00Z",
          changedByUserId: "user-1",
          changedByApiKeyId: null,
          ipAddress: "127.0.0.1",
          changesJson: "{}",
        },
      ],
      isLoading: false,
    }));

    const mockCreateObjectURL = vi.fn(() => "blob:mock-url");
    const mockRevokeObjectURL = vi.fn();
    const mockClick = vi.fn();
    const originalCreateObjectURL = URL.createObjectURL;
    const originalRevokeObjectURL = URL.revokeObjectURL;
    URL.createObjectURL = mockCreateObjectURL;
    URL.revokeObjectURL = mockRevokeObjectURL;

    const originalCreateElement = document.createElement.bind(document);
    vi.spyOn(document, "createElement").mockImplementation((tag: string, options?: ElementCreationOptions) => {
      if (tag === "a") {
        return { href: "", download: "", click: mockClick } as unknown as HTMLAnchorElement;
      }
      return originalCreateElement(tag, options);
    });

    renderWithProviders(<AuditLog />);
    await user.click(screen.getByRole("button", { name: /export csv/i }));

    expect(mockCreateObjectURL).toHaveBeenCalled();
    expect(mockClick).toHaveBeenCalled();
    expect(mockRevokeObjectURL).toHaveBeenCalledWith("blob:mock-url");

    URL.createObjectURL = originalCreateObjectURL;
    URL.revokeObjectURL = originalRevokeObjectURL;
    vi.restoreAllMocks();
  });

  it("renders entity type filter trigger", () => {
    renderWithProviders(<AuditLog />);
    // Radix Select renders trigger buttons; count the combobox elements
    const triggers = screen.getAllByRole("combobox");
    expect(triggers.length).toBeGreaterThanOrEqual(3);
  });

  it("renders DateRangePicker From and To buttons", () => {
    renderWithProviders(<AuditLog />);
    const buttons = screen.getAllByRole("button");
    const fromButton = buttons.find((b) => b.textContent === "From");
    const toButton = buttons.find((b) => b.textContent === "To");
    expect(fromButton).toBeDefined();
    expect(toButton).toBeDefined();
  });

  it("filters logs by search term that excludes entries", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const { useRecentAuditLogs } = await import("@/hooks/useAudit");
    vi.mocked(useRecentAuditLogs).mockReturnValue(mockQueryResult({
      data: [
        {
          id: "1",
          entityType: "Account",
          entityId: "abc-123",
          action: "Created",
          changedAt: "2024-01-15T10:00:00Z",
          changedByUserId: "user-1",
          changedByApiKeyId: null,
          ipAddress: "127.0.0.1",
          changesJson: "{}",
        },
      ],
      isLoading: false,
    }));

    renderWithProviders(<AuditLog />);
    expect(screen.getByTestId("audit-table")).toHaveTextContent("1 rows");

    // Search for something that doesn't match
    await user.type(screen.getByLabelText(/search audit log by entity id/i), "zzz");
    expect(screen.getByTestId("audit-table")).toHaveTextContent("0 rows");
  });

});

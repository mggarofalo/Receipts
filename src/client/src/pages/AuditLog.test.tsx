import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockQueryResult } from "@/test/mock-hooks";
import AuditLog from "./AuditLog";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useEnumMetadata", () => ({
  useEnumMetadata: vi.fn(() => ({
    adjustmentTypes: [],
    authEventTypes: [],
    pricingModes: [],
    auditActions: [],
    entityTypes: [],
    adjustmentTypeLabels: {},
    authEventLabels: {},
    pricingModeLabels: {},
    auditActionLabels: {},
    entityTypeLabels: {},
    isLoading: false,
  })),
}));

vi.mock("@/hooks/useAudit", () => ({
  useRecentAuditLogs: vi.fn(() => ({
    data: { data: [], total: 0, offset: 0, limit: 50 },
    isLoading: false,
  })),
}));

vi.mock("@/hooks/useServerPagination", () => ({
  useServerPagination: vi.fn(() => ({
    offset: 0,
    limit: 50,
    currentPage: 1,
    pageSize: 50,
    totalPages: () => 1,
    setPage: vi.fn(),
    setPageSize: vi.fn(),
    resetPage: vi.fn(),
  })),
}));

vi.mock("@/hooks/useServerSort", () => ({
  useServerSort: vi.fn(() => ({
    sortBy: "changedAt",
    sortDirection: "desc",
    toggleSort: vi.fn(),
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

vi.mock("@/components/Pagination", () => ({
  Pagination: function MockPagination() {
    return <div data-testid="pagination">Pagination</div>;
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

  it("renders the AuditLogTable component", () => {
    renderWithProviders(<AuditLog />);
    expect(screen.getByTestId("audit-table")).toBeInTheDocument();
  });

  it("renders the Pagination component", () => {
    renderWithProviders(<AuditLog />);
    expect(screen.getByTestId("pagination")).toBeInTheDocument();
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

  it("passes server-returned logs to AuditLogTable", async () => {
    const { useRecentAuditLogs } = await import("@/hooks/useAudit");
    vi.mocked(useRecentAuditLogs).mockReturnValue(mockQueryResult({
      data: {
        data: [
          {
            id: "1",
            entityType: "Account",
            entityId: "abc-123",
            action: "Create",
            changedAt: "2024-01-15T10:00:00Z",
            changedByUserId: "user-1",
            changedByApiKeyId: null,
            ipAddress: "127.0.0.1",
            changesJson: "{}",
          },
        ],
        total: 1,
        offset: 0,
        limit: 50,
      },
      isLoading: false,
    }));

    renderWithProviders(<AuditLog />);
    expect(screen.getByTestId("audit-table")).toHaveTextContent("1 rows");
  });

  it("enables Export CSV button when logs exist", async () => {
    const { useRecentAuditLogs } = await import("@/hooks/useAudit");
    vi.mocked(useRecentAuditLogs).mockReturnValue(mockQueryResult({
      data: {
        data: [
          {
            id: "1",
            entityType: "Account",
            entityId: "abc-123",
            action: "Create",
            changedAt: "2024-01-15T10:00:00Z",
            changedByUserId: "user-1",
            changedByApiKeyId: null,
            ipAddress: "127.0.0.1",
            changesJson: "{}",
          },
        ],
        total: 1,
        offset: 0,
        limit: 50,
      },
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
      data: {
        data: [
          {
            id: "1",
            entityType: "Account",
            entityId: "abc-123",
            action: "Create",
            changedAt: "2024-01-15T10:00:00Z",
            changedByUserId: "user-1",
            changedByApiKeyId: null,
            ipAddress: "127.0.0.1",
            changesJson: "{}",
          },
        ],
        total: 1,
        offset: 0,
        limit: 50,
      },
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
    expect(triggers.length).toBeGreaterThanOrEqual(2);
  });

  it("renders DateRangePicker From and To buttons", () => {
    renderWithProviders(<AuditLog />);
    const buttons = screen.getAllByRole("button");
    const fromButton = buttons.find((b) => b.textContent === "From");
    const toButton = buttons.find((b) => b.textContent === "To");
    expect(fromButton).toBeDefined();
    expect(toButton).toBeDefined();
  });

  it("passes filter params to useRecentAuditLogs", async () => {
    const { useRecentAuditLogs } = await import("@/hooks/useAudit");
    const mockFn = vi.mocked(useRecentAuditLogs);
    mockFn.mockReturnValue(mockQueryResult({
      data: { data: [], total: 0, offset: 0, limit: 50 },
      isLoading: false,
    }));

    renderWithProviders(<AuditLog />);

    // Verify it was called with filter object (server-side filtering)
    expect(mockFn).toHaveBeenCalledWith(
      expect.objectContaining({
        offset: expect.any(Number),
        limit: expect.any(Number),
      }),
    );
  });
});

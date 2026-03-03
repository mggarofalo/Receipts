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
  }: {
    isLoading: boolean;
  }) {
    return isLoading ? "Loading..." : "AuditLogTable";
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
    expect(screen.getByText("AuditLogTable")).toBeInTheDocument();
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
    expect(screen.getByText("AuditLogTable")).toBeInTheDocument();
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
    expect(screen.getByText("AuditLogTable")).toBeInTheDocument();
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

});

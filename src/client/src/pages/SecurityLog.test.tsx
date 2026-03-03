import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import SecurityLog from "./SecurityLog";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/usePermission", () => ({
  usePermission: vi.fn(() => ({
    isAdmin: () => true,
  })),
}));

vi.mock("@/hooks/useAuthAudit", () => ({
  useMyAuthAuditLog: vi.fn(() => ({ data: [], isLoading: false })),
  useRecentAuthAuditLogs: vi.fn(() => ({ data: [], isLoading: false })),
  useFailedAuthAttempts: vi.fn(() => ({ data: [], isLoading: false })),
}));

vi.mock("@/components/AuthAuditTable", () => ({
  AuthAuditTable: function MockAuthAuditTable({
    isLoading,
  }: {
    isLoading: boolean;
  }) {
    return isLoading ? "Loading..." : "AuthAuditTable";
  },
}));

describe("SecurityLog", () => {
  it("renders the page heading", () => {
    renderWithProviders(<SecurityLog />);
    expect(
      screen.getByRole("heading", { name: /security log/i }),
    ).toBeInTheDocument();
  });

  it("renders the My Activity tab", () => {
    renderWithProviders(<SecurityLog />);
    expect(
      screen.getByRole("tab", { name: /my activity/i }),
    ).toBeInTheDocument();
  });

  it("renders admin-only tabs when user is admin", () => {
    renderWithProviders(<SecurityLog />);
    expect(
      screen.getByRole("tab", { name: /all events/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("tab", { name: /failed logins/i }),
    ).toBeInTheDocument();
  });

  it("hides admin tabs when user is not admin", async () => {
    const { usePermission } = await import("@/hooks/usePermission");
    vi.mocked(usePermission).mockReturnValue({
      isAdmin: () => false,
    } as unknown as ReturnType<typeof usePermission>);

    renderWithProviders(<SecurityLog />);
    expect(
      screen.queryByRole("tab", { name: /all events/i }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole("tab", { name: /failed logins/i }),
    ).not.toBeInTheDocument();
  });
});

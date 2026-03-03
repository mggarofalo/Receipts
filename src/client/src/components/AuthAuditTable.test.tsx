import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { AuthAuditTable } from "./AuthAuditTable";
import { TooltipProvider } from "@/components/ui/tooltip";
import type { AuthAuditLog } from "@/lib/audit-utils";

const mockLogs: AuthAuditLog[] = [
  {
    id: "auth-log-1",
    eventType: "Login",
    userId: "user-001",
    apiKeyId: null,
    username: "admin@example.com",
    success: true,
    failureReason: null,
    ipAddress: "192.168.1.1",
    userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
    timestamp: "2025-01-15T10:30:00Z",
    metadataJson: null,
  },
  {
    id: "auth-log-2",
    eventType: "Login",
    userId: "user-002",
    apiKeyId: null,
    username: "user@example.com",
    success: false,
    failureReason: "Invalid password",
    ipAddress: "10.0.0.1",
    userAgent: null,
    timestamp: "2025-01-16T14:00:00Z",
    metadataJson: null,
  },
];

function renderWithTooltip(ui: React.ReactElement) {
  return render(<TooltipProvider>{ui}</TooltipProvider>);
}

describe("AuthAuditTable", () => {
  it("renders loading skeletons when isLoading is true", () => {
    const { container } = renderWithTooltip(
      <AuthAuditTable logs={[]} isLoading={true} />,
    );
    const skeletons = container.querySelectorAll('[data-slot="skeleton"]');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it("renders empty state when logs array is empty", () => {
    renderWithTooltip(
      <AuthAuditTable logs={[]} isLoading={false} />,
    );
    expect(screen.getByText("No auth audit entries found.")).toBeInTheDocument();
  });

  it("renders table headers without Username column by default", () => {
    renderWithTooltip(
      <AuthAuditTable logs={mockLogs} isLoading={false} />,
    );
    expect(screen.getByText("Timestamp")).toBeInTheDocument();
    expect(screen.getByText("Event Type")).toBeInTheDocument();
    expect(screen.getByText("Result")).toBeInTheDocument();
    expect(screen.getByText("IP Address")).toBeInTheDocument();
    expect(screen.getByText("User Agent")).toBeInTheDocument();
    expect(screen.queryByText("Username")).not.toBeInTheDocument();
  });

  it("renders Username column when showUsername is true", () => {
    renderWithTooltip(
      <AuthAuditTable logs={mockLogs} isLoading={false} showUsername={true} />,
    );
    expect(screen.getByText("Username")).toBeInTheDocument();
    expect(screen.getByText("admin@example.com")).toBeInTheDocument();
    expect(screen.getByText("user@example.com")).toBeInTheDocument();
  });

  it("renders event type badges for each log entry", () => {
    renderWithTooltip(
      <AuthAuditTable logs={mockLogs} isLoading={false} />,
    );
    // Both logs have "Login" event type
    const loginBadges = screen.getAllByText("Login");
    expect(loginBadges).toHaveLength(2);
  });

  it("renders Success and Failed result badges", () => {
    renderWithTooltip(
      <AuthAuditTable logs={mockLogs} isLoading={false} />,
    );
    expect(screen.getByText("Success")).toBeInTheDocument();
    expect(screen.getByText("Failed")).toBeInTheDocument();
  });
});

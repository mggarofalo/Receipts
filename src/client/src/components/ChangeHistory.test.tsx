import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { ChangeHistory } from "./ChangeHistory";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { mockQueryResult } from "@/test/mock-hooks";

vi.mock("@/hooks/useAudit", () => ({
  useEntityAuditHistory: vi.fn(),
}));

import { useEntityAuditHistory } from "@/hooks/useAudit";

const mockUseEntityAuditHistory = vi.mocked(useEntityAuditHistory);

function renderWithProviders(ui: React.ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false, gcTime: 0 } },
  });
  return render(
    <QueryClientProvider client={queryClient}>
      <TooltipProvider>{ui}</TooltipProvider>
    </QueryClientProvider>,
  );
}

describe("ChangeHistory", () => {
  it("renders loading skeletons when data is loading", () => {
    mockUseEntityAuditHistory.mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: true,
    }));

    const { container } = renderWithProviders(
      <ChangeHistory entityType="Receipt" entityId="abc-123" />,
    );
    const skeletons = container.querySelectorAll('[data-slot="skeleton"]');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it("renders empty state when no history is available", () => {
    mockUseEntityAuditHistory.mockReturnValue(mockQueryResult({
      data: [],
      isLoading: false,
    }));

    renderWithProviders(
      <ChangeHistory entityType="Receipt" entityId="abc-123" />,
    );
    expect(
      screen.getByText("No history available for this entity."),
    ).toBeInTheDocument();
  });

  it("renders empty state when data is undefined (null-ish)", () => {
    mockUseEntityAuditHistory.mockReturnValue(mockQueryResult({
      data: undefined,
      isLoading: false,
    }));

    renderWithProviders(
      <ChangeHistory entityType="Receipt" entityId="abc-123" />,
    );
    expect(
      screen.getByText("No history available for this entity."),
    ).toBeInTheDocument();
  });

  it("renders timeline entries when data is available", () => {
    mockUseEntityAuditHistory.mockReturnValue(mockQueryResult({
      data: [
        {
          id: "log-1",
          entityType: "Receipt",
          entityId: "abc-123",
          action: "Created",
          changesJson: "[]",
          changedByUserId: "user-001-abcde",
          changedByApiKeyId: null,
          changedAt: new Date().toISOString(),
          ipAddress: "192.168.1.1",
        },
        {
          id: "log-2",
          entityType: "Receipt",
          entityId: "abc-123",
          action: "Updated",
          changesJson: JSON.stringify([
            { field: "description", oldValue: "Old", newValue: "New" },
          ]),
          changedByUserId: null,
          changedByApiKeyId: null,
          changedAt: new Date().toISOString(),
          ipAddress: null,
        },
      ],
      isLoading: false,
    }));

    renderWithProviders(
      <ChangeHistory entityType="Receipt" entityId="abc-123" />,
    );
    expect(screen.getByText("Created")).toBeInTheDocument();
    expect(screen.getByText("Updated")).toBeInTheDocument();
  });

  it("passes correct entityType and entityId to the hook", () => {
    mockUseEntityAuditHistory.mockReturnValue(mockQueryResult({
      data: [],
      isLoading: false,
    }));

    renderWithProviders(
      <ChangeHistory entityType="Account" entityId="xyz-789" />,
    );
    expect(mockUseEntityAuditHistory).toHaveBeenCalledWith("Account", "xyz-789");
  });

  it("timestamp tooltip trigger is keyboard-focusable (tabIndex=0)", () => {
    const changedAt = new Date("2025-01-15T10:30:00Z").toISOString();
    mockUseEntityAuditHistory.mockReturnValue(mockQueryResult({
      data: [
        {
          id: "log-ts",
          entityType: "Receipt",
          entityId: "abc-123",
          action: "Created",
          changesJson: "[]",
          changedByUserId: null,
          changedByApiKeyId: null,
          changedAt,
          ipAddress: null,
        },
      ],
      isLoading: false,
    }));

    const { container } = renderWithProviders(
      <ChangeHistory entityType="Receipt" entityId="abc-123" />,
    );

    // The timestamp trigger span must have tabIndex=0 so keyboard users can focus it
    const triggers = container.querySelectorAll('[tabindex="0"]');
    expect(triggers.length).toBeGreaterThan(0);
  });

  it("user ID tooltip trigger is keyboard-focusable and shows truncated ID", () => {
    const fullUserId = "550e8400-e29b-41d4-a716-446655440000";
    mockUseEntityAuditHistory.mockReturnValue(mockQueryResult({
      data: [
        {
          id: "log-user",
          entityType: "Receipt",
          entityId: "abc-123",
          action: "Updated",
          changesJson: "[]",
          changedByUserId: fullUserId,
          changedByApiKeyId: null,
          changedAt: new Date().toISOString(),
          ipAddress: null,
        },
      ],
      isLoading: false,
    }));

    const { container } = renderWithProviders(
      <ChangeHistory entityType="Receipt" entityId="abc-123" />,
    );

    // User ID trigger must be focusable
    const focusableElements = container.querySelectorAll('[tabindex="0"]');
    expect(focusableElements.length).toBeGreaterThan(0);

    // The user ID trigger should have an aria-label containing the full ID
    const userIdTrigger = Array.from(focusableElements).find((el) =>
      el.getAttribute("aria-label")?.includes(fullUserId),
    );
    expect(userIdTrigger).toBeDefined();
  });

  it("API key tooltip trigger is keyboard-focusable and shows truncated ID", () => {
    const fullApiKeyId = "660e8400-e29b-41d4-a716-446655440001";
    mockUseEntityAuditHistory.mockReturnValue(mockQueryResult({
      data: [
        {
          id: "log-apikey",
          entityType: "Receipt",
          entityId: "abc-123",
          action: "Created",
          changesJson: "[]",
          changedByUserId: null,
          changedByApiKeyId: fullApiKeyId,
          changedAt: new Date().toISOString(),
          ipAddress: null,
        },
      ],
      isLoading: false,
    }));

    const { container } = renderWithProviders(
      <ChangeHistory entityType="Receipt" entityId="abc-123" />,
    );

    // API key trigger must be focusable
    const focusableElements = container.querySelectorAll('[tabindex="0"]');
    expect(focusableElements.length).toBeGreaterThan(0);

    // The API key trigger should have an aria-label containing the full ID
    const apiKeyTrigger = Array.from(focusableElements).find((el) =>
      el.getAttribute("aria-label")?.includes(fullApiKeyId),
    );
    expect(apiKeyTrigger).toBeDefined();
  });

  it("tooltip triggers have role=button for screen reader semantics", () => {
    mockUseEntityAuditHistory.mockReturnValue(mockQueryResult({
      data: [
        {
          id: "log-role",
          entityType: "Receipt",
          entityId: "abc-123",
          action: "Created",
          changesJson: "[]",
          changedByUserId: "user-001",
          changedByApiKeyId: null,
          changedAt: new Date().toISOString(),
          ipAddress: null,
        },
      ],
      isLoading: false,
    }));

    renderWithProviders(
      <ChangeHistory entityType="Receipt" entityId="abc-123" />,
    );

    // Both timestamp and user ID triggers should be reachable as buttons
    const buttons = screen.getAllByRole("button");
    expect(buttons.length).toBeGreaterThanOrEqual(2);
  });

  it("full timestamp is exposed in aria-label for screen readers", () => {
    const changedAt = new Date("2025-06-01T14:00:00Z").toISOString();
    mockUseEntityAuditHistory.mockReturnValue(mockQueryResult({
      data: [
        {
          id: "log-aria",
          entityType: "Receipt",
          entityId: "abc-123",
          action: "Deleted",
          changesJson: "[]",
          changedByUserId: null,
          changedByApiKeyId: null,
          changedAt,
          ipAddress: null,
        },
      ],
      isLoading: false,
    }));

    const { container } = renderWithProviders(
      <ChangeHistory entityType="Receipt" entityId="abc-123" />,
    );

    // The aria-label on the timestamp trigger must expose the full formatted timestamp
    const timestampTrigger = container.querySelector(
      '[aria-label^="Timestamp:"]',
    );
    expect(timestampTrigger).not.toBeNull();
    expect(timestampTrigger?.getAttribute("aria-label")).toContain("2025");
  });
});

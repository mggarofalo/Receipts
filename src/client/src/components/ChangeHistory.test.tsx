import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { ChangeHistory } from "./ChangeHistory";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

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
    mockUseEntityAuditHistory.mockReturnValue({
      data: undefined,
      isLoading: true,
    } as ReturnType<typeof useEntityAuditHistory>);

    const { container } = renderWithProviders(
      <ChangeHistory entityType="Receipt" entityId="abc-123" />,
    );
    const skeletons = container.querySelectorAll('[data-slot="skeleton"]');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it("renders empty state when no history is available", () => {
    mockUseEntityAuditHistory.mockReturnValue({
      data: [],
      isLoading: false,
    } as unknown as ReturnType<typeof useEntityAuditHistory>);

    renderWithProviders(
      <ChangeHistory entityType="Receipt" entityId="abc-123" />,
    );
    expect(
      screen.getByText("No history available for this entity."),
    ).toBeInTheDocument();
  });

  it("renders empty state when data is undefined (null-ish)", () => {
    mockUseEntityAuditHistory.mockReturnValue({
      data: undefined,
      isLoading: false,
    } as ReturnType<typeof useEntityAuditHistory>);

    renderWithProviders(
      <ChangeHistory entityType="Receipt" entityId="abc-123" />,
    );
    expect(
      screen.getByText("No history available for this entity."),
    ).toBeInTheDocument();
  });

  it("renders timeline entries when data is available", () => {
    mockUseEntityAuditHistory.mockReturnValue({
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
    } as unknown as ReturnType<typeof useEntityAuditHistory>);

    renderWithProviders(
      <ChangeHistory entityType="Receipt" entityId="abc-123" />,
    );
    expect(screen.getByText("Created")).toBeInTheDocument();
    expect(screen.getByText("Updated")).toBeInTheDocument();
  });

  it("passes correct entityType and entityId to the hook", () => {
    mockUseEntityAuditHistory.mockReturnValue({
      data: [],
      isLoading: false,
    } as unknown as ReturnType<typeof useEntityAuditHistory>);

    renderWithProviders(
      <ChangeHistory entityType="Account" entityId="xyz-789" />,
    );
    expect(mockUseEntityAuditHistory).toHaveBeenCalledWith("Account", "xyz-789");
  });
});

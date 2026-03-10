import { describe, it, expect, vi, beforeEach, type Mock } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { createElement, type ReactNode } from "react";

vi.mock("@/lib/api-client", () => ({
  default: {
    GET: vi.fn(),
    POST: vi.fn(),
    PUT: vi.fn(),
    DELETE: vi.fn(),
  },
}));

vi.mock("sonner", () => ({
  toast: { success: vi.fn(), error: vi.fn(), info: vi.fn() },
}));

import client from "@/lib/api-client";
import { useEnumMetadata } from "./useEnumMetadata";

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false, gcTime: 0 } },
  });
  return function Wrapper({ children }: { children: ReactNode }) {
    return createElement(QueryClientProvider, { client: queryClient }, children);
  };
}

const mockResponse = {
  adjustmentTypes: [
    { value: "Tip", label: "Tip" },
    { value: "Discount", label: "Discount" },
  ],
  authEventTypes: [
    { value: "Login", label: "Login" },
    { value: "LoginFailed", label: "Login Failed" },
  ],
  pricingModes: [
    { value: "quantity", label: "Quantity" },
    { value: "flat", label: "Flat" },
  ],
  auditActions: [
    { value: "Create", label: "Created" },
    { value: "Update", label: "Updated" },
  ],
  entityTypes: [
    { value: "Account", label: "Account" },
    { value: "ReceiptItem", label: "Receipt Item" },
  ],
};

beforeEach(() => {
  vi.clearAllMocks();
});

describe("useEnumMetadata", () => {
  it("fetches and returns enum arrays", async () => {
    (client.GET as Mock).mockResolvedValue({
      data: mockResponse,
      error: null,
    });

    const { result } = renderHook(() => useEnumMetadata(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(client.GET).toHaveBeenCalledWith("/api/metadata/enums");
    expect(result.current.adjustmentTypes).toEqual(mockResponse.adjustmentTypes);
    expect(result.current.authEventTypes).toEqual(mockResponse.authEventTypes);
    expect(result.current.pricingModes).toEqual(mockResponse.pricingModes);
    expect(result.current.auditActions).toEqual(mockResponse.auditActions);
    expect(result.current.entityTypes).toEqual(mockResponse.entityTypes);
  });

  it("derives label lookup maps", async () => {
    (client.GET as Mock).mockResolvedValue({
      data: mockResponse,
      error: null,
    });

    const { result } = renderHook(() => useEnumMetadata(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.adjustmentTypeLabels).toEqual({
      Tip: "Tip",
      Discount: "Discount",
    });
    expect(result.current.authEventLabels).toEqual({
      Login: "Login",
      LoginFailed: "Login Failed",
    });
    expect(result.current.pricingModeLabels).toEqual({
      quantity: "Quantity",
      flat: "Flat",
    });
    expect(result.current.auditActionLabels).toEqual({
      Create: "Created",
      Update: "Updated",
    });
    expect(result.current.entityTypeLabels).toEqual({
      Account: "Account",
      ReceiptItem: "Receipt Item",
    });
  });

  it("returns empty arrays while loading", () => {
    (client.GET as Mock).mockReturnValue(new Promise(() => {}));

    const { result } = renderHook(() => useEnumMetadata(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);
    expect(result.current.adjustmentTypes).toEqual([]);
    expect(result.current.authEventTypes).toEqual([]);
    expect(result.current.pricingModes).toEqual([]);
    expect(result.current.auditActions).toEqual([]);
    expect(result.current.entityTypes).toEqual([]);
  });
});

import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { renderHook, waitFor, act } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { createElement, type ReactNode } from "react";

vi.mock("@/lib/auth", () => ({
  getAccessToken: vi.fn(() => "mock-token"),
}));

vi.mock("@/lib/toast", () => ({
  showSuccess: vi.fn(),
  showError: vi.fn(),
}));

import { useBackupExport } from "./useBackup";
import { showSuccess, showError } from "@/lib/toast";

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false, gcTime: 0 } },
  });
  return function Wrapper({ children }: { children: ReactNode }) {
    return createElement(QueryClientProvider, { client: queryClient }, children);
  };
}

describe("useBackupExport", () => {
  const originalFetch = globalThis.fetch;
  const originalCreateObjectURL = globalThis.URL.createObjectURL;
  const originalRevokeObjectURL = globalThis.URL.revokeObjectURL;
  const originalCreateElement = document.createElement.bind(document);

  beforeEach(() => {
    vi.clearAllMocks();
    // Minimal URL mocks — jsdom doesn't implement blob URLs.
    globalThis.URL.createObjectURL = vi.fn(() => "blob:mock");
    globalThis.URL.revokeObjectURL = vi.fn();
    // jsdom warns/throws on anchor.click() because it cannot navigate. Stub
    // the created anchor's click with a no-op so the happy-path test is stable.
    vi.spyOn(document, "createElement").mockImplementation((tagName: string) => {
      const el = originalCreateElement(tagName);
      if (tagName.toLowerCase() === "a") {
        (el as HTMLAnchorElement).click = vi.fn();
      }
      return el;
    });
  });

  afterEach(() => {
    globalThis.fetch = originalFetch;
    globalThis.URL.createObjectURL = originalCreateObjectURL;
    globalThis.URL.revokeObjectURL = originalRevokeObjectURL;
    vi.restoreAllMocks();
  });

  it("downloads the blob and toasts success on 200", async () => {
    const blob = new Blob(["backup-bytes"], { type: "application/octet-stream" });
    globalThis.fetch = vi.fn(async () =>
      new Response(blob, {
        status: 200,
        headers: {
          "Content-Disposition": 'attachment; filename="my-backup.sqlite"',
        },
      }),
    ) as typeof fetch;

    const { result } = renderHook(() => useBackupExport(), {
      wrapper: createWrapper(),
    });

    await act(async () => {
      result.current.mutate();
    });
    await waitFor(() => {
      expect(showSuccess).toHaveBeenCalledWith("Backup exported successfully.");
    });
    expect(globalThis.URL.createObjectURL).toHaveBeenCalled();
    expect(globalThis.URL.revokeObjectURL).toHaveBeenCalledWith("blob:mock");
  });

  it("toasts a permission error on 403", async () => {
    globalThis.fetch = vi.fn(
      async () => new Response("", { status: 403 }),
    ) as typeof fetch;

    const { result } = renderHook(() => useBackupExport(), {
      wrapper: createWrapper(),
    });

    await act(async () => {
      result.current.mutate();
    });
    await waitFor(() => {
      expect(showError).toHaveBeenCalledWith(
        "You do not have permission to export backups.",
      );
    });
  });

  it("toasts a generic error on other failures", async () => {
    globalThis.fetch = vi.fn(
      async () => new Response("", { status: 500 }),
    ) as typeof fetch;

    const { result } = renderHook(() => useBackupExport(), {
      wrapper: createWrapper(),
    });

    await act(async () => {
      result.current.mutate();
    });
    await waitFor(() => {
      expect(showError).toHaveBeenCalledWith("Export failed (500).");
    });
  });
});

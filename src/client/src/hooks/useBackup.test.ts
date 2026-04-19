import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
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
    const fakeBlob = { size: 12, type: "application/octet-stream" } as Blob;
    // Hand-rolled response rather than `new Response(blob, ...)` — node's
    // undici Response requires the init body to expose `.stream()`, which
    // jsdom's Blob doesn't in CI, causing a TypeError before the mutationFn
    // can observe the response.
    globalThis.fetch = vi.fn(async () => ({
      ok: true,
      status: 200,
      headers: {
        get: (name: string) =>
          name.toLowerCase() === "content-disposition"
            ? 'attachment; filename="my-backup.sqlite"'
            : null,
      },
      blob: async () => fakeBlob,
    })) as unknown as typeof fetch;

    const { result } = renderHook(() => useBackupExport(), {
      wrapper: createWrapper(),
    });

    // mutateAsync awaits the mutationFn; onSuccess/onError run synchronously
    // after resolution, so showSuccess/URL spies are observable immediately.
    await act(async () => {
      await result.current.mutateAsync();
    });

    expect(showSuccess).toHaveBeenCalledWith("Backup exported successfully.");
    expect(showError).not.toHaveBeenCalled();
    expect(globalThis.URL.createObjectURL).toHaveBeenCalledWith(fakeBlob);
    expect(globalThis.URL.revokeObjectURL).toHaveBeenCalledWith("blob:mock");
  });

  it("toasts a permission error on 403", async () => {
    globalThis.fetch = vi.fn(async () => ({
      ok: false,
      status: 403,
    })) as unknown as typeof fetch;

    const { result } = renderHook(() => useBackupExport(), {
      wrapper: createWrapper(),
    });

    await act(async () => {
      await result.current.mutateAsync().catch(() => {});
    });
    expect(showError).toHaveBeenCalledWith(
      "You do not have permission to export backups.",
    );
  });

  it("toasts a generic error on other failures", async () => {
    globalThis.fetch = vi.fn(async () => ({
      ok: false,
      status: 500,
    })) as unknown as typeof fetch;

    const { result } = renderHook(() => useBackupExport(), {
      wrapper: createWrapper(),
    });

    await act(async () => {
      await result.current.mutateAsync().catch(() => {});
    });
    expect(showError).toHaveBeenCalledWith("Export failed (500).");
  });
});

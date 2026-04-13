import { http, HttpResponse } from "msw";
import { server } from "@/test/msw/server";
import client, { isTimeoutError } from "./api-client";
import {
  setTokens,
  clearTokens,
  getAccessToken,
  getRefreshToken,
  addPasswordChangeRequiredListener,
} from "./auth";

beforeEach(() => {
  clearTokens();
});

describe("auth middleware (integration)", () => {
  it("injects Bearer token into request headers", async () => {
    let capturedAuth: string | null = null;

    server.use(
      http.get("*/api/cards", ({ request }) => {
        capturedAuth = request.headers.get("Authorization");
        return HttpResponse.json({ data: [], total: 0, offset: 0, limit: 50 });
      }),
    );

    setTokens("my-access-token", "my-refresh-token");
    await client.GET("/api/cards");

    expect(capturedAuth).toBe("Bearer my-access-token");
  });

  it("refreshes token on 401 and retries with new token", async () => {
    let callCount = 0;
    let retryAuth: string | null = null;

    server.use(
      http.get("*/api/cards", ({ request }) => {
        callCount++;
        if (callCount === 1) {
          return HttpResponse.json(
            { message: "Unauthorized" },
            { status: 401 },
          );
        }
        retryAuth = request.headers.get("Authorization");
        return HttpResponse.json({ data: [], total: 0, offset: 0, limit: 50 });
      }),
      http.post("*/api/auth/refresh", () => {
        return HttpResponse.json({
          accessToken: "new-access-token",
          refreshToken: "new-refresh-token",
          expiresIn: 3600,
          mustResetPassword: false,
          tokenType: "Bearer",
          scope: "",
        });
      }),
    );

    setTokens("expired-access-token", "valid-refresh-token");
    const { data } = await client.GET("/api/cards");

    expect(callCount).toBe(2);
    expect(retryAuth).toBe("Bearer new-access-token");
    expect(getAccessToken()).toBe("new-access-token");
    expect(getRefreshToken()).toBe("new-refresh-token");
    expect(data).toEqual({ data: [], total: 0, offset: 0, limit: 50 });
  });

  it("deduplicates concurrent refresh attempts", async () => {
    let refreshCallCount = 0;
    let nextCallId = 0;

    server.use(
      http.get("*/api/cards", () => {
        const callId = nextCallId++;
        // All first attempts return 401
        if (callId < 3) {
          return HttpResponse.json(
            { message: "Unauthorized" },
            { status: 401 },
          );
        }
        // Retries succeed
        return HttpResponse.json({ data: [], total: 0, offset: 0, limit: 50 });
      }),
      http.post("*/api/auth/refresh", () => {
        refreshCallCount++;
        return HttpResponse.json({
          accessToken: "refreshed-token",
          refreshToken: "refreshed-refresh",
          expiresIn: 3600,
          mustResetPassword: false,
          tokenType: "Bearer",
          scope: "",
        });
      }),
    );

    setTokens("expired-token", "valid-refresh-token");

    // Fire 3 concurrent requests — all will get 401 and trigger refresh
    const results = await Promise.all([
      client.GET("/api/cards"),
      client.GET("/api/cards"),
      client.GET("/api/cards"),
    ]);

    // Only one refresh call should have been made
    expect(refreshCallCount).toBe(1);
    // All 3 should have retried and succeeded
    for (const result of results) {
      expect(result.data).toEqual({ data: [], total: 0, offset: 0, limit: 50 });
    }
  });

  it("notifies password-change-required listener on 403 with matching detail", async () => {
    const listener = vi.fn();
    const unsubscribe = addPasswordChangeRequiredListener(listener);

    try {
      server.use(
        http.get("*/api/cards", () => {
          return HttpResponse.json(
            { detail: "Password change required" },
            { status: 403 },
          );
        }),
      );

      setTokens("valid-token", "valid-refresh");
      await client.GET("/api/cards");

      expect(listener).toHaveBeenCalledOnce();
    } finally {
      unsubscribe();
    }
  });

  it("produces a TimeoutError when the request times out", async () => {
    // Build an already-aborted signal without calling abort() (jsdom re-throws abort reasons)
    const timeoutReason = new DOMException("Signal timed out", "TimeoutError");
    const fakeSignal = Object.create(AbortSignal.prototype) as AbortSignal;
    Object.defineProperty(fakeSignal, "aborted", { value: true });
    Object.defineProperty(fakeSignal, "reason", { value: timeoutReason });
    Object.defineProperty(fakeSignal, "throwIfAborted", {
      value() {
        throw timeoutReason;
      },
    });
    Object.defineProperty(fakeSignal, "addEventListener", { value: () => {} });
    Object.defineProperty(fakeSignal, "removeEventListener", {
      value: () => {},
    });
    Object.defineProperty(fakeSignal, "dispatchEvent", { value: () => false });
    Object.defineProperty(fakeSignal, "onabort", {
      value: null,
      writable: true,
    });

    const timeoutSpy = vi
      .spyOn(AbortSignal, "timeout")
      .mockReturnValue(fakeSignal);

    try {
      setTokens("valid-token", "valid-refresh");
      await client.GET("/api/cards");
      expect.fail("Expected request to throw a TimeoutError");
    } catch (err) {
      expect(isTimeoutError(err)).toBe(true);
    } finally {
      timeoutSpy.mockRestore();
    }
  });

  it("clears tokens and redirects to /login when refresh fails", async () => {
    // Replace window.location with a writable stub
    const originalLocation = window.location;
    const fakeLocation = { ...originalLocation, href: "" } as Location;
    Object.defineProperty(window, "location", {
      value: fakeLocation,
      writable: true,
      configurable: true,
    });

    try {
      server.use(
        http.get("*/api/cards", () => {
          return HttpResponse.json(
            { message: "Unauthorized" },
            { status: 401 },
          );
        }),
        http.post("*/api/auth/refresh", () => {
          return HttpResponse.json(
            { message: "Unauthorized" },
            { status: 401 },
          );
        }),
      );

      setTokens("expired-token", "expired-refresh-token");
      await client.GET("/api/cards");

      expect(getAccessToken()).toBeNull();
      expect(getRefreshToken()).toBeNull();
      expect(fakeLocation.href).toBe("/login");
    } finally {
      Object.defineProperty(window, "location", {
        value: originalLocation,
        writable: true,
        configurable: true,
      });
    }
  });
});

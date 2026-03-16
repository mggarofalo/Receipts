import { describe, it, expect, vi, beforeEach } from "vitest";
import type { Middleware, MiddlewareCallbackParams } from "openapi-fetch";

// Capture middleware registered via client.use()
const registeredMiddleware: Middleware[] = [];

vi.mock("openapi-fetch", () => ({
  default: () => ({
    GET: vi.fn(),
    POST: vi.fn(),
    PUT: vi.fn(),
    DELETE: vi.fn(),
    use: (mw: Middleware) => {
      registeredMiddleware.push(mw);
    },
  }),
}));

vi.mock("@/lib/auth", () => ({
  getAccessToken: vi.fn(() => null),
  getRefreshToken: vi.fn(() => null),
  setTokens: vi.fn(),
  clearTokens: vi.fn(),
  notifyTokenRefresh: vi.fn(),
  notifyPasswordChangeRequired: vi.fn(),
}));

vi.mock("@/lib/signalr-connection", () => ({
  getConnectionId: vi.fn(() => null),
}));

// Must import after mocks are set up
import * as auth from "@/lib/auth";
import * as signalr from "@/lib/signalr-connection";

const mockedAuth = vi.mocked(auth);
const mockedSignalR = vi.mocked(signalr);

let authMiddleware: Middleware;
let signalRMiddleware: Middleware;

// Helper to build middleware callback params with correct types
function makeParams(
  request: Request,
  response?: Response,
): MiddlewareCallbackParams & { response: Response } {
  return {
    request,
    schemaPath: "/api/items",
    params: {},
    id: "test",
    options: {} as MiddlewareCallbackParams["options"],
    response: response ?? new Response(),
  };
}

// Helper to create a minimal Request
function makeRequest(url = "https://api.test/api/items"): Request {
  return new Request(url);
}

// Helper to create a minimal Response
function makeResponse(status: number, body?: unknown): Response {
  return new Response(body ? JSON.stringify(body) : null, {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

beforeEach(async () => {
  registeredMiddleware.length = 0;
  vi.resetModules();

  // Re-import to trigger module-level registration
  const mod = await import("./api-client");
  // Extract the isTimeoutError and isNetworkError from the module
  Object.assign(globalThis, { __apiClientModule: mod });

  // The module registers 2 middleware via client.use()
  authMiddleware = registeredMiddleware[0];
  signalRMiddleware = registeredMiddleware[1];

  vi.clearAllMocks();
});

describe("isTimeoutError", () => {
  it("returns true for DOMException with TimeoutError name", async () => {
    const { isTimeoutError } = await import("./api-client");
    const err = new DOMException("The operation timed out", "TimeoutError");
    expect(isTimeoutError(err)).toBe(true);
  });

  it("returns false for DOMException with different name", async () => {
    const { isTimeoutError } = await import("./api-client");
    const err = new DOMException("Aborted", "AbortError");
    expect(isTimeoutError(err)).toBe(false);
  });

  it("returns false for non-DOMException", async () => {
    const { isTimeoutError } = await import("./api-client");
    expect(isTimeoutError(new Error("timeout"))).toBe(false);
    expect(isTimeoutError("timeout")).toBe(false);
    expect(isTimeoutError(null)).toBe(false);
  });
});

describe("isNetworkError", () => {
  it("returns true for TypeError with fetch in message", async () => {
    const { isNetworkError } = await import("./api-client");
    const err = new TypeError("Failed to fetch");
    expect(isNetworkError(err)).toBe(true);
  });

  it("returns false for TypeError without fetch in message", async () => {
    const { isNetworkError } = await import("./api-client");
    const err = new TypeError("Cannot read property 'x'");
    expect(isNetworkError(err)).toBe(false);
  });

  it("returns false for non-TypeError", async () => {
    const { isNetworkError } = await import("./api-client");
    expect(isNetworkError(new Error("fetch failed"))).toBe(false);
    expect(isNetworkError(null)).toBe(false);
  });
});

describe("authMiddleware.onRequest", () => {
  it("attaches Authorization header when token exists", async () => {
    mockedAuth.getAccessToken.mockReturnValue("my-token");
    const request = makeRequest();

    const result = await authMiddleware.onRequest!(makeParams(request));

    expect((result as Request).headers.get("Authorization")).toBe(
      "Bearer my-token",
    );
  });

  it("does not attach Authorization header when no token", async () => {
    mockedAuth.getAccessToken.mockReturnValue(null);
    const request = makeRequest();

    const result = await authMiddleware.onRequest!(makeParams(request));

    expect((result as Request).headers.has("Authorization")).toBe(false);
  });
});

describe("authMiddleware.onResponse", () => {
  const callOnResponse = (request: Request, response: Response) =>
    authMiddleware.onResponse!(makeParams(request, response));

  it("passes through non-401 non-403 responses", async () => {
    const request = makeRequest();
    const response = makeResponse(200);

    const result = await callOnResponse(request, response);

    expect(result).toBe(response);
    expect(mockedAuth.clearTokens).not.toHaveBeenCalled();
  });

  it("handles 403 with password change required", async () => {
    const request = makeRequest();
    const response = makeResponse(403, {
      detail: "Password change required",
    });

    const result = await callOnResponse(request, response);

    expect(result).toBe(response);
    expect(mockedAuth.notifyPasswordChangeRequired).toHaveBeenCalled();
  });

  it("handles 403 with different detail (no password change notification)", async () => {
    const request = makeRequest();
    const response = makeResponse(403, { detail: "Access denied" });

    const result = await callOnResponse(request, response);

    expect(result).toBe(response);
    expect(mockedAuth.notifyPasswordChangeRequired).not.toHaveBeenCalled();
  });

  it("handles 403 with non-JSON body", async () => {
    const request = makeRequest();
    const response = new Response("Forbidden", {
      status: 403,
      headers: { "Content-Type": "text/plain" },
    });

    const result = await callOnResponse(request, response);

    expect(result).toBe(response);
    expect(mockedAuth.notifyPasswordChangeRequired).not.toHaveBeenCalled();
  });

  it("skips refresh for auth endpoints on 401", async () => {
    const request = makeRequest("https://api.test/api/auth/login");
    const response = makeResponse(401);

    const result = await callOnResponse(request, response);

    expect(result).toBe(response);
    expect(mockedAuth.getRefreshToken).not.toHaveBeenCalled();
  });

  it("clears tokens and redirects on failed refresh (no refresh token)", async () => {
    mockedAuth.getRefreshToken.mockReturnValue(null);

    // Mock window.location
    const originalLocation = window.location;
    Object.defineProperty(window, "location", {
      writable: true,
      value: { href: "/" },
    });

    const request = makeRequest();
    const response = makeResponse(401);

    await callOnResponse(request, response);

    expect(mockedAuth.clearTokens).toHaveBeenCalled();
    expect(window.location.href).toBe("/login");

    // Restore
    Object.defineProperty(window, "location", {
      writable: true,
      value: originalLocation,
    });
  });

  it("retries request with new token on successful refresh", async () => {
    mockedAuth.getRefreshToken.mockReturnValue("valid-refresh");
    mockedAuth.getAccessToken.mockReturnValue("new-access-token");

    // Mock the global fetch for the refresh call and retry
    const refreshResponse = new Response(
      JSON.stringify({
        accessToken: "new-access-token",
        refreshToken: "new-refresh",
      }),
      { status: 200 },
    );
    const retryResponse = makeResponse(200, { data: "success" });

    const fetchSpy = vi.spyOn(globalThis, "fetch");
    fetchSpy.mockResolvedValueOnce(refreshResponse);
    fetchSpy.mockResolvedValueOnce(retryResponse);

    const request = makeRequest();
    const response = makeResponse(401);

    const result = await callOnResponse(request, response);

    expect(mockedAuth.setTokens).toHaveBeenCalledWith(
      "new-access-token",
      "new-refresh",
    );
    expect(mockedAuth.notifyTokenRefresh).toHaveBeenCalled();
    expect(result).toBe(retryResponse);

    fetchSpy.mockRestore();
  });

  it("clears tokens when refresh endpoint returns non-OK", async () => {
    mockedAuth.getRefreshToken.mockReturnValue("expired-refresh");

    const originalLocation = window.location;
    Object.defineProperty(window, "location", {
      writable: true,
      value: { href: "/" },
    });

    const fetchSpy = vi.spyOn(globalThis, "fetch");
    fetchSpy.mockResolvedValueOnce(makeResponse(401)); // refresh fails

    const request = makeRequest();
    const response = makeResponse(401);

    await callOnResponse(request, response);

    expect(mockedAuth.clearTokens).toHaveBeenCalled();
    expect(window.location.href).toBe("/login");

    fetchSpy.mockRestore();
    Object.defineProperty(window, "location", {
      writable: true,
      value: originalLocation,
    });
  });

  it("clears tokens when refresh fetch throws", async () => {
    mockedAuth.getRefreshToken.mockReturnValue("some-refresh");

    const originalLocation = window.location;
    Object.defineProperty(window, "location", {
      writable: true,
      value: { href: "/" },
    });

    const fetchSpy = vi.spyOn(globalThis, "fetch");
    fetchSpy.mockRejectedValueOnce(new Error("Network error"));

    const request = makeRequest();
    const response = makeResponse(401);

    await callOnResponse(request, response);

    expect(mockedAuth.clearTokens).toHaveBeenCalled();
    expect(window.location.href).toBe("/login");

    fetchSpy.mockRestore();
    Object.defineProperty(window, "location", {
      writable: true,
      value: originalLocation,
    });
  });
});

describe("signalRConnectionMiddleware.onRequest", () => {
  it("attaches X-SignalR-Connection-Id header when connected", async () => {
    mockedSignalR.getConnectionId.mockReturnValue("conn-abc");
    const request = makeRequest();

    const result = await signalRMiddleware.onRequest!(makeParams(request));

    expect((result as Request).headers.get("X-SignalR-Connection-Id")).toBe(
      "conn-abc",
    );
  });

  it("does not attach header when no connection ID", async () => {
    mockedSignalR.getConnectionId.mockReturnValue(null);
    const request = makeRequest();

    const result = await signalRMiddleware.onRequest!(makeParams(request));

    expect(
      (result as Request).headers.has("X-SignalR-Connection-Id"),
    ).toBe(false);
  });
});

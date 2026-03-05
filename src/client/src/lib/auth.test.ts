import { describe, it, expect, beforeEach, vi } from "vitest";
import {
  getAccessToken,
  getRefreshToken,
  setTokens,
  clearTokens,
  isAuthenticated,
  parseJwtPayload,
  addTokenRefreshListener,
  notifyTokenRefresh,
  addPasswordChangeRequiredListener,
  notifyPasswordChangeRequired,
} from "./auth";

describe("token storage", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("returns null when no access token is stored", () => {
    expect(getAccessToken()).toBeNull();
  });

  it("returns null when no refresh token is stored", () => {
    expect(getRefreshToken()).toBeNull();
  });

  it("stores and retrieves tokens", () => {
    setTokens("access-123", "refresh-456");
    expect(getAccessToken()).toBe("access-123");
    expect(getRefreshToken()).toBe("refresh-456");
  });

  it("clears both tokens", () => {
    setTokens("access-123", "refresh-456");
    clearTokens();
    expect(getAccessToken()).toBeNull();
    expect(getRefreshToken()).toBeNull();
  });

  it("reports authenticated when access token exists", () => {
    setTokens("token", "refresh");
    expect(isAuthenticated()).toBe(true);
  });

  it("reports not authenticated when no access token", () => {
    expect(isAuthenticated()).toBe(false);
  });
});

describe("parseJwtPayload", () => {
  function encodePayload(payload: Record<string, unknown>): string {
    const header = btoa(JSON.stringify({ alg: "HS256", typ: "JWT" }));
    const body = btoa(JSON.stringify(payload));
    return `${header}.${body}.signature`;
  }

  it("parses standard email and role claims", () => {
    const token = encodePayload({
      email: "user@example.com",
      role: ["Admin", "User"],
    });
    const result = parseJwtPayload(token);
    expect(result).toEqual({
      email: "user@example.com",
      roles: ["Admin", "User"],
      mustResetPassword: false,
    });
  });

  it("parses Microsoft identity claims", () => {
    const token = encodePayload({
      "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress":
        "ms@example.com",
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Admin",
    });
    const result = parseJwtPayload(token);
    expect(result).toEqual({
      email: "ms@example.com",
      roles: ["Admin"],
      mustResetPassword: false,
    });
  });

  it("parses must_reset_password claim as true", () => {
    const token = encodePayload({
      email: "user@example.com",
      must_reset_password: "true",
    });
    const result = parseJwtPayload(token);
    expect(result?.mustResetPassword).toBe(true);
  });

  it("parses must_reset_password claim as false when absent", () => {
    const token = encodePayload({
      email: "user@example.com",
    });
    const result = parseJwtPayload(token);
    expect(result?.mustResetPassword).toBe(false);
  });

  it("wraps a single role string into an array", () => {
    const token = encodePayload({
      email: "user@example.com",
      role: "User",
    });
    const result = parseJwtPayload(token);
    expect(result?.roles).toEqual(["User"]);
  });

  it("returns empty roles when no role claim", () => {
    const token = encodePayload({ email: "user@example.com" });
    const result = parseJwtPayload(token);
    expect(result?.roles).toEqual([]);
  });

  it("returns empty email when no email claim", () => {
    const token = encodePayload({ role: "Admin" });
    const result = parseJwtPayload(token);
    expect(result?.email).toBe("");
  });

  it("returns null for invalid token format", () => {
    expect(parseJwtPayload("not-a-jwt")).toBeNull();
  });

  it("returns null for token with only two parts", () => {
    expect(parseJwtPayload("header.payload")).toBeNull();
  });

  it("returns null for token with invalid base64", () => {
    expect(parseJwtPayload("a.!!!invalid!!!.c")).toBeNull();
  });
});

describe("token refresh listeners", () => {
  it("calls all registered listeners on notify", () => {
    const listener1 = vi.fn();
    const listener2 = vi.fn();
    const unsub1 = addTokenRefreshListener(listener1);
    const unsub2 = addTokenRefreshListener(listener2);

    notifyTokenRefresh();

    expect(listener1).toHaveBeenCalledOnce();
    expect(listener2).toHaveBeenCalledOnce();

    unsub1();
    unsub2();
  });

  it("unsubscribe prevents future calls", () => {
    const listener = vi.fn();
    const unsub = addTokenRefreshListener(listener);

    unsub();
    notifyTokenRefresh();

    expect(listener).not.toHaveBeenCalled();
  });
});

describe("password change required listeners", () => {
  it("calls all registered listeners on notify", () => {
    const listener1 = vi.fn();
    const listener2 = vi.fn();
    const unsub1 = addPasswordChangeRequiredListener(listener1);
    const unsub2 = addPasswordChangeRequiredListener(listener2);

    notifyPasswordChangeRequired();

    expect(listener1).toHaveBeenCalledOnce();
    expect(listener2).toHaveBeenCalledOnce();

    unsub1();
    unsub2();
  });

  it("unsubscribe prevents future calls", () => {
    const listener = vi.fn();
    const unsub = addPasswordChangeRequiredListener(listener);

    unsub();
    notifyPasswordChangeRequired();

    expect(listener).not.toHaveBeenCalled();
  });
});

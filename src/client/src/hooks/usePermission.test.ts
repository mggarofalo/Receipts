import { describe, it, expect } from "vitest";
import { renderHook } from "@testing-library/react";
import { usePermission } from "./usePermission";
import { createWrapper } from "@/test/test-utils";

describe("usePermission", () => {
  it("returns roles from auth context", () => {
    const { result } = renderHook(() => usePermission(), {
      wrapper: createWrapper({
        auth: { user: { email: "user@test.com", roles: ["Admin", "User"], mustResetPassword: false } },
      }),
    });
    expect(result.current.roles).toEqual(["Admin", "User"]);
  });

  it("hasRole returns true for existing role", () => {
    const { result } = renderHook(() => usePermission(), {
      wrapper: createWrapper({
        auth: { user: { email: "user@test.com", roles: ["Admin"], mustResetPassword: false } },
      }),
    });
    expect(result.current.hasRole("Admin")).toBe(true);
  });

  it("hasRole returns false for missing role", () => {
    const { result } = renderHook(() => usePermission(), {
      wrapper: createWrapper({
        auth: { user: { email: "user@test.com", roles: ["User"], mustResetPassword: false } },
      }),
    });
    expect(result.current.hasRole("Admin")).toBe(false);
  });

  it("isAdmin returns true for Admin role", () => {
    const { result } = renderHook(() => usePermission(), {
      wrapper: createWrapper({
        auth: { user: { email: "admin@test.com", roles: ["Admin"], mustResetPassword: false } },
      }),
    });
    expect(result.current.isAdmin()).toBe(true);
  });

  it("isAdmin returns false without Admin role", () => {
    const { result } = renderHook(() => usePermission(), {
      wrapper: createWrapper({
        auth: { user: { email: "user@test.com", roles: ["User"], mustResetPassword: false } },
      }),
    });
    expect(result.current.isAdmin()).toBe(false);
  });

  it("returns empty roles when user is null", () => {
    const { result } = renderHook(() => usePermission(), {
      wrapper: createWrapper({ auth: { user: null } }),
    });
    expect(result.current.roles).toEqual([]);
    expect(result.current.isAdmin()).toBe(false);
  });
});

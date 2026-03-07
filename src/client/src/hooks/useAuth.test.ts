import { describe, it, expect } from "vitest";
import { renderHook } from "@testing-library/react";
import { useAuth } from "./useAuth";
import { createWrapper } from "@/test/test-utils";

describe("useAuth", () => {
  it("returns auth context when inside provider", () => {
    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper({
        auth: { user: { userId: "user-id", email: "user@test.com", roles: ["User"], mustResetPassword: false } },
      }),
    });
    expect(result.current.user).toEqual({
      userId: "user-id",
      email: "user@test.com",
      roles: ["User"],
      mustResetPassword: false,
    });
  });

  it("throws when used outside provider", () => {
    expect(() => {
      renderHook(() => useAuth());
    }).toThrow("useAuth must be used within an AuthProvider");
  });
});

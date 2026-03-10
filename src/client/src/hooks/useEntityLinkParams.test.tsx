import { renderHook, act } from "@testing-library/react";
import { MemoryRouter } from "react-router";
import { describe, it, expect } from "vitest";
import { useEntityLinkParams } from "./useEntityLinkParams";
import type { ReactNode } from "react";

function createWrapper(route: string) {
  return function Wrapper({ children }: { children: ReactNode }) {
    return <MemoryRouter initialEntries={[route]}>{children}</MemoryRouter>;
  };
}

describe("useEntityLinkParams", () => {
  it("returns empty params when no search params present", () => {
    const { result } = renderHook(
      () => useEntityLinkParams(["vendorId", "categoryId"]),
      { wrapper: createWrapper("/test") },
    );

    expect(result.current.params).toEqual({});
  });

  it("reads recognized params from URL", () => {
    const { result } = renderHook(
      () => useEntityLinkParams(["vendorId", "categoryId"]),
      { wrapper: createWrapper("/test?vendorId=123&categoryId=456") },
    );

    expect(result.current.params).toEqual({
      vendorId: "123",
      categoryId: "456",
    });
  });

  it("ignores unrecognized params", () => {
    const { result } = renderHook(
      () => useEntityLinkParams(["vendorId"]),
      { wrapper: createWrapper("/test?vendorId=123&unknown=xyz") },
    );

    expect(result.current.params).toEqual({ vendorId: "123" });
  });

  it("hasActiveFilter is false when no params", () => {
    const { result } = renderHook(
      () => useEntityLinkParams(["vendorId"]),
      { wrapper: createWrapper("/test") },
    );

    expect(result.current.hasActiveFilter).toBe(false);
  });

  it("hasActiveFilter is true when params present", () => {
    const { result } = renderHook(
      () => useEntityLinkParams(["vendorId"]),
      { wrapper: createWrapper("/test?vendorId=123") },
    );

    expect(result.current.hasActiveFilter).toBe(true);
  });

  it("clearParams removes recognized params", () => {
    const { result } = renderHook(
      () => useEntityLinkParams(["vendorId", "categoryId"]),
      { wrapper: createWrapper("/test?vendorId=123&categoryId=456") },
    );

    expect(result.current.params).toEqual({
      vendorId: "123",
      categoryId: "456",
    });

    act(() => {
      result.current.clearParams();
    });

    expect(result.current.params).toEqual({});
    expect(result.current.hasActiveFilter).toBe(false);
  });
});

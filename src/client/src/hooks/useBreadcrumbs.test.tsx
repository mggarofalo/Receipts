import { renderHook } from "@testing-library/react";
import { MemoryRouter } from "react-router";
import { useBreadcrumbs } from "./useBreadcrumbs";
import type { ReactNode } from "react";

function createWrapper(route: string) {
  return function Wrapper({ children }: { children: ReactNode }) {
    return <MemoryRouter initialEntries={[route]}>{children}</MemoryRouter>;
  };
}

describe("useBreadcrumbs", () => {
  it("returns empty array for root path", () => {
    const { result } = renderHook(() => useBreadcrumbs(), {
      wrapper: createWrapper("/"),
    });
    expect(result.current).toEqual([]);
  });

  it("returns Home + page for known route", () => {
    const { result } = renderHook(() => useBreadcrumbs(), {
      wrapper: createWrapper("/accounts"),
    });
    expect(result.current).toEqual([
      { label: "Home", path: "/" },
      { label: "Accounts", path: "/accounts" },
    ]);
  });

  it("handles nested known route like /admin/users", () => {
    const { result } = renderHook(() => useBreadcrumbs(), {
      wrapper: createWrapper("/admin/users"),
    });
    expect(result.current).toEqual([
      { label: "Home", path: "/" },
      { label: "User Management", path: "/admin/users" },
    ]);
  });

  it("builds segments for unknown paths", () => {
    const { result } = renderHook(() => useBreadcrumbs(), {
      wrapper: createWrapper("/foo/bar"),
    });
    expect(result.current).toEqual([
      { label: "Home", path: "/" },
      { label: "foo", path: "/foo" },
      { label: "bar", path: "/foo/bar" },
    ]);
  });

  it("maps receipt-detail correctly", () => {
    const { result } = renderHook(() => useBreadcrumbs(), {
      wrapper: createWrapper("/receipt-detail"),
    });
    expect(result.current[1]).toEqual({
      label: "Receipt Detail",
      path: "/receipt-detail",
    });
  });
});

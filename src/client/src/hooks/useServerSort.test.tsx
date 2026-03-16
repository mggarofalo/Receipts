import { describe, it, expect } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { MemoryRouter } from "react-router";
import { useServerSort } from "./useServerSort";
import type { ReactNode } from "react";

function createWrapper(route: string = "/") {
  return function Wrapper({ children }: { children: ReactNode }) {
    return <MemoryRouter initialEntries={[route]}>{children}</MemoryRouter>;
  };
}

describe("useServerSort", () => {
  it("returns null sortBy and default asc direction with no options", () => {
    const { result } = renderHook(() => useServerSort(), {
      wrapper: createWrapper(),
    });
    expect(result.current.sortBy).toBeNull();
    expect(result.current.sortDirection).toBe("asc");
  });

  it("uses defaultSortBy when no search param", () => {
    const { result } = renderHook(
      () => useServerSort({ defaultSortBy: "name" }),
      { wrapper: createWrapper() },
    );
    expect(result.current.sortBy).toBe("name");
    expect(result.current.sortDirection).toBe("asc");
  });

  it("uses defaultSortDirection when no search param", () => {
    const { result } = renderHook(
      () => useServerSort({ defaultSortBy: "date", defaultSortDirection: "desc" }),
      { wrapper: createWrapper() },
    );
    expect(result.current.sortBy).toBe("date");
    expect(result.current.sortDirection).toBe("desc");
  });

  it("reads sortBy from search params", () => {
    const { result } = renderHook(() => useServerSort(), {
      wrapper: createWrapper("/?sortBy=amount"),
    });
    expect(result.current.sortBy).toBe("amount");
  });

  it("reads sortDirection from search params", () => {
    const { result } = renderHook(() => useServerSort(), {
      wrapper: createWrapper("/?sortBy=name&sortDirection=desc"),
    });
    expect(result.current.sortBy).toBe("name");
    expect(result.current.sortDirection).toBe("desc");
  });

  it("search params override defaults", () => {
    const { result } = renderHook(
      () => useServerSort({ defaultSortBy: "name", defaultSortDirection: "desc" }),
      { wrapper: createWrapper("/?sortBy=date&sortDirection=asc") },
    );
    expect(result.current.sortBy).toBe("date");
    expect(result.current.sortDirection).toBe("asc");
  });

  it("falls back to defaultSortDirection for invalid sortDirection param", () => {
    const { result } = renderHook(
      () => useServerSort({ defaultSortDirection: "desc" }),
      { wrapper: createWrapper("/?sortBy=name&sortDirection=invalid") },
    );
    expect(result.current.sortDirection).toBe("desc");
  });

  it("falls back to asc for invalid sortDirection when no default", () => {
    const { result } = renderHook(() => useServerSort(), {
      wrapper: createWrapper("/?sortBy=name&sortDirection=invalid"),
    });
    expect(result.current.sortDirection).toBe("asc");
  });

  it("toggleSort on same column flips direction from asc to desc", () => {
    const { result } = renderHook(() => useServerSort(), {
      wrapper: createWrapper("/?sortBy=name&sortDirection=asc"),
    });

    act(() => {
      result.current.toggleSort("name");
    });

    expect(result.current.sortBy).toBe("name");
    expect(result.current.sortDirection).toBe("desc");
  });

  it("toggleSort on same column flips direction from desc to asc", () => {
    const { result } = renderHook(() => useServerSort(), {
      wrapper: createWrapper("/?sortBy=name&sortDirection=desc"),
    });

    act(() => {
      result.current.toggleSort("name");
    });

    expect(result.current.sortBy).toBe("name");
    expect(result.current.sortDirection).toBe("asc");
  });

  it("toggleSort on different column sets new column with asc", () => {
    const { result } = renderHook(() => useServerSort(), {
      wrapper: createWrapper("/?sortBy=name&sortDirection=desc"),
    });

    act(() => {
      result.current.toggleSort("date");
    });

    expect(result.current.sortBy).toBe("date");
    expect(result.current.sortDirection).toBe("asc");
  });

  it("toggleSort from no sortBy sets column with asc", () => {
    const { result } = renderHook(() => useServerSort(), {
      wrapper: createWrapper(),
    });

    act(() => {
      result.current.toggleSort("amount");
    });

    expect(result.current.sortBy).toBe("amount");
    expect(result.current.sortDirection).toBe("asc");
  });
});

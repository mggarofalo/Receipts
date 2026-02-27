import { describe, it, expect } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { usePagination } from "./usePagination";

const items = Array.from({ length: 50 }, (_, i) => i + 1);

describe("usePagination", () => {
  it("returns first page by default", () => {
    const { result } = renderHook(() =>
      usePagination({ items, defaultPageSize: 10 }),
    );
    expect(result.current.currentPage).toBe(1);
    expect(result.current.paginatedItems).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
  });

  it("calculates total pages correctly", () => {
    const { result } = renderHook(() =>
      usePagination({ items, defaultPageSize: 10 }),
    );
    expect(result.current.totalPages).toBe(5);
    expect(result.current.totalItems).toBe(50);
  });

  it("navigates to a specific page", () => {
    const { result } = renderHook(() =>
      usePagination({ items, defaultPageSize: 10 }),
    );
    act(() => result.current.setPage(3));
    expect(result.current.currentPage).toBe(3);
    expect(result.current.paginatedItems).toEqual([21, 22, 23, 24, 25, 26, 27, 28, 29, 30]);
  });

  it("clamps page to minimum of 1", () => {
    const { result } = renderHook(() =>
      usePagination({ items, defaultPageSize: 10 }),
    );
    act(() => result.current.setPage(0));
    expect(result.current.currentPage).toBe(1);
  });

  it("clamps page to maximum", () => {
    const { result } = renderHook(() =>
      usePagination({ items, defaultPageSize: 10 }),
    );
    act(() => result.current.setPage(100));
    expect(result.current.currentPage).toBe(5);
  });

  it("resets to page 1 when page size changes", () => {
    const { result } = renderHook(() =>
      usePagination({ items, defaultPageSize: 10 }),
    );
    act(() => result.current.setPage(3));
    act(() => result.current.setPageSize(25));
    expect(result.current.currentPage).toBe(1);
    expect(result.current.pageSize).toBe(25);
    expect(result.current.totalPages).toBe(2);
  });

  it("uses default page size of 25", () => {
    const { result } = renderHook(() => usePagination({ items }));
    expect(result.current.pageSize).toBe(25);
    expect(result.current.totalPages).toBe(2);
  });

  it("handles empty items array", () => {
    const { result } = renderHook(() =>
      usePagination({ items: [], defaultPageSize: 10 }),
    );
    expect(result.current.totalPages).toBe(1);
    expect(result.current.paginatedItems).toEqual([]);
  });
});

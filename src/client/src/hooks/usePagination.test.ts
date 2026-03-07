import { describe, it, expect, beforeEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { usePagination } from "./usePagination";

const items = Array.from({ length: 50 }, (_, i) => i + 1);

describe("usePagination", () => {
  beforeEach(() => {
    localStorage.clear();
  });

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

  it("uses persisted page size from localStorage when available", () => {
    localStorage.setItem("table-page-size", "50");
    const { result } = renderHook(() =>
      usePagination({ items, defaultPageSize: 10 }),
    );
    expect(result.current.pageSize).toBe(50);
    expect(result.current.totalPages).toBe(1);
  });

  it("falls back to defaultPageSize when nothing is persisted", () => {
    const { result } = renderHook(() =>
      usePagination({ items, defaultPageSize: 10 }),
    );
    expect(result.current.pageSize).toBe(10);
  });

  it("persists new page size to localStorage via setPageSize", () => {
    const { result } = renderHook(() =>
      usePagination({ items, defaultPageSize: 10 }),
    );
    act(() => result.current.setPageSize(50));
    expect(localStorage.getItem("table-page-size")).toBe("50");
    expect(result.current.pageSize).toBe(50);
  });
});

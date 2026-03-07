import { describe, it, expect, beforeEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useServerPagination } from "./useServerPagination";

describe("useServerPagination", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("returns offset 0 and default page size on mount", () => {
    const { result } = renderHook(() => useServerPagination());
    expect(result.current.offset).toBe(0);
    expect(result.current.limit).toBe(25);
    expect(result.current.pageSize).toBe(25);
    expect(result.current.currentPage).toBe(1);
  });

  it("respects custom defaultPageSize", () => {
    const { result } = renderHook(() =>
      useServerPagination({ defaultPageSize: 20 }),
    );
    expect(result.current.limit).toBe(20);
  });

  it("navigates to a specific page", () => {
    const { result } = renderHook(() =>
      useServerPagination({ defaultPageSize: 10 }),
    );
    act(() => result.current.setPage(3, 100));
    expect(result.current.currentPage).toBe(3);
    expect(result.current.offset).toBe(20);
  });

  it("clamps page to minimum of 1", () => {
    const { result } = renderHook(() =>
      useServerPagination({ defaultPageSize: 10 }),
    );
    act(() => result.current.setPage(0, 100));
    expect(result.current.currentPage).toBe(1);
    expect(result.current.offset).toBe(0);
  });

  it("clamps page to maximum", () => {
    const { result } = renderHook(() =>
      useServerPagination({ defaultPageSize: 10 }),
    );
    act(() => result.current.setPage(999, 50));
    expect(result.current.currentPage).toBe(5);
    expect(result.current.offset).toBe(40);
  });

  it("calculates totalPages correctly", () => {
    const { result } = renderHook(() =>
      useServerPagination({ defaultPageSize: 10 }),
    );
    expect(result.current.totalPages(50)).toBe(5);
    expect(result.current.totalPages(51)).toBe(6);
    expect(result.current.totalPages(0)).toBe(1);
  });

  it("resets to offset 0 when page size changes", () => {
    const { result } = renderHook(() =>
      useServerPagination({ defaultPageSize: 10 }),
    );
    act(() => result.current.setPage(3, 100));
    expect(result.current.offset).toBe(20);

    act(() => result.current.setPageSize(25));
    expect(result.current.offset).toBe(0);
    expect(result.current.currentPage).toBe(1);
    expect(result.current.limit).toBe(25);
  });

  it("uses persisted page size from localStorage when available", () => {
    localStorage.setItem("table-page-size", "50");
    const { result } = renderHook(() =>
      useServerPagination({ defaultPageSize: 10 }),
    );
    expect(result.current.limit).toBe(50);
    expect(result.current.pageSize).toBe(50);
  });

  it("falls back to defaultPageSize when nothing is persisted", () => {
    const { result } = renderHook(() =>
      useServerPagination({ defaultPageSize: 10 }),
    );
    expect(result.current.limit).toBe(10);
  });

  it("persists new page size to localStorage via setPageSize", () => {
    const { result } = renderHook(() =>
      useServerPagination({ defaultPageSize: 10 }),
    );
    act(() => result.current.setPageSize(50));
    expect(localStorage.getItem("table-page-size")).toBe("50");
    expect(result.current.limit).toBe(50);
  });
});

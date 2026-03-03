import { renderHook, act } from "@testing-library/react";
import { useSavedFilters } from "./useSavedFilters";
import type { FilterPreset } from "@/lib/search";

describe("useSavedFilters", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  const preset: FilterPreset = {
    id: "f1",
    name: "Active Only",
    entityType: "account",
    values: { isActive: true },
    createdAt: "2024-01-01",
  };

  it("starts with empty filters for entity type", () => {
    const { result } = renderHook(() => useSavedFilters("account"));
    expect(result.current.filters).toEqual([]);
  });

  it("saves a filter and updates state", () => {
    const { result } = renderHook(() => useSavedFilters("account"));

    act(() => {
      result.current.save(preset);
    });

    expect(result.current.filters).toHaveLength(1);
    expect(result.current.filters[0].name).toBe("Active Only");
  });

  it("removes a filter by id", () => {
    const { result } = renderHook(() => useSavedFilters("account"));

    act(() => {
      result.current.save(preset);
    });
    expect(result.current.filters).toHaveLength(1);

    act(() => {
      result.current.remove("f1");
    });
    expect(result.current.filters).toEqual([]);
  });

  it("only returns filters for matching entity type", () => {
    const { result } = renderHook(() => useSavedFilters("receipt"));

    act(() => {
      result.current.save(preset); // entityType: "account"
    });

    // Should not appear since we're filtering for "receipt"
    expect(result.current.filters).toEqual([]);
  });
});

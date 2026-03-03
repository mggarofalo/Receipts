import { renderHook, act, waitFor } from "@testing-library/react";
import { useFuzzySearch } from "./useFuzzySearch";

const data = [
  { name: "Apple", category: "Fruit" },
  { name: "Banana", category: "Fruit" },
  { name: "Carrot", category: "Vegetable" },
];

const config = {
  keys: [{ name: "name" as const, weight: 1 }],
};

describe("useFuzzySearch", () => {
  it("returns all items when search is empty", () => {
    const { result } = renderHook(() =>
      useFuzzySearch({ data, config }),
    );
    expect(result.current.results).toHaveLength(3);
    expect(result.current.isSearching).toBe(false);
  });

  it("returns totalCount matching data length", () => {
    const { result } = renderHook(() =>
      useFuzzySearch({ data, config }),
    );
    expect(result.current.totalCount).toBe(3);
  });

  it("filters items after debounce", async () => {
    const { result } = renderHook(() =>
      useFuzzySearch({ data, config, debounceMs: 10 }),
    );

    act(() => {
      result.current.setSearch("Apple");
    });

    await waitFor(() => {
      expect(result.current.results.length).toBeLessThan(3);
    });

    expect(result.current.results[0].item.name).toBe("Apple");
  });

  it("clearSearch resets results", async () => {
    const { result } = renderHook(() =>
      useFuzzySearch({ data, config, debounceMs: 10 }),
    );

    act(() => {
      result.current.setSearch("Apple");
    });

    await waitFor(() => {
      expect(result.current.results.length).toBeLessThan(3);
    });

    act(() => {
      result.current.clearSearch();
    });

    expect(result.current.search).toBe("");
    expect(result.current.results).toHaveLength(3);
  });

  it("reports isSearching while debouncing", () => {
    const { result } = renderHook(() =>
      useFuzzySearch({ data, config, debounceMs: 500 }),
    );

    act(() => {
      result.current.setSearch("test");
    });

    expect(result.current.isSearching).toBe(true);
  });
});

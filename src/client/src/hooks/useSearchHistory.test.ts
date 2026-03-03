import { renderHook, act } from "@testing-library/react";
import { useSearchHistory } from "./useSearchHistory";

describe("useSearchHistory", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("starts with empty history", () => {
    const { result } = renderHook(() => useSearchHistory());
    expect(result.current.history).toEqual([]);
  });

  it("adds an entry and updates state", () => {
    const { result } = renderHook(() => useSearchHistory());

    act(() => {
      result.current.addEntry("receipt");
    });

    expect(result.current.history).toEqual(["receipt"]);
  });

  it("clears all history", () => {
    const { result } = renderHook(() => useSearchHistory());

    act(() => {
      result.current.addEntry("test");
    });
    expect(result.current.history).toHaveLength(1);

    act(() => {
      result.current.clearAll();
    });
    expect(result.current.history).toEqual([]);
  });

  it("deduplicates entries", () => {
    const { result } = renderHook(() => useSearchHistory());

    act(() => {
      result.current.addEntry("a");
      result.current.addEntry("b");
      result.current.addEntry("a");
    });

    expect(result.current.history).toEqual(["a", "b"]);
  });
});

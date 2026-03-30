import { renderHook, act } from "@testing-library/react";
import { useLocationHistory } from "./useLocationHistory";

vi.mock("@/hooks/useReceipts", () => ({
  useLocationSuggestions: vi.fn(() => ({ data: undefined })),
}));

describe("useLocationHistory", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("starts with empty locations and options", () => {
    const { result } = renderHook(() => useLocationHistory());
    expect(result.current.locations).toEqual([]);
    expect(result.current.options).toEqual([]);
  });

  it("adds a location and updates state", () => {
    const { result } = renderHook(() => useLocationHistory());

    act(() => {
      result.current.add("Walmart");
    });

    expect(result.current.locations).toEqual(["Walmart"]);
  });

  it("returns ComboboxOption objects in options", () => {
    const { result } = renderHook(() => useLocationHistory());

    act(() => {
      result.current.add("Target");
    });

    expect(result.current.options).toEqual([
      { value: "Target", label: "Target" },
    ]);
  });

  it("clears all locations", () => {
    const { result } = renderHook(() => useLocationHistory());

    act(() => {
      result.current.add("Walmart");
      result.current.add("Target");
    });
    expect(result.current.locations).toHaveLength(2);

    act(() => {
      result.current.clear();
    });
    expect(result.current.locations).toEqual([]);
    expect(result.current.options).toEqual([]);
  });

  it("deduplicates locations (case-insensitive)", () => {
    const { result } = renderHook(() => useLocationHistory());

    act(() => {
      result.current.add("Walmart");
      result.current.add("Target");
      result.current.add("walmart");
    });

    expect(result.current.locations).toEqual(["walmart", "Target"]);
  });
});

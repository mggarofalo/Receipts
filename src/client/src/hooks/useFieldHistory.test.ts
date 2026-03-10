import { renderHook, act } from "@testing-library/react";
import { createFieldHistory } from "@/lib/field-history";
import { useFieldHistory } from "./useFieldHistory";

describe("useFieldHistory", () => {
  const TEST_KEY = "test:use-field-history";

  beforeEach(() => {
    localStorage.clear();
  });

  it("starts with empty entries and options", () => {
    const fh = createFieldHistory(TEST_KEY);
    const { result } = renderHook(() => useFieldHistory(fh));
    expect(result.current.entries).toEqual([]);
    expect(result.current.options).toEqual([]);
  });

  it("adds an entry and updates state", () => {
    const fh = createFieldHistory(TEST_KEY);
    const { result } = renderHook(() => useFieldHistory(fh));

    act(() => {
      result.current.add("Alpha");
    });

    expect(result.current.entries).toEqual(["Alpha"]);
  });

  it("returns ComboboxOption objects in options", () => {
    const fh = createFieldHistory(TEST_KEY);
    const { result } = renderHook(() => useFieldHistory(fh));

    act(() => {
      result.current.add("Beta");
    });

    expect(result.current.options).toEqual([
      { value: "Beta", label: "Beta" },
    ]);
  });

  it("clears all entries", () => {
    const fh = createFieldHistory(TEST_KEY);
    const { result } = renderHook(() => useFieldHistory(fh));

    act(() => {
      result.current.add("Alpha");
      result.current.add("Beta");
    });
    expect(result.current.entries).toHaveLength(2);

    act(() => {
      result.current.clear();
    });
    expect(result.current.entries).toEqual([]);
    expect(result.current.options).toEqual([]);
  });

  it("deduplicates entries (case-insensitive)", () => {
    const fh = createFieldHistory(TEST_KEY);
    const { result } = renderHook(() => useFieldHistory(fh));

    act(() => {
      result.current.add("Alpha");
      result.current.add("Beta");
      result.current.add("alpha");
    });

    expect(result.current.entries).toEqual(["alpha", "Beta"]);
  });

  it("loads pre-existing entries from localStorage", () => {
    localStorage.setItem(TEST_KEY, JSON.stringify(["Pre-existing"]));
    const fh = createFieldHistory(TEST_KEY);
    const { result } = renderHook(() => useFieldHistory(fh));

    expect(result.current.entries).toEqual(["Pre-existing"]);
    expect(result.current.options).toEqual([
      { value: "Pre-existing", label: "Pre-existing" },
    ]);
  });
});

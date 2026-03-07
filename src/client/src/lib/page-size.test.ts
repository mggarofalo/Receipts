import { describe, it, expect, beforeEach, vi } from "vitest";
import { getPersistedPageSize, persistPageSize } from "./page-size";

describe("getPersistedPageSize", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("returns null when nothing is stored", () => {
    expect(getPersistedPageSize()).toBeNull();
  });

  it.each([10, 25, 50, 100])("returns %i when stored", (size) => {
    localStorage.setItem("table-page-size", String(size));
    expect(getPersistedPageSize()).toBe(size);
  });

  it.each(["abc", "15", "0", "-1", "999", ""])(
    'returns null for invalid stored value "%s"',
    (value) => {
      localStorage.setItem("table-page-size", value);
      expect(getPersistedPageSize()).toBeNull();
    },
  );

  it("returns null when localStorage.getItem throws", () => {
    vi.spyOn(Storage.prototype, "getItem").mockImplementation(() => {
      throw new Error("storage disabled");
    });
    expect(getPersistedPageSize()).toBeNull();
    vi.restoreAllMocks();
  });
});

describe("persistPageSize", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("writes the size to localStorage", () => {
    persistPageSize(50);
    expect(localStorage.getItem("table-page-size")).toBe("50");
  });

  it("overwrites a previously stored size", () => {
    persistPageSize(10);
    persistPageSize(100);
    expect(localStorage.getItem("table-page-size")).toBe("100");
  });

  it("does not write an invalid size", () => {
    persistPageSize(15);
    expect(localStorage.getItem("table-page-size")).toBeNull();
  });

  it("does not throw when localStorage.setItem throws", () => {
    vi.spyOn(Storage.prototype, "setItem").mockImplementation(() => {
      throw new Error("quota exceeded");
    });
    expect(() => persistPageSize(25)).not.toThrow();
    vi.restoreAllMocks();
  });
});

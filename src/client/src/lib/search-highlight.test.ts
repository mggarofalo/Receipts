import { getMatchIndices } from "./search-highlight";
import type { FuseResultMatch } from "fuse.js";

describe("getMatchIndices", () => {
  it("returns undefined when matches is undefined", () => {
    expect(getMatchIndices(undefined, "name")).toBeUndefined();
  });

  it("returns undefined when key is not found", () => {
    const matches: FuseResultMatch[] = [
      { key: "title", indices: [[0, 3]], value: "test" },
    ];
    expect(getMatchIndices(matches, "name")).toBeUndefined();
  });

  it("returns indices for matching key", () => {
    const matches: FuseResultMatch[] = [
      { key: "name", indices: [[0, 2], [5, 7]], value: "test" },
    ];
    expect(getMatchIndices(matches, "name")).toEqual([[0, 2], [5, 7]]);
  });

  it("returns indices from first matching entry", () => {
    const matches: FuseResultMatch[] = [
      { key: "name", indices: [[1, 3]], value: "a" },
      { key: "name", indices: [[4, 6]], value: "b" },
    ];
    expect(getMatchIndices(matches, "name")).toEqual([[1, 3]]);
  });

  it("handles empty matches array", () => {
    expect(getMatchIndices([], "name")).toBeUndefined();
  });
});

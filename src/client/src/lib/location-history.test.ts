import { describe, it, expect, beforeEach } from "vitest";
import {
  getLocationHistory,
  addLocation,
  clearLocationHistory,
} from "./location-history";

describe("location-history", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  describe("getLocationHistory", () => {
    it("returns empty array when no history exists", () => {
      expect(getLocationHistory()).toEqual([]);
    });

    it("returns stored locations", () => {
      localStorage.setItem(
        "receipts:location-history",
        JSON.stringify(["Walmart", "Target"]),
      );
      expect(getLocationHistory()).toEqual(["Walmart", "Target"]);
    });

    it("returns empty array on malformed JSON", () => {
      localStorage.setItem("receipts:location-history", "not-json");
      expect(getLocationHistory()).toEqual([]);
    });
  });

  describe("addLocation", () => {
    it("adds a location to the front", () => {
      addLocation("Walmart");
      addLocation("Target");
      expect(getLocationHistory()).toEqual(["Target", "Walmart"]);
    });

    it("moves duplicate to the front (case-insensitive)", () => {
      addLocation("Walmart");
      addLocation("Target");
      addLocation("walmart");
      expect(getLocationHistory()).toEqual(["walmart", "Target"]);
    });

    it("trims whitespace", () => {
      addLocation("  Costco  ");
      expect(getLocationHistory()).toEqual(["Costco"]);
    });

    it("ignores empty strings", () => {
      addLocation("");
      addLocation("   ");
      expect(getLocationHistory()).toEqual([]);
    });

    it("limits to 50 entries", () => {
      for (let i = 0; i < 60; i++) {
        addLocation(`Store ${i}`);
      }
      const history = getLocationHistory();
      expect(history).toHaveLength(50);
      expect(history[0]).toBe("Store 59");
    });

    it("does not crash when localStorage.setItem throws (quota exceeded)", () => {
      const original = localStorage.setItem.bind(localStorage);
      localStorage.setItem = () => {
        throw new DOMException("QuotaExceededError", "QuotaExceededError");
      };

      expect(() => addLocation("Walmart")).not.toThrow();

      localStorage.setItem = original;
    });
  });

  describe("getLocationHistory runtime validation", () => {
    it("filters out non-string entries from localStorage", () => {
      localStorage.setItem(
        "receipts:location-history",
        JSON.stringify(["Walmart", 42, null, "Target", { name: "bad" }]),
      );
      expect(getLocationHistory()).toEqual(["Walmart", "Target"]);
    });

    it("returns empty array when localStorage contains a non-array JSON value", () => {
      localStorage.setItem(
        "receipts:location-history",
        JSON.stringify({ not: "an array" }),
      );
      expect(getLocationHistory()).toEqual([]);
    });
  });

  describe("clearLocationHistory", () => {
    it("removes all locations", () => {
      addLocation("Walmart");
      addLocation("Target");
      clearLocationHistory();
      expect(getLocationHistory()).toEqual([]);
    });
  });
});

import { describe, it, expect, beforeEach } from "vitest";
import { createFieldHistory } from "./field-history";

describe("createFieldHistory", () => {
  const TEST_KEY = "test:field-history";

  beforeEach(() => {
    localStorage.clear();
  });

  describe("getHistory", () => {
    it("returns empty array when no history exists", () => {
      const fh = createFieldHistory(TEST_KEY);
      expect(fh.getHistory()).toEqual([]);
    });

    it("returns stored entries", () => {
      localStorage.setItem(TEST_KEY, JSON.stringify(["Alpha", "Beta"]));
      const fh = createFieldHistory(TEST_KEY);
      expect(fh.getHistory()).toEqual(["Alpha", "Beta"]);
    });

    it("returns empty array on malformed JSON", () => {
      localStorage.setItem(TEST_KEY, "not-json");
      const fh = createFieldHistory(TEST_KEY);
      expect(fh.getHistory()).toEqual([]);
    });

    it("filters out non-string entries from localStorage", () => {
      localStorage.setItem(
        TEST_KEY,
        JSON.stringify(["Alpha", 42, null, "Beta", { name: "bad" }]),
      );
      const fh = createFieldHistory(TEST_KEY);
      expect(fh.getHistory()).toEqual(["Alpha", "Beta"]);
    });

    it("returns empty array when localStorage contains a non-array JSON value", () => {
      localStorage.setItem(TEST_KEY, JSON.stringify({ not: "an array" }));
      const fh = createFieldHistory(TEST_KEY);
      expect(fh.getHistory()).toEqual([]);
    });
  });

  describe("addEntry", () => {
    it("adds an entry to the front", () => {
      const fh = createFieldHistory(TEST_KEY);
      fh.addEntry("Alpha");
      fh.addEntry("Beta");
      expect(fh.getHistory()).toEqual(["Beta", "Alpha"]);
    });

    it("moves duplicate to the front (case-insensitive)", () => {
      const fh = createFieldHistory(TEST_KEY);
      fh.addEntry("Alpha");
      fh.addEntry("Beta");
      fh.addEntry("alpha");
      expect(fh.getHistory()).toEqual(["alpha", "Beta"]);
    });

    it("trims whitespace", () => {
      const fh = createFieldHistory(TEST_KEY);
      fh.addEntry("  Gamma  ");
      expect(fh.getHistory()).toEqual(["Gamma"]);
    });

    it("ignores empty strings", () => {
      const fh = createFieldHistory(TEST_KEY);
      fh.addEntry("");
      fh.addEntry("   ");
      expect(fh.getHistory()).toEqual([]);
    });

    it("limits to default 50 entries", () => {
      const fh = createFieldHistory(TEST_KEY);
      for (let i = 0; i < 60; i++) {
        fh.addEntry(`Entry ${i}`);
      }
      const history = fh.getHistory();
      expect(history).toHaveLength(50);
      expect(history[0]).toBe("Entry 59");
    });

    it("respects custom maxEntries", () => {
      const fh = createFieldHistory(TEST_KEY, 5);
      for (let i = 0; i < 10; i++) {
        fh.addEntry(`Entry ${i}`);
      }
      const history = fh.getHistory();
      expect(history).toHaveLength(5);
      expect(history[0]).toBe("Entry 9");
      expect(history[4]).toBe("Entry 5");
    });

    it("does not crash when localStorage.setItem throws (quota exceeded)", () => {
      const fh = createFieldHistory(TEST_KEY);
      const original = localStorage.setItem.bind(localStorage);
      localStorage.setItem = () => {
        throw new DOMException("QuotaExceededError", "QuotaExceededError");
      };

      expect(() => fh.addEntry("Alpha")).not.toThrow();

      localStorage.setItem = original;
    });
  });

  describe("clearHistory", () => {
    it("removes all entries", () => {
      const fh = createFieldHistory(TEST_KEY);
      fh.addEntry("Alpha");
      fh.addEntry("Beta");
      fh.clearHistory();
      expect(fh.getHistory()).toEqual([]);
    });
  });

  describe("key property", () => {
    it("exposes the localStorage key", () => {
      const fh = createFieldHistory(TEST_KEY);
      expect(fh.key).toBe(TEST_KEY);
    });
  });

  describe("multiple instances with different keys", () => {
    it("stores entries independently", () => {
      const fh1 = createFieldHistory("test:key-1");
      const fh2 = createFieldHistory("test:key-2");

      fh1.addEntry("From Key 1");
      fh2.addEntry("From Key 2");

      expect(fh1.getHistory()).toEqual(["From Key 1"]);
      expect(fh2.getHistory()).toEqual(["From Key 2"]);
    });
  });
});

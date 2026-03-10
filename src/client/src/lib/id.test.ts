import { describe, it, expect } from "vitest";
import { generateId } from "./id";

const UUID_V4_REGEX =
  /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/;

describe("generateId", () => {
  it("returns a valid UUID v4 string", () => {
    const id = generateId();
    expect(id).toMatch(UUID_V4_REGEX);
  });

  it("generates unique IDs on each call", () => {
    const ids = new Set(Array.from({ length: 100 }, () => generateId()));
    expect(ids.size).toBe(100);
  });

  it("produces a valid UUID v4 via the fallback path", () => {
    const original = crypto.randomUUID;
    try {
      // Force the fallback by removing randomUUID
      Object.defineProperty(crypto, "randomUUID", {
        value: undefined,
        configurable: true,
      });
      const id = generateId();
      expect(id).toMatch(UUID_V4_REGEX);
    } finally {
      Object.defineProperty(crypto, "randomUUID", {
        value: original,
        configurable: true,
      });
    }
  });

  it("generates unique IDs via the fallback path", () => {
    const original = crypto.randomUUID;
    try {
      Object.defineProperty(crypto, "randomUUID", {
        value: undefined,
        configurable: true,
      });
      const ids = new Set(Array.from({ length: 100 }, () => generateId()));
      expect(ids.size).toBe(100);
    } finally {
      Object.defineProperty(crypto, "randomUUID", {
        value: original,
        configurable: true,
      });
    }
  });
});

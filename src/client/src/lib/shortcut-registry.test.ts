import { shortcuts, getShortcutsByCategory } from "./shortcut-registry";

describe("shortcuts", () => {
  it("is a non-empty array", () => {
    expect(shortcuts.length).toBeGreaterThan(0);
  });

  it("every shortcut has keys, label, and category", () => {
    for (const s of shortcuts) {
      expect(s.keys).toBeTruthy();
      expect(s.label).toBeTruthy();
      expect(["Global", "List Navigation", "Forms"]).toContain(s.category);
    }
  });

  it("contains the ? shortcut for help", () => {
    expect(shortcuts.find((s) => s.keys === "?")).toBeDefined();
  });
});

describe("getShortcutsByCategory", () => {
  it("returns a Map with all categories", () => {
    const map = getShortcutsByCategory();
    expect(map.has("Global")).toBe(true);
    expect(map.has("List Navigation")).toBe(true);
    expect(map.has("Forms")).toBe(true);
  });

  it("groups shortcuts correctly", () => {
    const map = getShortcutsByCategory();
    const global = map.get("Global")!;
    expect(global.every((s) => s.category === "Global")).toBe(true);
  });

  it("total items across categories equals shortcuts length", () => {
    const map = getShortcutsByCategory();
    let total = 0;
    for (const items of map.values()) {
      total += items.length;
    }
    expect(total).toBe(shortcuts.length);
  });
});

import {
  createFuseInstance,
  getSearchHistory,
  addSearchHistoryEntry,
  clearSearchHistory,
  getSavedFilters,
  saveFilterPreset,
  deleteFilterPreset,
  applyFilters,
  type FilterPreset,
  type FilterDefinition,
} from "./search";

beforeEach(() => {
  localStorage.clear();
});

describe("createFuseInstance", () => {
  it("creates a Fuse instance that searches data", () => {
    const data = [{ name: "Apple" }, { name: "Banana" }, { name: "Cherry" }];
    const fuse = createFuseInstance(data, {
      keys: [{ name: "name", weight: 1 }],
    });
    const results = fuse.search("Apple");
    expect(results.length).toBeGreaterThan(0);
    expect(results[0].item.name).toBe("Apple");
  });

  it("respects custom threshold", () => {
    const data = [{ name: "Apple" }];
    const strict = createFuseInstance(data, {
      keys: [{ name: "name", weight: 1 }],
      threshold: 0,
    });
    expect(strict.search("Appel")).toHaveLength(0);
  });
});

describe("search history", () => {
  it("returns empty array when no history", () => {
    expect(getSearchHistory()).toEqual([]);
  });

  it("adds entries and retrieves them", () => {
    addSearchHistoryEntry("receipt");
    addSearchHistoryEntry("account");
    const history = getSearchHistory();
    expect(history).toEqual(["account", "receipt"]);
  });

  it("deduplicates entries (moves to front)", () => {
    addSearchHistoryEntry("a");
    addSearchHistoryEntry("b");
    addSearchHistoryEntry("a");
    expect(getSearchHistory()).toEqual(["a", "b"]);
  });

  it("limits to 20 entries", () => {
    for (let i = 0; i < 25; i++) {
      addSearchHistoryEntry(`term-${i}`);
    }
    expect(getSearchHistory()).toHaveLength(20);
  });

  it("clears history", () => {
    addSearchHistoryEntry("test");
    clearSearchHistory();
    expect(getSearchHistory()).toEqual([]);
  });

  it("handles corrupt localStorage gracefully", () => {
    localStorage.setItem("receipts:search-history", "not-json");
    expect(getSearchHistory()).toEqual([]);
  });
});

describe("saved filters", () => {
  const preset: FilterPreset = {
    id: "f1",
    name: "Active",
    entityType: "account",
    values: { isActive: true },
    createdAt: "2024-01-01",
  };

  it("returns empty array when no filters saved", () => {
    expect(getSavedFilters()).toEqual([]);
  });

  it("saves and retrieves filters", () => {
    saveFilterPreset(preset);
    expect(getSavedFilters()).toEqual([preset]);
  });

  it("filters by entityType", () => {
    saveFilterPreset(preset);
    saveFilterPreset({ ...preset, id: "f2", entityType: "receipt" });
    expect(getSavedFilters("account")).toHaveLength(1);
    expect(getSavedFilters("receipt")).toHaveLength(1);
  });

  it("updates existing filter by id", () => {
    saveFilterPreset(preset);
    saveFilterPreset({ ...preset, name: "Updated" });
    const all = getSavedFilters();
    expect(all).toHaveLength(1);
    expect(all[0].name).toBe("Updated");
  });

  it("deletes filter by id", () => {
    saveFilterPreset(preset);
    deleteFilterPreset("f1");
    expect(getSavedFilters()).toEqual([]);
  });

  it("handles corrupt localStorage gracefully", () => {
    localStorage.setItem("receipts:saved-filters", "{bad}");
    expect(getSavedFilters()).toEqual([]);
  });
});

describe("applyFilters", () => {
  const items = [
    { name: "A", category: "food", price: 10, active: true, date: "2024-01-15" },
    { name: "B", category: "drink", price: 20, active: false, date: "2024-02-15" },
    { name: "C", category: "food", price: 30, active: true, date: "2024-03-15" },
  ];

  it("filters by select type", () => {
    const filters: FilterDefinition[] = [
      { key: "cat", type: "select", field: "category" },
    ];
    const result = applyFilters(items, filters, { cat: "food" });
    expect(result).toHaveLength(2);
  });

  it("ignores select filter when value is 'all'", () => {
    const filters: FilterDefinition[] = [
      { key: "cat", type: "select", field: "category" },
    ];
    const result = applyFilters(items, filters, { cat: "all" });
    expect(result).toHaveLength(3);
  });

  it("filters by boolean type", () => {
    const filters: FilterDefinition[] = [
      { key: "isActive", type: "boolean", field: "active" },
    ];
    const result = applyFilters(items, filters, { isActive: "true" });
    expect(result).toHaveLength(2);
  });

  it("filters by dateRange", () => {
    const filters: FilterDefinition[] = [
      { key: "dateRange", type: "dateRange", field: "date" },
    ];
    const result = applyFilters(items, filters, {
      dateRange: { from: "2024-02-01", to: "2024-02-28" },
    });
    expect(result).toHaveLength(1);
  });

  it("filters by numberRange", () => {
    const filters: FilterDefinition[] = [
      { key: "priceRange", type: "numberRange", field: "price" },
    ];
    const result = applyFilters(items, filters, {
      priceRange: { min: 15, max: 25 },
    });
    expect(result).toHaveLength(1);
  });

  it("returns all items when no filter values set", () => {
    const filters: FilterDefinition[] = [
      { key: "cat", type: "select", field: "category" },
    ];
    const result = applyFilters(items, filters, {});
    expect(result).toHaveLength(3);
  });
});

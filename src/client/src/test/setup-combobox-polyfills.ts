/**
 * Polyfills for ResizeObserver and scrollIntoView required by radix-ui / cmdk
 * in jsdom test environments. Import this file in any test that renders a
 * Combobox (or any radix Popover / Command-based component).
 *
 * Usage: import "@/test/setup-combobox-polyfills";
 */

beforeAll(() => {
  if (typeof window.ResizeObserver === "undefined") {
    window.ResizeObserver = class {
      observe() {}
      unobserve() {}
      disconnect() {}
    } as unknown as typeof ResizeObserver;
  }

  if (!Element.prototype.scrollIntoView) {
    Element.prototype.scrollIntoView = vi.fn();
  }
});

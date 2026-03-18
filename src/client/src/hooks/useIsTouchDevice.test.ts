import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useIsTouchDevice } from "./useIsTouchDevice";

type ChangeListener = (event: { matches: boolean }) => void;

function createMockMatchMedia(initialMatches: boolean) {
  const listeners: ChangeListener[] = [];
  let matches = initialMatches;

  const mql = {
    get matches() {
      return matches;
    },
    media: "(pointer: coarse)",
    addEventListener: vi.fn((_event: string, cb: ChangeListener) => {
      listeners.push(cb);
    }),
    removeEventListener: vi.fn((_event: string, cb: ChangeListener) => {
      const idx = listeners.indexOf(cb);
      if (idx >= 0) listeners.splice(idx, 1);
    }),
    // Not used but required by MediaQueryList interface
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    dispatchEvent: vi.fn(),
  };

  return {
    mql,
    listeners,
    setMatches(value: boolean) {
      matches = value;
      for (const cb of listeners) cb({ matches: value });
    },
    matchMedia: vi.fn(() => mql),
  };
}

describe("useIsTouchDevice", () => {
  let originalMatchMedia: typeof window.matchMedia;

  beforeEach(() => {
    originalMatchMedia = window.matchMedia;
  });

  afterEach(() => {
    window.matchMedia = originalMatchMedia;
  });

  it("returns false when pointer is not coarse", () => {
    const mock = createMockMatchMedia(false);
    window.matchMedia = mock.matchMedia as unknown as typeof window.matchMedia;

    const { result } = renderHook(() => useIsTouchDevice());
    expect(result.current).toBe(false);
  });

  it("returns true when pointer is coarse", () => {
    const mock = createMockMatchMedia(true);
    window.matchMedia = mock.matchMedia as unknown as typeof window.matchMedia;

    const { result } = renderHook(() => useIsTouchDevice());
    expect(result.current).toBe(true);
  });

  it("updates when the media query changes", () => {
    const mock = createMockMatchMedia(false);
    window.matchMedia = mock.matchMedia as unknown as typeof window.matchMedia;

    const { result } = renderHook(() => useIsTouchDevice());
    expect(result.current).toBe(false);

    act(() => {
      mock.setMatches(true);
    });
    expect(result.current).toBe(true);

    act(() => {
      mock.setMatches(false);
    });
    expect(result.current).toBe(false);
  });

  it("cleans up the listener on unmount", () => {
    const mock = createMockMatchMedia(false);
    window.matchMedia = mock.matchMedia as unknown as typeof window.matchMedia;

    const { unmount } = renderHook(() => useIsTouchDevice());
    expect(mock.mql.addEventListener).toHaveBeenCalledWith(
      "change",
      expect.any(Function),
    );

    unmount();
    expect(mock.mql.removeEventListener).toHaveBeenCalledWith(
      "change",
      expect.any(Function),
    );
  });
});

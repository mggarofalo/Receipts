import { act, renderHook } from "@testing-library/react";
import { MemoryRouter, useLocation } from "react-router";
import { describe, expect, it, vi } from "vitest";
import type { ReactNode } from "react";
import { useOpenNewItem } from "./useOpenNewItem";

type InitialEntry = string | { pathname: string; state?: unknown };

function createWrapper(initialEntry: InitialEntry = "/test") {
  return function Wrapper({ children }: { children: ReactNode }) {
    return (
      <MemoryRouter initialEntries={[initialEntry]}>{children}</MemoryRouter>
    );
  };
}

describe("useOpenNewItem", () => {
  it("calls open when the shortcut:new-item event fires", () => {
    const open = vi.fn();
    renderHook(() => useOpenNewItem(open), { wrapper: createWrapper() });

    act(() => {
      window.dispatchEvent(new CustomEvent("shortcut:new-item"));
    });

    expect(open).toHaveBeenCalledTimes(1);
  });

  it("removes the listener on unmount", () => {
    const open = vi.fn();
    const { unmount } = renderHook(() => useOpenNewItem(open), {
      wrapper: createWrapper(),
    });

    unmount();

    window.dispatchEvent(new CustomEvent("shortcut:new-item"));
    expect(open).not.toHaveBeenCalled();
  });

  it("resubscribes when the open callback changes", () => {
    const first = vi.fn();
    const second = vi.fn();
    const { rerender } = renderHook(
      ({ open }: { open: () => void }) => useOpenNewItem(open),
      { wrapper: createWrapper(), initialProps: { open: first } },
    );

    rerender({ open: second });

    act(() => {
      window.dispatchEvent(new CustomEvent("shortcut:new-item"));
    });

    expect(first).not.toHaveBeenCalled();
    expect(second).toHaveBeenCalledTimes(1);
  });

  it("opens when mounted with openNew navigation state", () => {
    const open = vi.fn();
    renderHook(() => useOpenNewItem(open), {
      wrapper: createWrapper({
        pathname: "/accounts",
        state: { openNew: true },
      }),
    });

    expect(open).toHaveBeenCalledTimes(1);
  });

  it("clears navigation state after consuming it", () => {
    const open = vi.fn();

    function HookAndLocation() {
      useOpenNewItem(open);
      const location = useLocation();
      return location.state as { openNew?: boolean } | null;
    }

    const { result } = renderHook(() => HookAndLocation(), {
      wrapper: createWrapper({
        pathname: "/accounts",
        state: { openNew: true },
      }),
    });

    expect(open).toHaveBeenCalledTimes(1);
    expect(result.current).toBeNull();
  });

  it("preserves pathname, search, and hash when clearing state", () => {
    const open = vi.fn();

    function HookAndLocation() {
      useOpenNewItem(open);
      const location = useLocation();
      return location;
    }

    const { result } = renderHook(() => HookAndLocation(), {
      wrapper: createWrapper({
        pathname: "/accounts",
        search: "?foo=bar",
        hash: "#section",
        state: { openNew: true },
      } as unknown as Parameters<typeof createWrapper>[0]),
    });

    expect(open).toHaveBeenCalledTimes(1);
    expect(result.current.pathname).toBe("/accounts");
    expect(result.current.search).toBe("?foo=bar");
    expect(result.current.hash).toBe("#section");
    expect(result.current.state).toBeNull();
  });

  it("does nothing when mounted without openNew state", () => {
    const open = vi.fn();
    renderHook(() => useOpenNewItem(open), {
      wrapper: createWrapper("/accounts"),
    });

    expect(open).not.toHaveBeenCalled();
  });

  it("does not re-trigger on subsequent renders once state is cleared", () => {
    const open = vi.fn();
    const { rerender } = renderHook(() => useOpenNewItem(open), {
      wrapper: createWrapper({
        pathname: "/accounts",
        state: { openNew: true },
      }),
    });

    rerender();
    rerender();

    expect(open).toHaveBeenCalledTimes(1);
  });
});

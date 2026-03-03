import { renderHook } from "@testing-library/react";
import { useFormShortcuts } from "./useFormShortcuts";
import type { RefObject } from "react";

vi.mock("react-hotkeys-hook", () => ({
  useHotkeys: vi.fn((key: string, callback: () => void) => {
    // Simulate the hotkey by storing callback for testing
    document.addEventListener("keydown", (e) => {
      if (e.key === "Enter" && (e.ctrlKey || e.metaKey)) {
        callback();
      }
    });
  }),
}));

describe("useFormShortcuts", () => {
  it("calls requestSubmit on Ctrl+Enter", () => {
    const requestSubmit = vi.fn();
    const formRef = {
      current: { requestSubmit } as unknown as HTMLFormElement,
    } as RefObject<HTMLFormElement | null>;

    renderHook(() => useFormShortcuts({ formRef }));

    document.dispatchEvent(
      new KeyboardEvent("keydown", { key: "Enter", ctrlKey: true }),
    );
    expect(requestSubmit).toHaveBeenCalled();
  });

  it("does not error when formRef.current is null", () => {
    const formRef = { current: null } as RefObject<HTMLFormElement | null>;

    renderHook(() => useFormShortcuts({ formRef }));

    expect(() => {
      document.dispatchEvent(
        new KeyboardEvent("keydown", { key: "Enter", ctrlKey: true }),
      );
    }).not.toThrow();
  });
});

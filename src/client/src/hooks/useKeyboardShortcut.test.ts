import { renderHook } from "@testing-library/react";
import { useKeyboardShortcut } from "./useKeyboardShortcut";

describe("useKeyboardShortcut", () => {
  it("fires handler on matching key with ctrl", () => {
    const handler = vi.fn();
    renderHook(() =>
      useKeyboardShortcut({ key: "s", handler }),
    );

    document.dispatchEvent(
      new KeyboardEvent("keydown", { key: "s", ctrlKey: true }),
    );
    expect(handler).toHaveBeenCalledTimes(1);
  });

  it("fires handler on matching key with meta", () => {
    const handler = vi.fn();
    renderHook(() =>
      useKeyboardShortcut({ key: "s", handler }),
    );

    document.dispatchEvent(
      new KeyboardEvent("keydown", { key: "s", metaKey: true }),
    );
    expect(handler).toHaveBeenCalledTimes(1);
  });

  it("does not fire when ctrlOrMeta is required but not pressed", () => {
    const handler = vi.fn();
    renderHook(() =>
      useKeyboardShortcut({ key: "s", handler }),
    );

    document.dispatchEvent(new KeyboardEvent("keydown", { key: "s" }));
    expect(handler).not.toHaveBeenCalled();
  });

  it("fires without modifier when ctrlOrMeta is false", () => {
    const handler = vi.fn();
    renderHook(() =>
      useKeyboardShortcut({ key: "?", ctrlOrMeta: false, handler }),
    );

    document.dispatchEvent(new KeyboardEvent("keydown", { key: "?" }));
    expect(handler).toHaveBeenCalledTimes(1);
  });

  it("does not fire when enabled is false", () => {
    const handler = vi.fn();
    renderHook(() =>
      useKeyboardShortcut({ key: "s", handler, enabled: false }),
    );

    document.dispatchEvent(
      new KeyboardEvent("keydown", { key: "s", ctrlKey: true }),
    );
    expect(handler).not.toHaveBeenCalled();
  });

  it("does not fire when a dialog is open", () => {
    const dialog = document.createElement("div");
    dialog.setAttribute("role", "dialog");
    document.body.appendChild(dialog);

    const handler = vi.fn();
    renderHook(() =>
      useKeyboardShortcut({ key: "s", handler }),
    );

    document.dispatchEvent(
      new KeyboardEvent("keydown", { key: "s", ctrlKey: true }),
    );
    expect(handler).not.toHaveBeenCalled();

    document.body.removeChild(dialog);
  });

  it("does not fire on form elements when enableOnFormTags is false", () => {
    const input = document.createElement("input");
    document.body.appendChild(input);
    input.focus();

    const handler = vi.fn();
    renderHook(() =>
      useKeyboardShortcut({
        key: "k",
        ctrlOrMeta: false,
        handler,
        enableOnFormTags: false,
      }),
    );

    document.dispatchEvent(new KeyboardEvent("keydown", { key: "k" }));
    expect(handler).not.toHaveBeenCalled();

    document.body.removeChild(input);
  });
});

import { useCallback, useContext } from "react";
import { useHotkeys } from "react-hotkeys-hook";
import { ShortcutsContext } from "@/contexts/shortcuts-context";
import { useKeyboardShortcut } from "@/hooks/useKeyboardShortcut";

export function useGlobalShortcuts() {
  const ctx = useContext(ShortcutsContext);

  // ? — Toggle help modal (useKeyboardShortcut because react-hotkeys-hook
  // doesn't match the "?" key)
  const toggleHelp = useCallback(
    () => ctx?.setHelpOpen(!ctx.helpOpen),
    [ctx],
  );
  useKeyboardShortcut({
    key: "?",
    ctrlOrMeta: false,
    handler: toggleHelp,
    enableOnFormTags: false,
  });

  // Ctrl+K is handled by GlobalSearchDialog via useKeyboardShortcut

  // Shift+N — Dispatch new-item event
  useHotkeys(
    "shift+n",
    () => {
      window.dispatchEvent(new CustomEvent("shortcut:new-item"));
    },
    { enableOnFormTags: false, preventDefault: true },
  );
}

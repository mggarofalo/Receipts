import { useContext } from "react";
import { useHotkeys } from "react-hotkeys-hook";
import { ShortcutsContext } from "@/contexts/shortcuts-context";

export function useGlobalShortcuts() {
  const ctx = useContext(ShortcutsContext);

  // ? — Toggle help modal
  useHotkeys(
    "shift+/",
    () => ctx?.setHelpOpen(!ctx.helpOpen),
    { enableOnFormTags: false, preventDefault: true },
    [ctx?.helpOpen],
  );

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

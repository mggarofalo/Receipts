import { useHotkeys } from "react-hotkeys-hook";

export interface ShortcutEntry {
  keys: string;
  description: string;
  scope?: string;
}

// Central registry — single source of truth for ShortcutsHelp modal
export const SHORTCUTS: ShortcutEntry[] = [
  { keys: "ctrl+k, meta+k", description: "Open global search", scope: "Global" },
  { keys: "ctrl+n, meta+n", description: "Create new item", scope: "Global" },
  { keys: "?", description: "Show keyboard shortcuts", scope: "Global" },
  { keys: "Esc", description: "Close dialog / cancel", scope: "Global" },
  { keys: "j", description: "Move selection down", scope: "List" },
  { keys: "k", description: "Move selection up", scope: "List" },
  { keys: "Enter", description: "Open selected item", scope: "List" },
  { keys: "ctrl+a, meta+a", description: "Select all rows", scope: "List" },
  { keys: "ctrl+Enter, meta+Enter", description: "Submit form", scope: "Form" },
];

interface UseKeyboardShortcutOptions {
  onSearch?: () => void;
  onNew?: () => void;
  onHelp?: () => void;
}

export function useKeyboardShortcuts({
  onSearch,
  onNew,
  onHelp,
}: UseKeyboardShortcutOptions = {}) {
  useHotkeys(
    "ctrl+k,meta+k",
    (e) => {
      e.preventDefault();
      onSearch?.();
    },
    { enableOnFormElements: false },
  );

  useHotkeys(
    "ctrl+n,meta+n",
    (e) => {
      e.preventDefault();
      onNew?.();
    },
    { enableOnFormElements: false },
  );

  useHotkeys(
    "shift+slash",
    (e) => {
      e.preventDefault();
      onHelp?.();
    },
    { enableOnFormElements: false },
  );
}

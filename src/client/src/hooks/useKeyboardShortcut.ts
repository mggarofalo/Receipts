import { useEffect } from "react";

interface UseKeyboardShortcutOptions {
  key: string;
  ctrlOrMeta?: boolean;
  handler: () => void;
  enabled?: boolean;
}

export function useKeyboardShortcut({
  key,
  ctrlOrMeta = true,
  handler,
  enabled = true,
}: UseKeyboardShortcutOptions) {
  useEffect(() => {
    if (!enabled) return;

    function onKeyDown(e: KeyboardEvent) {
      const modifierOk = ctrlOrMeta ? e.metaKey || e.ctrlKey : true;
      if (modifierOk && e.key.toLowerCase() === key.toLowerCase()) {
        e.preventDefault();
        handler();
      }
    }

    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, [key, ctrlOrMeta, handler, enabled]);
}

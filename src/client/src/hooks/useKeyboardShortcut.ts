import { useEffect } from "react";

const FORM_TAGS = new Set(["INPUT", "TEXTAREA", "SELECT"]);

interface UseKeyboardShortcutOptions {
  key: string;
  ctrlOrMeta?: boolean;
  handler: () => void;
  enabled?: boolean;
  enableOnFormTags?: boolean;
}

export function useKeyboardShortcut({
  key,
  ctrlOrMeta = true,
  handler,
  enabled = true,
  enableOnFormTags = true,
}: UseKeyboardShortcutOptions) {
  useEffect(() => {
    if (!enabled) return;

    function onKeyDown(e: KeyboardEvent) {
      if (document.querySelector("[role=dialog]")) return;
      if (
        !enableOnFormTags &&
        FORM_TAGS.has((document.activeElement?.tagName ?? "").toUpperCase())
      ) {
        return;
      }
      const modifierOk = ctrlOrMeta ? e.metaKey || e.ctrlKey : true;
      if (modifierOk && e.key.toLowerCase() === key.toLowerCase()) {
        e.preventDefault();
        handler();
      }
    }

    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, [key, ctrlOrMeta, handler, enabled, enableOnFormTags]);
}

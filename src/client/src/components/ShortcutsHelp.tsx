import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { SHORTCUTS } from "@/hooks/useKeyboardShortcuts";
import { useHotkeys } from "react-hotkeys-hook";

function formatKeys(keys: string): string {
  return keys
    .split(",")
    .map((combo) =>
      combo
        .trim()
        .replace("ctrl", "Ctrl")
        .replace("meta", "⌘")
        .replace("shift+slash", "?")
        .replace("+", " + "),
    )
    .join(" / ");
}

// Group shortcuts by scope
const grouped = SHORTCUTS.reduce<Record<string, typeof SHORTCUTS>>(
  (acc, shortcut) => {
    const scope = shortcut.scope ?? "Global";
    (acc[scope] ??= []).push(shortcut);
    return acc;
  },
  {},
);

export function ShortcutsHelp() {
  const [open, setOpen] = useState(false);

  useHotkeys("shift+slash", (e) => {
    e.preventDefault();
    setOpen((prev) => !prev);
  });

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogContent className="max-w-md" aria-describedby={undefined}>
        <DialogHeader>
          <DialogTitle>Keyboard Shortcuts</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          {Object.entries(grouped).map(([scope, shortcuts]) => (
            <div key={scope}>
              <h3 className="mb-2 text-sm font-semibold text-muted-foreground">
                {scope}
              </h3>
              <table className="w-full text-sm">
                <tbody>
                  {shortcuts.map((s) => (
                    <tr key={s.keys} className="border-b last:border-0">
                      <td className="py-1.5 pr-4">
                        <kbd className="rounded bg-muted px-1.5 py-0.5 font-mono text-xs">
                          {formatKeys(s.keys)}
                        </kbd>
                      </td>
                      <td className="py-1.5 text-muted-foreground">
                        {s.description}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ))}
        </div>
        <p className="mt-2 text-xs text-muted-foreground">
          Press <kbd className="rounded bg-muted px-1 py-0.5 font-mono">?</kbd> to
          toggle this dialog
        </p>
      </DialogContent>
    </Dialog>
  );
}

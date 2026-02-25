import { useState, type ReactNode } from "react";
import { HotkeysProvider } from "react-hotkeys-hook";
import { ShortcutsContext } from "./shortcuts-context";

export function ShortcutsProvider({ children }: { children: ReactNode }) {
  const [helpOpen, setHelpOpen] = useState(false);

  return (
    <HotkeysProvider>
      <ShortcutsContext.Provider value={{ helpOpen, setHelpOpen }}>
        {children}
      </ShortcutsContext.Provider>
    </HotkeysProvider>
  );
}

import { createContext } from "react";

export interface ShortcutsContextValue {
  helpOpen: boolean;
  setHelpOpen: (open: boolean) => void;
}

export const ShortcutsContext = createContext<ShortcutsContextValue | null>(
  null,
);

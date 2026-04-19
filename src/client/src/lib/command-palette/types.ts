import type { ComponentType, SVGProps } from "react";
import type { NavigateFunction } from "react-router";

export type CommandGroupId =
  | "create"
  | "navigate"
  | "reports"
  | "preferences";

export interface CommandContext {
  navigate: NavigateFunction;
  close: () => void;
  currentPath: string;
  setTheme: (theme: "light" | "dark" | "system") => void;
  logout: () => Promise<void>;
  openShortcutsHelp: () => void;
}

export interface Command {
  id: string;
  label: string;
  group: CommandGroupId;
  icon: ComponentType<SVGProps<SVGSVGElement>>;
  keywords?: string[];
  shortcut?: string;
  requiresAdmin?: boolean;
  /** Path compared against `currentPath` to decide whether the ⇧N hint applies. */
  targetPath?: string;
  run: (ctx: CommandContext) => void | Promise<void>;
}

export const COMMAND_GROUP_LABELS: Record<CommandGroupId, string> = {
  create: "Create",
  navigate: "Go to",
  reports: "Reports",
  preferences: "Preferences",
};

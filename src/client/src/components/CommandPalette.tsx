import { Fragment, useCallback, useContext, useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router";
import { ChevronDown } from "lucide-react";
import { useTheme } from "next-themes";
import {
  CommandDialog,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
  CommandSeparator,
  CommandShortcut,
} from "@/components/ui/command";
import { useAuth } from "@/hooks/useAuth";
import { usePermission } from "@/hooks/usePermission";
import { ShortcutsContext } from "@/contexts/shortcuts-context";
import { COMMANDS } from "@/lib/command-palette/commands";
import {
  COMMAND_GROUP_LABELS,
  type Command,
  type CommandContext,
  type CommandGroupId,
} from "@/lib/command-palette/types";
import { useEntityResults } from "@/lib/command-palette/entity-results";

const ENTITY_GROUP_VISIBLE_LIMIT = 8;

interface CommandPaletteProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

const GROUP_ORDER: CommandGroupId[] = [
  "create",
  "navigate",
  "reports",
  "preferences",
];

function commandSearchValue(cmd: Command): string {
  return [cmd.id, cmd.label, ...(cmd.keywords ?? [])].join(" ").toLowerCase();
}

/** Spell out keyboard-shortcut glyphs so screen readers don't read them as symbols. */
function spokenShortcut(shortcut: string): string {
  return shortcut
    .replace(/⇧\s*/g, "Shift+")
    .replace(/⌘\s*/g, "Command+")
    .replace(/⌃\s*/g, "Control+")
    .replace(/⌥\s*/g, "Option+")
    .replace(/↵/g, "Enter")
    .trim();
}

export function CommandPalette({ open, onOpenChange }: CommandPaletteProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const { setTheme } = useTheme();
  const { logout } = useAuth();
  const { isAdmin } = usePermission();
  const shortcutsCtx = useContext(ShortcutsContext);
  const [query, setQuery] = useState("");
  const [expandedGroups, setExpandedGroups] = useState<Set<string>>(new Set());

  const admin = isAdmin();
  const close = useCallback(() => onOpenChange(false), [onOpenChange]);
  const openShortcutsHelp = useCallback(
    () => shortcutsCtx?.setHelpOpen(true),
    [shortcutsCtx],
  );

  const ctx = useMemo<CommandContext>(
    () => ({
      navigate,
      close,
      currentPath: location.pathname,
      setTheme,
      logout,
      openShortcutsHelp,
    }),
    [navigate, close, location.pathname, setTheme, logout, openShortcutsHelp],
  );

  const visibleCommands = useMemo(
    () => COMMANDS.filter((c) => !c.requiresAdmin || admin),
    [admin],
  );

  const commandsByGroup = useMemo(() => {
    const map: Record<CommandGroupId, Command[]> = {
      create: [],
      navigate: [],
      reports: [],
      preferences: [],
    };
    for (const cmd of visibleCommands) map[cmd.group].push(cmd);
    return map;
  }, [visibleCommands]);

  const entityGroups = useEntityResults({ isAdmin: admin });

  const showEntities = query.trim().length > 0;

  function toggleGroupExpanded(id: string) {
    setExpandedGroups((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  return (
    <CommandDialog open={open} onOpenChange={onOpenChange}>
      <CommandInput
        placeholder="Type a command or search…"
        value={query}
        onValueChange={setQuery}
      />
      <CommandList>
        <CommandEmpty>
          No matches. Try a different word or press Esc to close.
        </CommandEmpty>

        {GROUP_ORDER.map((groupId, index) => {
          const commands = commandsByGroup[groupId];
          if (commands.length === 0) return null;
          return (
            <Fragment key={groupId}>
              {index > 0 && <CommandSeparator />}
              <CommandGroup heading={COMMAND_GROUP_LABELS[groupId]}>
                {commands.map((cmd) => {
                  const Icon = cmd.icon;
                  const showShiftN =
                    cmd.group === "create" &&
                    cmd.targetPath === location.pathname;
                  const shortcutGlyph = showShiftN
                    ? "⇧ N"
                    : cmd.shortcut ?? null;
                  return (
                    <CommandItem
                      key={cmd.id}
                      value={commandSearchValue(cmd)}
                      onSelect={() => {
                        void cmd.run(ctx);
                      }}
                    >
                      <Icon aria-hidden="true" className="mr-2 h-4 w-4" />
                      <span>{cmd.label}</span>
                      {shortcutGlyph ? (
                        <CommandShortcut>
                          <span aria-hidden="true">{shortcutGlyph}</span>
                          <span className="sr-only">
                            {spokenShortcut(shortcutGlyph)}
                          </span>
                        </CommandShortcut>
                      ) : null}
                    </CommandItem>
                  );
                })}
              </CommandGroup>
            </Fragment>
          );
        })}

        {showEntities &&
          entityGroups.map((group) => {
            if (group.items.length === 0) return null;
            const Icon = group.icon;
            const expanded = expandedGroups.has(group.id);
            const visibleItems = expanded
              ? group.items
              : group.items.slice(0, ENTITY_GROUP_VISIBLE_LIMIT);
            const hiddenCount = group.items.length - visibleItems.length;
            return (
              <Fragment key={group.id}>
                <CommandSeparator />
                <CommandGroup heading={group.heading}>
                  {visibleItems.map((item) => (
                    <CommandItem
                      key={item.id}
                      value={item.searchValue}
                      onSelect={() => {
                        close();
                        navigate(item.href);
                      }}
                    >
                      <Icon
                        aria-hidden="true"
                        className="mr-2 h-4 w-4 text-muted-foreground"
                      />
                      <span className="truncate">{item.label}</span>
                      {item.meta ? (
                        <span className="ml-2 truncate font-mono text-xs text-muted-foreground">
                          {item.meta}
                        </span>
                      ) : null}
                    </CommandItem>
                  ))}
                  {hiddenCount > 0 && (
                    <CommandItem
                      key={`${group.id}:more`}
                      value={`show-more-${group.id}`}
                      onSelect={() => toggleGroupExpanded(group.id)}
                      className="text-muted-foreground"
                      aria-expanded={expanded}
                    >
                      <ChevronDown aria-hidden="true" className="mr-2 h-4 w-4" />
                      <span>
                        Show {hiddenCount} more {group.heading.toLowerCase()}
                      </span>
                    </CommandItem>
                  )}
                </CommandGroup>
              </Fragment>
            );
          })}
      </CommandList>
    </CommandDialog>
  );
}

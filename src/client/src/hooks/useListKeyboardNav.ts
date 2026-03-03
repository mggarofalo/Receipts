import { useState, useCallback, useEffect, useRef } from "react";
import { useHotkeys } from "react-hotkeys-hook";

export interface UseListKeyboardNavOptions<T> {
  items: T[];
  getId: (item: T) => string;
  enabled?: boolean;
  onOpen?: (item: T) => void;
  onDelete?: () => void;
  onSelectAll?: () => void;
  onDeselectAll?: () => void;
  onToggleSelect?: (item: T) => void;
  selected?: Set<string>;
}

export function useListKeyboardNav<T>({
  items,
  getId,
  enabled = true,
  onOpen,
  onDelete,
  onSelectAll,
  onDeselectAll,
  onToggleSelect,
  selected,
}: UseListKeyboardNavOptions<T>) {
  const [focusedIndex, setFocusedIndex] = useState(-1);
  const tableRef = useRef<HTMLDivElement>(null);
  const prevItemsLengthRef = useRef(items.length);

  // Reset focus when items change
  useEffect(() => {
    if (prevItemsLengthRef.current !== items.length) {
      prevItemsLengthRef.current = items.length;
      setFocusedIndex(-1);
    }
  }, [items.length]);

  const scrollToRow = useCallback((index: number) => {
    const row = tableRef.current?.querySelectorAll("tbody tr")[index];
    row?.scrollIntoView({ block: "nearest" });
  }, []);

  // j / ArrowDown — move down
  useHotkeys(
    "j, ArrowDown",
    () => {
      setFocusedIndex((prev) => {
        const next = Math.min(prev + 1, items.length - 1);
        scrollToRow(next);
        return next;
      });
    },
    { enabled: enabled && items.length > 0, enableOnFormTags: false },
    [items.length, scrollToRow],
  );

  // k / ArrowUp — move up
  useHotkeys(
    "k, ArrowUp",
    () => {
      setFocusedIndex((prev) => {
        const next = Math.max(prev - 1, 0);
        scrollToRow(next);
        return next;
      });
    },
    { enabled: enabled && items.length > 0, enableOnFormTags: false },
    [items.length, scrollToRow],
  );

  // Enter — open focused item (deferred so the keydown event doesn't
  // propagate into the newly-mounted dialog and trigger an immediate submit)
  useHotkeys(
    "Enter",
    (e) => {
      if (focusedIndex >= 0 && focusedIndex < items.length && onOpen) {
        e.preventDefault();
        e.stopPropagation();
        const item = items[focusedIndex];
        setTimeout(() => onOpen(item), 0);
      }
    },
    {
      enabled: enabled && !!onOpen && focusedIndex >= 0,
      enableOnFormTags: false,
    },
    [focusedIndex, items, onOpen],
  );

  // Space — toggle selection on focused item
  useHotkeys(
    "Space",
    () => {
      if (focusedIndex >= 0 && focusedIndex < items.length && onToggleSelect) {
        onToggleSelect(items[focusedIndex]);
      }
    },
    {
      enabled: enabled && !!onToggleSelect && focusedIndex >= 0,
      enableOnFormTags: false,
      preventDefault: true,
    },
    [focusedIndex, items, onToggleSelect],
  );

  // Delete — trigger delete action
  useHotkeys(
    "Delete",
    () => {
      if (onDelete && selected && selected.size > 0) {
        onDelete();
      }
    },
    {
      enabled: enabled && !!onDelete && !!selected && selected.size > 0,
      enableOnFormTags: false,
    },
    [onDelete, selected],
  );

  // Ctrl+A — select all / deselect all
  useHotkeys(
    "mod+a",
    () => {
      if (!selected || !onSelectAll || !onDeselectAll) return;
      if (selected.size === items.length) {
        onDeselectAll();
      } else {
        onSelectAll();
      }
    },
    {
      enabled: enabled && !!onSelectAll && !!onDeselectAll,
      enableOnFormTags: false,
      preventDefault: true,
    },
    [selected, items.length, onSelectAll, onDeselectAll],
  );

  return {
    focusedIndex,
    setFocusedIndex,
    tableRef,
    focusedId:
      focusedIndex >= 0 && focusedIndex < items.length
        ? getId(items[focusedIndex])
        : null,
  };
}

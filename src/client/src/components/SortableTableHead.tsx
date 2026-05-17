import { TableHead } from "@/components/ui/table";
import { ArrowUp, ArrowDown, ArrowUpDown } from "lucide-react";

interface SortableTableHeadProps {
  column: string;
  label: string;
  currentSortBy: string | null;
  currentSortDirection: "asc" | "desc";
  onToggleSort: (column: string) => void;
  className?: string;
}

export function SortableTableHead({
  column,
  label,
  currentSortBy,
  currentSortDirection,
  onToggleSort,
  className,
}: SortableTableHeadProps) {
  const isActive = currentSortBy === column;

  const ariaSortValue: "ascending" | "descending" | "none" = isActive
    ? currentSortDirection === "asc"
      ? "ascending"
      : "descending"
    : "none";

  return (
    <TableHead
      aria-sort={ariaSortValue}
      className={`select-none ${className ?? ""}`}
    >
      <button
        type="button"
        className="inline-flex items-center gap-1 cursor-pointer hover:text-foreground w-full"
        onClick={() => onToggleSort(column)}
      >
        {label}
        {isActive ? (
          currentSortDirection === "asc" ? (
            <ArrowUp aria-hidden className="h-3.5 w-3.5" />
          ) : (
            <ArrowDown aria-hidden className="h-3.5 w-3.5" />
          )
        ) : (
          <ArrowUpDown aria-hidden className="h-3.5 w-3.5 opacity-30" />
        )}
      </button>
    </TableHead>
  );
}

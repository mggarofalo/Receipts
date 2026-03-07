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

  return (
    <TableHead
      className={`cursor-pointer select-none hover:bg-muted/50 ${className ?? ""}`}
      onClick={() => onToggleSort(column)}
    >
      <span className="inline-flex items-center gap-1">
        {label}
        {isActive ? (
          currentSortDirection === "asc" ? (
            <ArrowUp className="h-3.5 w-3.5" />
          ) : (
            <ArrowDown className="h-3.5 w-3.5" />
          )
        ) : (
          <ArrowUpDown className="h-3.5 w-3.5 opacity-30" />
        )}
      </span>
    </TableHead>
  );
}

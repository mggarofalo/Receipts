import { useSearchParams } from "react-router";
import { useCallback } from "react";

interface UseServerSortOptions {
  defaultSortBy?: string;
  defaultSortDirection?: "asc" | "desc";
}

interface UseServerSortReturn {
  sortBy: string | null;
  sortDirection: "asc" | "desc";
  toggleSort: (column: string) => void;
}

export function useServerSort({
  defaultSortBy,
  defaultSortDirection = "asc",
}: UseServerSortOptions = {}): UseServerSortReturn {
  const [searchParams, setSearchParams] = useSearchParams();

  const sortBy = searchParams.get("sortBy") ?? defaultSortBy ?? null;
  const rawDirection = searchParams.get("sortDirection");
  const sortDirection: "asc" | "desc" =
    rawDirection === "asc" || rawDirection === "desc"
      ? rawDirection
      : defaultSortDirection;

  const toggleSort = useCallback(
    (column: string) => {
      setSearchParams((prev) => {
        const next = new URLSearchParams(prev);
        if (sortBy === column) {
          next.set("sortDirection", sortDirection === "asc" ? "desc" : "asc");
        } else {
          next.set("sortBy", column);
          next.set("sortDirection", "asc");
        }
        return next;
      });
    },
    [sortBy, sortDirection, setSearchParams],
  );

  return { sortBy, sortDirection, toggleSort };
}

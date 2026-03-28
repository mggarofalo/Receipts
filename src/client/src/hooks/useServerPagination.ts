import { useReducer, useMemo, useCallback, useRef, useEffect } from "react";
import { getPersistedPageSize, persistPageSize } from "@/lib/page-size";

interface UseServerPaginationOptions {
  defaultPageSize?: number;
  sortBy?: string | null;
  sortDirection?: "asc" | "desc";
}

interface UseServerPaginationReturn {
  offset: number;
  limit: number;
  currentPage: number;
  pageSize: number;
  totalPages: (total: number) => number;
  setPage: (page: number, total: number) => void;
  setPageSize: (size: number) => void;
  resetPage: () => void;
}

type Action =
  | { type: "SET_OFFSET"; offset: number }
  | { type: "SET_LIMIT"; limit: number };

interface State {
  offset: number;
  limit: number;
}

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case "SET_OFFSET":
      if (state.offset === action.offset) return state;
      return { ...state, offset: action.offset };
    case "SET_LIMIT":
      if (state.offset === 0 && state.limit === action.limit) return state;
      return { offset: 0, limit: action.limit };
  }
}

export function useServerPagination({
  defaultPageSize = 25,
  sortBy,
  sortDirection,
}: UseServerPaginationOptions = {}): UseServerPaginationReturn {
  const [state, dispatch] = useReducer(reducer, {
    offset: 0,
    limit: getPersistedPageSize() ?? defaultPageSize,
  });

  // Reset to page 1 when sort params change (e.g. browser back/forward).
  // Skip the initial mount so we don't dispatch a redundant reset.
  const prevSortRef = useRef({ sortBy, sortDirection });
  useEffect(() => {
    const prev = prevSortRef.current;
    if (prev.sortBy !== sortBy || prev.sortDirection !== sortDirection) {
      prevSortRef.current = { sortBy, sortDirection };
      dispatch({ type: "SET_OFFSET", offset: 0 });
    }
  }, [sortBy, sortDirection]);

  const currentPage = useMemo(
    () => Math.floor(state.offset / state.limit) + 1,
    [state.offset, state.limit],
  );

  const totalPages = useCallback(
    (total: number) => Math.max(1, Math.ceil(total / state.limit)),
    [state.limit],
  );

  const setPage = useCallback(
    (page: number, total: number) => {
      const maxPage = Math.max(1, Math.ceil(total / state.limit));
      const safePage = Math.max(1, Math.min(page, maxPage));
      dispatch({ type: "SET_OFFSET", offset: (safePage - 1) * state.limit });
    },
    [state.limit],
  );

  const setPageSize = useCallback((size: number) => {
    persistPageSize(size);
    dispatch({ type: "SET_LIMIT", limit: size });
  }, []);

  const resetPage = useCallback(() => {
    dispatch({ type: "SET_OFFSET", offset: 0 });
  }, []);

  return useMemo(
    () => ({
      offset: state.offset,
      limit: state.limit,
      currentPage,
      pageSize: state.limit,
      totalPages,
      setPage,
      setPageSize,
      resetPage,
    }),
    [
      state.offset,
      state.limit,
      currentPage,
      totalPages,
      setPage,
      setPageSize,
      resetPage,
    ],
  );
}

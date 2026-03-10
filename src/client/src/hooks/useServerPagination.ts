import { useReducer, useMemo, useCallback } from "react";
import { getPersistedPageSize, persistPageSize } from "@/lib/page-size";

interface UseServerPaginationOptions {
  defaultPageSize?: number;
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
}: UseServerPaginationOptions = {}): UseServerPaginationReturn {
  const [state, dispatch] = useReducer(reducer, {
    offset: 0,
    limit: getPersistedPageSize() ?? defaultPageSize,
  });

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

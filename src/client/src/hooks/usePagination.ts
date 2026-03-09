import { useReducer, useMemo, useCallback } from "react";
import { getPersistedPageSize, persistPageSize } from "@/lib/page-size";

interface UsePaginationOptions<T> {
  items: T[];
  defaultPageSize?: number;
}

interface UsePaginationReturn<T> {
  paginatedItems: T[];
  currentPage: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
}

type Action =
  | { type: "SET_PAGE"; page: number }
  | { type: "SET_PAGE_SIZE"; size: number };

interface State {
  currentPage: number;
  pageSize: number;
}

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case "SET_PAGE":
      return { ...state, currentPage: action.page };
    case "SET_PAGE_SIZE":
      return { currentPage: 1, pageSize: action.size };
  }
}

export function usePagination<T>({
  items,
  defaultPageSize = 25,
}: UsePaginationOptions<T>): UsePaginationReturn<T> {
  const [state, dispatch] = useReducer(reducer, {
    currentPage: 1,
    pageSize: getPersistedPageSize() ?? defaultPageSize,
  });

  const totalItems = items.length;
  const totalPages = Math.max(1, Math.ceil(totalItems / state.pageSize));

  const safePage = Math.min(state.currentPage, totalPages);

  const paginatedItems = useMemo(() => {
    const start = (safePage - 1) * state.pageSize;
    return items.slice(start, start + state.pageSize);
  }, [items, safePage, state.pageSize]);

  const setPage = useCallback(
    (page: number) => {
      dispatch({
        type: "SET_PAGE",
        page: Math.max(1, Math.min(page, totalPages)),
      });
    },
    [totalPages],
  );

  const setPageSize = useCallback((size: number) => {
    persistPageSize(size);
    dispatch({ type: "SET_PAGE_SIZE", size });
  }, []);

  return {
    paginatedItems,
    currentPage: safePage,
    pageSize: state.pageSize,
    totalItems,
    totalPages,
    setPage,
    setPageSize,
  };
}

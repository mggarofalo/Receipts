import { useSearchParams } from "react-router";
import { useCallback, useMemo } from "react";

export function useEntityLinkParams<T extends string>(recognizedParams: readonly T[]) {
  const [searchParams, setSearchParams] = useSearchParams();

  const params = useMemo(() => {
    const result: Partial<Record<T, string>> = {};
    for (const key of recognizedParams) {
      const value = searchParams.get(key);
      if (value) result[key] = value;
    }
    return result;
  }, [searchParams, recognizedParams]);

  const clearParams = useCallback(() => {
    setSearchParams((prev) => {
      const next = new URLSearchParams(prev);
      for (const key of recognizedParams) {
        next.delete(key);
      }
      return next;
    });
  }, [setSearchParams, recognizedParams]);

  const hasActiveFilter = Object.keys(params).length > 0;

  return { params, clearParams, hasActiveFilter };
}

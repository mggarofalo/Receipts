import { useMemo } from "react";
import { useAuth } from "@/hooks/useAuth";

export function usePermission() {
  const { user } = useAuth();
  const roles = user?.roles ?? [];

  return useMemo(
    () => ({
      roles,
      hasRole: (role: string) => roles.includes(role),
      isAdmin: () => roles.includes("Admin"),
    }),
    [roles],
  );
}

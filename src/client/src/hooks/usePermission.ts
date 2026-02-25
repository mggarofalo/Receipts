import { useMemo } from "react";
import { useAuth } from "@/hooks/useAuth";

export function usePermission() {
  const { user } = useAuth();

  return useMemo(() => {
    const roles = user?.roles ?? [];
    return {
      roles,
      hasRole: (role: string) => roles.includes(role),
      isAdmin: () => roles.includes("Admin"),
    };
  }, [user?.roles]);
}

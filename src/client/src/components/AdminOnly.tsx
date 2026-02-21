import type { ReactNode } from "react";
import { usePermission } from "@/hooks/usePermission";

export function AdminOnly({ children }: { children: ReactNode }) {
  const { isAdmin } = usePermission();
  return isAdmin() ? children : null;
}

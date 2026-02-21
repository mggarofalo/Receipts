import { Navigate } from "react-router";
import type { ReactNode } from "react";
import { usePermission } from "@/hooks/usePermission";

export function AdminRoute({ children }: { children: ReactNode }) {
  const { isAdmin } = usePermission();

  if (!isAdmin()) {
    return <Navigate to="/" replace />;
  }

  return children;
}

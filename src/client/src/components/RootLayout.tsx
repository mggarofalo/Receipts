import { Outlet } from "react-router";
import { Toaster } from "@/components/ui/sonner";
import { ErrorBoundary } from "@/components/ErrorBoundary";

export function RootLayout() {
  return (
    <ErrorBoundary>
      <Outlet />
      <Toaster />
    </ErrorBoundary>
  );
}

import { useLocation } from "react-router";

const routeLabels: Record<string, string> = {
  "/": "Home",
  "/accounts": "Accounts",
  "/receipts": "Receipts",
  "/receipt-items": "Receipt Items",
  "/transactions": "Transactions",
  "/trips": "Trips",
  "/receipt-detail": "Receipt Detail",
  "/transaction-detail": "Transaction Detail",
  "/api-keys": "API Keys",
  "/security": "Security",
  "/audit": "Audit Log",
  "/trash": "Recycle Bin",
  "/admin/users": "User Management",
};

export interface BreadcrumbSegment {
  label: string;
  path: string;
}

export function useBreadcrumbs(): BreadcrumbSegment[] {
  const { pathname } = useLocation();

  if (pathname === "/") return [];

  const crumbs: BreadcrumbSegment[] = [{ label: "Home", path: "/" }];

  // Handle nested paths like /admin/users
  const label = routeLabels[pathname];
  if (label) {
    crumbs.push({ label, path: pathname });
  } else {
    // Build segments for unknown paths
    const parts = pathname.split("/").filter(Boolean);
    let accumulated = "";
    for (const part of parts) {
      accumulated += `/${part}`;
      const segLabel = routeLabels[accumulated] ?? part;
      crumbs.push({ label: segLabel, path: accumulated });
    }
  }

  return crumbs;
}

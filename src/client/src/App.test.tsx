import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { createMemoryRouter, RouterProvider, Outlet } from "react-router";
import type { ReactNode } from "react";

// Mock all page components to simple stubs
vi.mock("@/pages/Home", () => ({
  default: () => <div data-testid="page-home">Home Page</div>,
}));
vi.mock("@/pages/Login", () => ({
  default: () => <div data-testid="page-login">Login Page</div>,
}));
vi.mock("@/pages/ChangePassword", () => ({
  default: () => <div data-testid="page-change-password">Change Password</div>,
}));
vi.mock("@/pages/Accounts", () => ({
  default: () => <div data-testid="page-accounts">Accounts Page</div>,
}));
vi.mock("@/pages/Categories", () => ({
  default: () => <div data-testid="page-categories">Categories</div>,
}));
vi.mock("@/pages/Subcategories", () => ({
  default: () => <div data-testid="page-subcategories">Subcategories</div>,
}));
vi.mock("@/pages/Receipts", () => ({
  default: () => <div data-testid="page-receipts">Receipts</div>,
}));
vi.mock("@/pages/ReceiptItems", () => ({
  default: () => <div data-testid="page-receipt-items">ReceiptItems</div>,
}));
vi.mock("@/pages/Transactions", () => ({
  default: () => <div data-testid="page-transactions">Transactions</div>,
}));
vi.mock("@/pages/Trips", () => ({
  default: () => <div data-testid="page-trips">Trips</div>,
}));
vi.mock("@/pages/ItemTemplates", () => ({
  default: () => <div data-testid="page-item-templates">ItemTemplates</div>,
}));
vi.mock("@/pages/ReceiptDetail", () => ({
  default: () => <div data-testid="page-receipt-detail">ReceiptDetail</div>,
}));
vi.mock("@/pages/TransactionDetail", () => ({
  default: () => (
    <div data-testid="page-transaction-detail">TransactionDetail</div>
  ),
}));
vi.mock("@/pages/ApiKeys", () => ({
  default: () => <div data-testid="page-api-keys">ApiKeys</div>,
}));
vi.mock("@/pages/AdminUsers", () => ({
  default: () => <div data-testid="page-admin-users">AdminUsers</div>,
}));
vi.mock("@/pages/AuditLog", () => ({
  default: () => <div data-testid="page-audit-log">AuditLog</div>,
}));
vi.mock("@/pages/SecurityLog", () => ({
  default: () => <div data-testid="page-security-log">SecurityLog</div>,
}));
vi.mock("@/pages/RecycleBin", () => ({
  default: () => <div data-testid="page-recycle-bin">RecycleBin</div>,
}));
vi.mock("@/pages/NotFound", () => ({
  default: () => <div data-testid="page-not-found">Not Found</div>,
}));

// Mock layout/route wrappers to pass through children via Outlet
vi.mock("@/components/RootLayout", () => ({
  RootLayout: () => (
    <div data-testid="root-layout">
      <Outlet />
    </div>
  ),
}));

vi.mock("@/components/PublicLayout", () => ({
  PublicLayout: () => (
    <div data-testid="public-layout">
      <Outlet />
    </div>
  ),
}));

vi.mock("@/components/Layout", () => ({
  Layout: () => (
    <div data-testid="layout">
      <Outlet />
    </div>
  ),
}));

vi.mock("@/components/ProtectedRoute", () => ({
  ProtectedRoute: ({ children }: { children: ReactNode }) => <>{children}</>,
}));

vi.mock("@/components/AdminRoute", () => ({
  AdminRoute: ({ children }: { children: ReactNode }) => <>{children}</>,
}));

vi.mock("@/components/ui/sonner", () => ({
  Toaster: () => null,
}));

// Import mocked components (vi.mock is hoisted, so these get the mocked versions)
import { RootLayout } from "@/components/RootLayout";
import { PublicLayout } from "@/components/PublicLayout";
import { Layout } from "@/components/Layout";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { AdminRoute } from "@/components/AdminRoute";
import Home from "@/pages/Home";
import Login from "@/pages/Login";
import ChangePassword from "@/pages/ChangePassword";
import Accounts from "@/pages/Accounts";
import Categories from "@/pages/Categories";
import Subcategories from "@/pages/Subcategories";
import Receipts from "@/pages/Receipts";
import ReceiptItems from "@/pages/ReceiptItems";
import Transactions from "@/pages/Transactions";
import Trips from "@/pages/Trips";
import ItemTemplates from "@/pages/ItemTemplates";
import ReceiptDetail from "@/pages/ReceiptDetail";
import TransactionDetail from "@/pages/TransactionDetail";
import ApiKeys from "@/pages/ApiKeys";
import AdminUsers from "@/pages/AdminUsers";
import AuditLog from "@/pages/AuditLog";
import SecurityLog from "@/pages/SecurityLog";
import RecycleBin from "@/pages/RecycleBin";
import NotFound from "@/pages/NotFound";

// Build the route config matching App.tsx structure
const routes = [
  {
    element: <RootLayout />,
    children: [
      {
        element: <PublicLayout />,
        children: [
          { path: "/login", element: <Login /> },
          { path: "/change-password", element: <ChangePassword /> },
        ],
      },
      {
        element: (
          <ProtectedRoute>
            <Layout />
          </ProtectedRoute>
        ),
        children: [
          { path: "/", element: <Home /> },
          { path: "/accounts", element: <Accounts /> },
          { path: "/categories", element: <Categories /> },
          { path: "/subcategories", element: <Subcategories /> },
          { path: "/receipts", element: <Receipts /> },
          { path: "/receipt-items", element: <ReceiptItems /> },
          { path: "/transactions", element: <Transactions /> },
          { path: "/trips", element: <Trips /> },
          { path: "/item-templates", element: <ItemTemplates /> },
          { path: "/receipt-detail", element: <ReceiptDetail /> },
          { path: "/transaction-detail", element: <TransactionDetail /> },
          { path: "/api-keys", element: <ApiKeys /> },
          { path: "/security", element: <SecurityLog /> },
          {
            path: "/audit",
            element: (
              <AdminRoute>
                <AuditLog />
              </AdminRoute>
            ),
          },
          {
            path: "/trash",
            element: (
              <AdminRoute>
                <RecycleBin />
              </AdminRoute>
            ),
          },
          {
            path: "/admin/users",
            element: (
              <AdminRoute>
                <AdminUsers />
              </AdminRoute>
            ),
          },
        ],
      },
      { path: "*", element: <NotFound /> },
    ],
  },
];

function renderRoute(path: string) {
  const router = createMemoryRouter(routes, { initialEntries: [path] });
  return render(<RouterProvider router={router} />);
}

describe("App router", () => {
  it('renders Home page at "/" route', async () => {
    renderRoute("/");
    expect(await screen.findByTestId("page-home")).toBeInTheDocument();
  });

  it('renders Accounts page at "/accounts" route', async () => {
    renderRoute("/accounts");
    expect(await screen.findByTestId("page-accounts")).toBeInTheDocument();
  });

  it('renders Login page at "/login" route', async () => {
    renderRoute("/login");
    expect(await screen.findByTestId("page-login")).toBeInTheDocument();
  });

  it("renders NotFound for unknown routes", async () => {
    renderRoute("/some/unknown/path");
    expect(await screen.findByTestId("page-not-found")).toBeInTheDocument();
  });
});

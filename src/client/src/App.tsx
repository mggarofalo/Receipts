import { createBrowserRouter } from "react-router";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { AdminRoute } from "@/components/AdminRoute";
import { Layout } from "@/components/Layout";
import { PublicLayout } from "@/components/PublicLayout";
import { RootLayout } from "@/components/RootLayout";
import Home from "@/pages/Home";
import Login from "@/pages/Login";
import Register from "@/pages/Register";
import ApiKeys from "@/pages/ApiKeys";
import Accounts from "@/pages/Accounts";
import Receipts from "@/pages/Receipts";
import ReceiptItems from "@/pages/ReceiptItems";
import Transactions from "@/pages/Transactions";
import Trips from "@/pages/Trips";
import ReceiptDetail from "@/pages/ReceiptDetail";
import TransactionDetail from "@/pages/TransactionDetail";
import AdminUsers from "@/pages/AdminUsers";
import AuditLog from "@/pages/AuditLog";
import SecurityLog from "@/pages/SecurityLog";
import RecycleBin from "@/pages/RecycleBin";
import NotFound from "@/pages/NotFound";

export const router = createBrowserRouter([
  {
    element: <RootLayout />,
    children: [
      {
        element: <PublicLayout />,
        children: [
          { path: "/login", element: <Login /> },
          { path: "/register", element: <Register /> },
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
          { path: "/receipts", element: <Receipts /> },
          { path: "/receipt-items", element: <ReceiptItems /> },
          { path: "/transactions", element: <Transactions /> },
          { path: "/trips", element: <Trips /> },
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
]);

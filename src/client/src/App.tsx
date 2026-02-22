import { Routes, Route } from "react-router";
import { Toaster } from "@/components/ui/sonner";
import { ErrorBoundary } from "@/components/ErrorBoundary";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { AdminRoute } from "@/components/AdminRoute";
import { Layout } from "@/components/Layout";
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

function App() {
  return (
    <ErrorBoundary>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        {/* Protected routes with layout */}
        <Route
          element={
            <ProtectedRoute>
              <Layout />
            </ProtectedRoute>
          }
        >
          <Route path="/" element={<Home />} />
          <Route path="/accounts" element={<Accounts />} />
          <Route path="/receipts" element={<Receipts />} />
          <Route path="/receipt-items" element={<ReceiptItems />} />
          <Route path="/transactions" element={<Transactions />} />
          <Route path="/trips" element={<Trips />} />
          <Route path="/receipt-detail" element={<ReceiptDetail />} />
          <Route path="/transaction-detail" element={<TransactionDetail />} />
          <Route path="/api-keys" element={<ApiKeys />} />
          <Route path="/security" element={<SecurityLog />} />
          <Route
            path="/audit"
            element={
              <AdminRoute>
                <AuditLog />
              </AdminRoute>
            }
          />
          <Route
            path="/trash"
            element={
              <AdminRoute>
                <RecycleBin />
              </AdminRoute>
            }
          />
          <Route
            path="/admin/users"
            element={
              <AdminRoute>
                <AdminUsers />
              </AdminRoute>
            }
          />
        </Route>

        <Route path="*" element={<NotFound />} />
      </Routes>
      <Toaster />
    </ErrorBoundary>
  );
}

export default App;

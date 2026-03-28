import { useSearchParams, Navigate } from "react-router";

/**
 * Backward-compatibility redirect: /receipt-detail?id=X -> /receipts/X
 */
export default function ReceiptDetailRedirect() {
  const [searchParams] = useSearchParams();
  const id = searchParams.get("id");

  if (!id) {
    return <Navigate to="/receipts" replace />;
  }

  return <Navigate to={`/receipts/${id}`} replace />;
}

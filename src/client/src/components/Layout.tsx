import { Link, Outlet, useNavigate } from "react-router";
import { useAuth } from "@/hooks/useAuth";
import { usePermission } from "@/hooks/usePermission";
import { useSignalR } from "@/hooks/useSignalR";
import type { SignalRConnectionState } from "@/hooks/useSignalR";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Separator } from "@/components/ui/separator";

const connectionStateColors: Record<SignalRConnectionState, string> = {
  connected: "bg-green-500",
  reconnecting: "bg-yellow-500 animate-pulse",
  disconnected: "bg-red-500",
};

const connectionStateLabels: Record<SignalRConnectionState, string> = {
  connected: "Live",
  reconnecting: "Reconnecting",
  disconnected: "Offline",
};

export function Layout() {
  const { user, logout } = useAuth();
  const { isAdmin } = usePermission();
  const navigate = useNavigate();
  const { connectionState } = useSignalR(!!user);

  async function handleLogout() {
    await logout();
    navigate("/login");
  }

  return (
    <div className="min-h-screen flex flex-col">
      <header className="border-b">
        <div className="container mx-auto flex h-14 items-center justify-between px-4">
          <nav className="flex items-center gap-6">
            <Link to="/" className="font-semibold text-lg">
              Receipts
            </Link>
            <Separator orientation="vertical" className="h-6" />
            <Link
              to="/"
              className="text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              Home
            </Link>
            <Link
              to="/accounts"
              className="text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              Accounts
            </Link>
            <Link
              to="/receipts"
              className="text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              Receipts
            </Link>
            <Link
              to="/receipt-items"
              className="text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              Items
            </Link>
            <Link
              to="/transactions"
              className="text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              Transactions
            </Link>
            <Link
              to="/trips"
              className="text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              Trips
            </Link>
            <Link
              to="/security"
              className="text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              Security
            </Link>
            {isAdmin() && (
              <>
                <Link
                  to="/admin/users"
                  className="text-sm text-muted-foreground hover:text-foreground transition-colors"
                >
                  Users
                </Link>
                <Link
                  to="/audit"
                  className="text-sm text-muted-foreground hover:text-foreground transition-colors"
                >
                  Audit
                </Link>
                <Link
                  to="/trash"
                  className="text-sm text-muted-foreground hover:text-foreground transition-colors"
                >
                  Trash
                </Link>
              </>
            )}
          </nav>

          <div className="flex items-center gap-3">
            <div
              className="flex items-center gap-1.5"
              title={`SignalR: ${connectionStateLabels[connectionState]}`}
            >
              <span
                className={`h-2 w-2 rounded-full ${connectionStateColors[connectionState]}`}
              />
              <span className="text-xs text-muted-foreground">
                {connectionStateLabels[connectionState]}
              </span>
            </div>

            {user && (
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" size="sm">
                    {user.email}
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  <DropdownMenuItem onClick={() => navigate("/api-keys")}>
                    API Keys
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={handleLogout}>
                    Logout
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            )}
          </div>
        </div>
      </header>

      <main className="flex-1 container mx-auto px-4 py-6">
        <Outlet />
      </main>
    </div>
  );
}

import { Link, Outlet, useNavigate } from "react-router";
import { useAuth } from "@/hooks/useAuth";
import { usePermission } from "@/hooks/usePermission";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Separator } from "@/components/ui/separator";
import { ConnectionStatus } from "@/components/ConnectionStatus";
import { ShortcutsHelp } from "@/components/ShortcutsHelp";
import { PageTransition } from "@/components/PageTransition";

export function Layout() {
  const { user, logout } = useAuth();
  const { isAdmin } = usePermission();
  const navigate = useNavigate();

  async function handleLogout() {
    await logout();
    navigate("/login");
  }

  return (
    <div className="min-h-screen flex flex-col">
      {/* Skip navigation — a11y (MGG-46) */}
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:absolute focus:top-2 focus:left-2 focus:z-50 focus:rounded focus:bg-background focus:px-4 focus:py-2 focus:text-sm focus:font-medium focus:ring-2 focus:ring-ring"
      >
        Skip to main content
      </a>

      <header className="border-b">
        <div className="container mx-auto flex h-14 items-center justify-between px-4">
          <nav aria-label="Main navigation" className="flex items-center gap-6">
            <Link to="/" className="font-semibold text-lg">
              Receipts
            </Link>
            <Separator orientation="vertical" className="h-6" aria-hidden="true" />
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
            <ConnectionStatus />

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

      <main id="main-content" className="flex-1 container mx-auto px-4 py-6">
        <PageTransition>
          <Outlet />
        </PageTransition>
      </main>

      {/* Global keyboard shortcuts handler — renders the ? help modal */}
      <ShortcutsHelp />
    </div>
  );
}

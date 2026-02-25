import { useState } from "react";
import { Link, Outlet, useNavigate, useNavigation, useLocation } from "react-router";
import { Search } from "lucide-react";
import { useAuth } from "@/hooks/useAuth";
import { usePermission } from "@/hooks/usePermission";
import { useSignalR } from "@/hooks/useSignalR";
import type { SignalRConnectionState } from "@/hooks/useSignalR";
import { useGlobalShortcuts } from "@/hooks/useGlobalShortcuts";
import { ShortcutsHelp } from "@/components/ShortcutsHelp";
import { Breadcrumbs } from "@/components/Breadcrumbs";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Separator } from "@/components/ui/separator";
import { GlobalSearchDialog } from "@/components/GlobalSearchDialog";
import { ThemeToggle } from "@/components/ThemeToggle";

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
  const navigation = useNavigation();
  const location = useLocation();
  const { connectionState } = useSignalR(!!user);
  const [searchOpen, setSearchOpen] = useState(false);
  useGlobalShortcuts();

  function navLinkProps(to: string) {
    const isActive =
      to === "/" ? location.pathname === "/" : location.pathname.startsWith(to);
    return {
      to,
      className:
        "text-sm text-muted-foreground hover:text-foreground transition-colors",
      ...(isActive ? { "aria-current": "page" as const } : {}),
    };
  }

  async function handleLogout() {
    await logout();
    navigate("/login");
  }

  return (
    <div className="min-h-screen flex flex-col">
      <a href="#main-content" className="skip-link">
        Skip to main content
      </a>
      <header className="border-b">
        <div className="container mx-auto flex h-14 items-center justify-between px-4">
          <nav className="flex items-center gap-6" aria-label="Main navigation">
            <Link to="/" className="font-semibold text-lg">
              Receipts
            </Link>
            <Separator orientation="vertical" className="h-6" />
            <Link {...navLinkProps("/")}>Home</Link>
            <Link {...navLinkProps("/accounts")}>Accounts</Link>
            <Link {...navLinkProps("/receipts")}>Receipts</Link>
            <Link {...navLinkProps("/receipt-items")}>Items</Link>
            <Link {...navLinkProps("/transactions")}>Transactions</Link>
            <Link {...navLinkProps("/trips")}>Trips</Link>
            <Link {...navLinkProps("/security")}>Security</Link>
            {isAdmin() && (
              <>
                <Link {...navLinkProps("/admin/users")}>Users</Link>
                <Link {...navLinkProps("/audit")}>Audit</Link>
                <Link {...navLinkProps("/trash")}>Trash</Link>
              </>
            )}
          </nav>

          <div className="flex items-center gap-3">
            <Button
              variant="outline"
              size="sm"
              className="gap-1.5 text-muted-foreground"
              onClick={() => setSearchOpen(true)}
            >
              <Search className="h-3.5 w-3.5" />
              Search
              <kbd className="pointer-events-none select-none rounded border bg-muted px-1.5 py-0.5 text-[10px] font-medium">
                Ctrl+K
              </kbd>
            </Button>
            <div
              className="flex items-center gap-1.5"
              role="status"
              aria-live="polite"
            >
              <span
                className={`h-2 w-2 rounded-full ${connectionStateColors[connectionState]}`}
                aria-hidden="true"
              />
              <span className="text-xs text-muted-foreground">
                {connectionStateLabels[connectionState]}
              </span>
            </div>

            <ThemeToggle />

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

      {navigation.state === "loading" && (
        <div className="h-0.5 w-full overflow-hidden bg-muted">
          <div className="h-full w-1/3 animate-pulse bg-primary" />
        </div>
      )}

      <main id="main-content" tabIndex={-1} className="flex-1 container mx-auto px-4 py-6">
        <Breadcrumbs />
        <div key={location.pathname} className="animate-in fade-in duration-200">
          <Outlet />
        </div>
      </main>

      <GlobalSearchDialog open={searchOpen} onOpenChange={setSearchOpen} />
      <ShortcutsHelp />
    </div>
  );
}

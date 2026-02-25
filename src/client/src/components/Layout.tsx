import { useState } from "react";
import { Link, Outlet, useNavigate, useNavigation, useLocation } from "react-router";
import { Menu, Search } from "lucide-react";
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
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
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
  const [mobileOpen, setMobileOpen] = useState(false);
  useGlobalShortcuts();

  function navLinkClass(to: string) {
    const isActive =
      to === "/" ? location.pathname === "/" : location.pathname.startsWith(to);
    return {
      className:
        "text-sm text-muted-foreground hover:text-foreground transition-colors",
      ...(isActive ? { "aria-current": "page" as const } : {}),
    };
  }

  function mobileNavLink(to: string, label: string) {
    const isActive =
      to === "/" ? location.pathname === "/" : location.pathname.startsWith(to);
    return (
      <Link
        key={to}
        to={to}
        onClick={() => setMobileOpen(false)}
        className={`block rounded-md px-3 py-2 text-sm transition-colors ${
          isActive
            ? "bg-accent text-accent-foreground font-medium"
            : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
        }`}
      >
        {label}
      </Link>
    );
  }

  async function handleLogout() {
    await logout();
    navigate("/login");
  }

  const navLinks = [
    { to: "/", label: "Home" },
    { to: "/accounts", label: "Accounts" },
    { to: "/receipts", label: "Receipts" },
    { to: "/receipt-items", label: "Items" },
    { to: "/transactions", label: "Transactions" },
    { to: "/trips", label: "Trips" },
    { to: "/security", label: "Security" },
  ];

  const adminLinks = [
    { to: "/admin/users", label: "Users" },
    { to: "/audit", label: "Audit" },
    { to: "/trash", label: "Trash" },
  ];

  return (
    <div className="min-h-screen flex flex-col">
      <a href="#main-content" className="skip-link">
        Skip to main content
      </a>
      <header className="border-b">
        <div className="container mx-auto flex h-14 items-center justify-between px-4">
          {/* Mobile: hamburger + brand */}
          <div className="flex items-center gap-2 lg:hidden">
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8"
              onClick={() => setMobileOpen(true)}
              aria-label="Open navigation menu"
            >
              <Menu className="h-5 w-5" />
            </Button>
            <Link to="/" className="font-semibold text-lg">
              Receipts
            </Link>
          </div>

          {/* Desktop: full nav */}
          <nav
            className="hidden lg:flex items-center gap-6"
            aria-label="Main navigation"
          >
            <Link to="/" className="font-semibold text-lg">
              Receipts
            </Link>
            <Separator orientation="vertical" className="h-6" />
            {navLinks.map(({ to, label }) => (
              <Link key={to} to={to} {...navLinkClass(to)}>
                {label}
              </Link>
            ))}
            {isAdmin() &&
              adminLinks.map(({ to, label }) => (
                <Link key={to} to={to} {...navLinkClass(to)}>
                  {label}
                </Link>
              ))}
          </nav>

          {/* Desktop: right-side actions */}
          <div className="hidden lg:flex items-center gap-3">
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

          {/* Mobile: compact actions */}
          <div className="flex items-center gap-1 lg:hidden">
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8"
              onClick={() => setSearchOpen(true)}
              aria-label="Search"
            >
              <Search className="h-4 w-4" />
            </Button>
            <ThemeToggle />
          </div>
        </div>
      </header>

      {navigation.state === "loading" && (
        <div className="h-0.5 w-full overflow-hidden bg-muted">
          <div className="h-full w-1/3 animate-pulse bg-primary" />
        </div>
      )}

      <main
        id="main-content"
        tabIndex={-1}
        className="flex-1 container mx-auto px-4 py-6"
      >
        <Breadcrumbs />
        <div key={location.pathname} className="animate-in fade-in duration-200">
          <Outlet />
        </div>
      </main>

      {/* Mobile navigation drawer */}
      <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
        <SheetContent side="left" className="w-72">
          <SheetHeader>
            <SheetTitle>Receipts</SheetTitle>
          </SheetHeader>
          <nav className="flex flex-col gap-1 px-4" aria-label="Mobile navigation">
            {navLinks.map(({ to, label }) => mobileNavLink(to, label))}
            {isAdmin() && (
              <>
                <Separator className="my-2" />
                <span className="px-3 py-1 text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  Admin
                </span>
                {adminLinks.map(({ to, label }) => mobileNavLink(to, label))}
              </>
            )}
          </nav>
          <Separator className="mx-4" />
          <div className="flex flex-col gap-2 px-4">
            <div
              className="flex items-center gap-1.5 px-3 py-1"
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
            {user && (
              <>
                <Link
                  to="/api-keys"
                  onClick={() => setMobileOpen(false)}
                  className="block rounded-md px-3 py-2 text-sm text-muted-foreground hover:bg-accent hover:text-accent-foreground transition-colors"
                >
                  API Keys
                </Link>
                <button
                  onClick={() => {
                    setMobileOpen(false);
                    handleLogout();
                  }}
                  className="rounded-md px-3 py-2 text-left text-sm text-muted-foreground hover:bg-accent hover:text-accent-foreground transition-colors"
                >
                  Logout
                </button>
              </>
            )}
          </div>
        </SheetContent>
      </Sheet>

      <GlobalSearchDialog open={searchOpen} onOpenChange={setSearchOpen} />
      <ShortcutsHelp />
    </div>
  );
}

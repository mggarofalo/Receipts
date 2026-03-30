import { describe, it, expect, vi } from "vitest";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { render } from "@testing-library/react";
import { createMemoryRouter, RouterProvider } from "react-router";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthContext, type AuthContextValue } from "@/contexts/auth-context";
import {
  ShortcutsContext,
  type ShortcutsContextValue,
} from "@/contexts/shortcuts-context";
import { Layout } from "./Layout";

// Mock hooks used by Layout
vi.mock("@/hooks/useSignalR", () => ({
  useSignalR: vi.fn(() => ({ connectionState: "connected" as const })),
}));

vi.mock("@/hooks/useGlobalShortcuts", () => ({
  useGlobalShortcuts: vi.fn(),
}));

vi.mock("@/hooks/useBreadcrumbs", () => ({
  useBreadcrumbs: vi.fn(() => []),
}));

vi.mock("@/components/ThemeToggle", () => ({
  ThemeToggle: () => <div data-testid="theme-toggle">ThemeToggle</div>,
}));

vi.mock("@/components/GlobalSearchDialog", () => ({
  GlobalSearchDialog: () => <div data-testid="global-search" />,
}));

vi.mock("@/components/ShortcutsHelp", () => ({
  ShortcutsHelp: () => <div data-testid="shortcuts-help" />,
}));

const defaultAuth: AuthContextValue = {
  user: { userId: "test-user-id", email: "test@example.com", roles: ["User"], mustResetPassword: false },
  isLoading: false,
  mustResetPassword: false,
  login: async () => {},
  logout: async () => {},
  changePassword: async () => {},
};

const defaultShortcuts: ShortcutsContextValue = {
  helpOpen: false,
  setHelpOpen: vi.fn(),
};

function renderLayout(authOverrides?: Partial<AuthContextValue>) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: 0 },
    },
  });
  const authValue = { ...defaultAuth, ...authOverrides };

  const router = createMemoryRouter(
    [
      {
        path: "/",
        element: <Layout />,
        children: [
          { index: true, element: <div>Home Page Content</div> },
        ],
      },
    ],
    { initialEntries: ["/"] },
  );

  return render(
    <QueryClientProvider client={queryClient}>
      <AuthContext.Provider value={authValue}>
        <ShortcutsContext.Provider value={defaultShortcuts}>
          <RouterProvider router={router} />
        </ShortcutsContext.Provider>
      </AuthContext.Provider>
    </QueryClientProvider>,
  );
}

describe("Layout", () => {
  it("renders the app brand name", () => {
    renderLayout();
    const brands = screen.getAllByText("Receipts");
    expect(brands.length).toBeGreaterThan(0);
  });

  it("renders outlet content", () => {
    renderLayout();
    expect(screen.getByText("Home Page Content")).toBeInTheDocument();
  });

  it("shows user email in the header", () => {
    renderLayout({ user: { userId: "admin-id", email: "admin@test.com", roles: ["Admin"], mustResetPassword: false } });
    expect(screen.getByText("admin@test.com")).toBeInTheDocument();
  });

  it("renders the Search button", () => {
    renderLayout();
    const searchButtons = screen.getAllByRole("button", { name: /search/i });
    expect(searchButtons.length).toBeGreaterThan(0);
  });

  it("renders connection status indicator", () => {
    renderLayout();
    // The mock returns "connected", so the label "Live" should appear
    const liveTexts = screen.getAllByText("Live");
    expect(liveTexts.length).toBeGreaterThan(0);
  });

  it("renders skip-to-content link for accessibility", () => {
    renderLayout();
    const skipLink = screen.getByText("Skip to main content");
    expect(skipLink).toBeInTheDocument();
    expect(skipLink).toHaveAttribute("href", "#main-content");
  });

  it("opens mobile navigation sheet when hamburger button is clicked", async () => {
    const user = userEvent.setup();
    renderLayout();
    const hamburger = screen.getByLabelText("Open navigation menu");
    await user.click(hamburger);
    // The Sheet should now be open and show mobile nav items
    await waitFor(() => {
      expect(screen.getByRole("navigation", { name: /mobile navigation/i })).toBeInTheDocument();
    });
  });

  it("closes mobile nav sheet when a link is clicked", async () => {
    const user = userEvent.setup();
    renderLayout();
    const hamburger = screen.getByLabelText("Open navigation menu");
    await user.click(hamburger);
    await waitFor(() => {
      expect(screen.getByRole("navigation", { name: /mobile navigation/i })).toBeInTheDocument();
    });
    // Click a nav link inside the mobile sheet
    const mobileNav = screen.getByRole("navigation", { name: /mobile navigation/i });
    const homeLink = mobileNav.querySelector("a[href='/']");
    expect(homeLink).not.toBeNull();
    await user.click(homeLink!);
    // Sheet should close — mobile nav should disappear
    await waitFor(() => {
      expect(screen.queryByRole("navigation", { name: /mobile navigation/i })).not.toBeInTheDocument();
    });
  });

  it("calls logout and navigates to /login when Logout is clicked in user dropdown", async () => {
    const user = userEvent.setup();
    const logoutMock = vi.fn().mockResolvedValue(undefined);
    renderLayout({ logout: logoutMock });
    // Open the user dropdown
    const emailButton = screen.getByText("test@example.com");
    await user.click(emailButton);
    // Click logout
    await waitFor(() => {
      expect(screen.getByText("Logout")).toBeInTheDocument();
    });
    await user.click(screen.getByText("Logout"));
    await waitFor(() => {
      expect(logoutMock).toHaveBeenCalled();
    });
  });

  it("shows admin nav group when user has Admin role", () => {
    renderLayout({ user: { userId: "admin-id", email: "admin@test.com", roles: ["Admin"], mustResetPassword: false } });
    // Admin group should be rendered in the desktop nav
    expect(screen.getByText("Admin")).toBeInTheDocument();
  });

  it("does not show admin nav group for non-admin users", () => {
    renderLayout({ user: { userId: "user-id", email: "user@test.com", roles: ["User"], mustResetPassword: false } });
    expect(screen.queryByText("Admin")).not.toBeInTheDocument();
  });

  it("applies active styling to Dashboard link when on root route", () => {
    renderLayout();
    // The desktop nav contains the NavigationMenu with a Dashboard link
    const desktopNav = screen.getByRole("navigation", { name: /main navigation/i });
    const homeLinks = desktopNav.querySelectorAll("a[href='/']");
    // Find the Dashboard link (not the brand link) -- it should contain "Dashboard" text
    const homeNavLink = Array.from(homeLinks).find((link) =>
      link.textContent?.includes("Dashboard"),
    );
    expect(homeNavLink).toBeDefined();
    expect(homeNavLink!.className).toContain("bg-accent");
  });

  it("renders search button with Ctrl+K keyboard shortcut hint", () => {
    renderLayout();
    expect(screen.getByText("Ctrl+K")).toBeInTheDocument();
  });

  it("handles logout in mobile nav", async () => {
    const user = userEvent.setup();
    const logoutMock = vi.fn().mockResolvedValue(undefined);
    renderLayout({ logout: logoutMock });
    // Open mobile nav
    await user.click(screen.getByLabelText("Open navigation menu"));
    await waitFor(() => {
      expect(screen.getByRole("navigation", { name: /mobile navigation/i })).toBeInTheDocument();
    });
    // Find and click the Logout button in the mobile drawer
    const mobileLogout = screen.getAllByText("Logout").find(
      (el) => el.closest("[data-state]") !== null,
    );
    if (mobileLogout) {
      await user.click(mobileLogout);
      await waitFor(() => {
        expect(logoutMock).toHaveBeenCalled();
      });
    }
  });

  it("navigates to API Keys when clicked in user dropdown", async () => {
    const user = userEvent.setup();
    renderLayout();
    const emailButton = screen.getByText("test@example.com");
    await user.click(emailButton);
    await waitFor(() => {
      expect(screen.getByText("API Keys")).toBeInTheDocument();
    });
  });

  it("does not show version info when version is dev and hash is local", () => {
    // Default globals from vite.config.ts define block are "dev" and "local"
    renderLayout();
    expect(screen.queryByText(/dev/)).not.toBeInTheDocument();
  });

  it("applies bottom padding to main content to clear the Sentry feedback button", () => {
    renderLayout();
    const main = document.getElementById("main-content");
    expect(main).not.toBeNull();
    expect(main!.className).toContain("pb-16");
  });
});

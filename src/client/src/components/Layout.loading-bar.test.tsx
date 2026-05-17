/**
 * Focused tests for the route-transition loading bar in Layout.
 * These are in a separate file so that the react-router module mock
 * does not interfere with the real router used in Layout.test.tsx.
 */
import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import * as ReactRouter from "react-router";
import { MemoryRouter } from "react-router";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthContext, type AuthContextValue } from "@/contexts/auth-context";
import {
  ShortcutsContext,
  type ShortcutsContextValue,
} from "@/contexts/shortcuts-context";
import { Layout } from "./Layout";

// Mock only the hooks we need to control, leave the rest (including Link, MemoryRouter) real
vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof import("react-router")>();
  return {
    ...actual,
    useNavigation: vi.fn(() => ({ state: "idle" as const })),
    useNavigate: vi.fn(() => vi.fn()),
    useLocation: vi.fn(() => ({ pathname: "/", search: "", hash: "", state: null, key: "default" })),
    useNavigationType: vi.fn(() => "POP" as const),
    Outlet: vi.fn(() => <div>Outlet</div>),
    useMatches: vi.fn(() => []),
  };
});

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

vi.mock("@/components/CommandPalette", () => ({
  CommandPalette: () => <div data-testid="command-palette" />,
}));

vi.mock("@/components/Breadcrumbs", () => ({
  Breadcrumbs: () => <nav aria-label="breadcrumbs" />,
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

function renderLayout() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false, gcTime: 0 } },
  });
  return render(
    <MemoryRouter initialEntries={["/"]}>
      <QueryClientProvider client={queryClient}>
        <AuthContext.Provider value={defaultAuth}>
          <ShortcutsContext.Provider value={defaultShortcuts}>
            <Layout />
          </ShortcutsContext.Provider>
        </AuthContext.Provider>
      </QueryClientProvider>
    </MemoryRouter>,
  );
}

describe("Layout – route-transition loading bar", () => {
  it("does not render the loading bar when navigation is idle", () => {
    vi.mocked(ReactRouter.useNavigation).mockReturnValue({ state: "idle" } as ReturnType<typeof ReactRouter.useNavigation>);
    renderLayout();
    // No aria-busy status element should be present from the loading bar
    const statuses = screen.queryAllByRole("status");
    const loadingBar = statuses.find((el) => el.getAttribute("aria-busy") === "true");
    expect(loadingBar).toBeUndefined();
  });

  it("renders the loading bar with role=status and aria-live=polite when navigating", () => {
    vi.mocked(ReactRouter.useNavigation).mockReturnValue({ state: "loading" } as ReturnType<typeof ReactRouter.useNavigation>);
    renderLayout();
    const statuses = screen.queryAllByRole("status");
    const loadingBar = statuses.find((el) => el.getAttribute("aria-busy") === "true");
    expect(loadingBar).toBeDefined();
    expect(loadingBar).toHaveAttribute("aria-live", "polite");
  });

  it("renders sr-only 'Loading page…' text when navigating", () => {
    vi.mocked(ReactRouter.useNavigation).mockReturnValue({ state: "loading" } as ReturnType<typeof ReactRouter.useNavigation>);
    renderLayout();
    expect(screen.getByText("Loading page…")).toBeInTheDocument();
    expect(screen.getByText("Loading page…")).toHaveClass("sr-only");
  });

  it("marks the decorative progress bar container as aria-hidden", () => {
    vi.mocked(ReactRouter.useNavigation).mockReturnValue({ state: "loading" } as ReturnType<typeof ReactRouter.useNavigation>);
    renderLayout();
    const statuses = screen.queryAllByRole("status");
    const loadingStatus = statuses.find((el) => el.getAttribute("aria-busy") === "true");
    const decorativeBar = loadingStatus?.querySelector("[aria-hidden='true']");
    expect(decorativeBar).toBeInTheDocument();
  });
});

import {
  ArrowRightLeft,
  BarChart3,
  Building2,
  Camera,
  ChartColumn,
  CreditCard,
  Database,
  FolderTree,
  Home,
  Key,
  KeyRound,
  LayoutList,
  LogOut,
  Moon,
  Package,
  Receipt,
  RotateCcw,
  ScanSearch,
  ScrollText,
  Search,
  Shield,
  Sparkles,
  Sun,
  SunMoon,
  Tag,
  Tags,
  Trash2,
  TrendingUp,
  UserPlus,
  Users,
} from "lucide-react";
import type { Command } from "./types";

/**
 * Dispatches the app's existing per-page "open new-item dialog" handler.
 * If already on the target page, dispatch immediately; otherwise navigate
 * first and dispatch after a short delay so the target page's effect has
 * mounted and registered its listener.
 */
function runNewItem(
  targetPath: string,
  {
    navigate,
    close,
    currentPath,
  }: {
    navigate: (path: string) => void;
    close: () => void;
    currentPath: string;
  },
) {
  close();
  if (currentPath === targetPath) {
    window.dispatchEvent(new CustomEvent("shortcut:new-item"));
    return;
  }
  navigate(targetPath);
  setTimeout(() => {
    window.dispatchEvent(new CustomEvent("shortcut:new-item"));
  }, 50);
}

function goTo(path: string) {
  return (ctx: { navigate: (path: string) => void; close: () => void }) => {
    ctx.close();
    ctx.navigate(path);
  };
}

export const REPORT_COMMANDS: Array<{ slug: string; name: string }> = [
  { slug: "out-of-balance", name: "Out of Balance" },
  { slug: "item-similarity", name: "Item Similarity" },
  { slug: "item-cost-over-time", name: "Item Cost Over Time" },
  { slug: "spending-by-location", name: "Spending by Location" },
  { slug: "category-trends", name: "Category Trends" },
  { slug: "duplicate-detection", name: "Duplicate Detection" },
  { slug: "uncategorized-items", name: "Uncategorized Items" },
];

export const COMMANDS: Command[] = [
  // ---- Create ----
  {
    id: "create:receipt",
    label: "New Receipt",
    group: "create",
    icon: Receipt,
    keywords: ["add", "create"],
    targetPath: "/receipts/new",
    run: goTo("/receipts/new"),
  },
  {
    id: "create:receipt-scan",
    label: "Scan Receipt",
    group: "create",
    icon: Camera,
    keywords: ["photo", "camera", "ocr", "upload"],
    targetPath: "/receipts/scan",
    run: goTo("/receipts/scan"),
  },
  {
    id: "create:account",
    label: "New Account",
    group: "create",
    icon: Building2,
    keywords: ["add", "create"],
    targetPath: "/accounts",
    run: (ctx) => runNewItem("/accounts", ctx),
  },
  {
    id: "create:card",
    label: "New Card",
    group: "create",
    icon: CreditCard,
    keywords: ["add", "create", "payment"],
    targetPath: "/cards",
    run: (ctx) => runNewItem("/cards", ctx),
  },
  {
    id: "create:category",
    label: "New Category",
    group: "create",
    icon: Tag,
    keywords: ["add", "create"],
    targetPath: "/categories",
    run: (ctx) => runNewItem("/categories", ctx),
  },
  {
    id: "create:subcategory",
    label: "New Subcategory",
    group: "create",
    icon: Tags,
    keywords: ["add", "create"],
    targetPath: "/subcategories",
    run: (ctx) => runNewItem("/subcategories", ctx),
  },
  {
    id: "create:item-template",
    label: "New Item Template",
    group: "create",
    icon: Package,
    keywords: ["add", "create", "autocomplete"],
    targetPath: "/item-templates",
    run: (ctx) => runNewItem("/item-templates", ctx),
  },
  {
    id: "create:api-key",
    label: "New API Key",
    group: "create",
    icon: Key,
    keywords: ["add", "create", "token", "access"],
    targetPath: "/api-keys",
    run: (ctx) => runNewItem("/api-keys", ctx),
  },
  {
    id: "create:user",
    label: "New User",
    group: "create",
    icon: UserPlus,
    keywords: ["add", "create", "invite"],
    targetPath: "/admin/users",
    requiresAdmin: true,
    run: (ctx) => runNewItem("/admin/users", ctx),
  },

  // ---- Navigate ----
  {
    id: "nav:dashboard",
    label: "Go to Dashboard",
    group: "navigate",
    icon: Home,
    keywords: ["home"],
    run: goTo("/"),
  },
  {
    id: "nav:receipts",
    label: "Go to Receipts",
    group: "navigate",
    icon: Receipt,
    run: goTo("/receipts"),
  },
  {
    id: "nav:reports",
    label: "Go to Reports",
    group: "navigate",
    icon: ChartColumn,
    keywords: ["analytics", "charts"],
    run: goTo("/reports"),
  },
  {
    id: "nav:accounts",
    label: "Go to Accounts",
    group: "navigate",
    icon: Building2,
    run: goTo("/accounts"),
  },
  {
    id: "nav:cards",
    label: "Go to Cards",
    group: "navigate",
    icon: CreditCard,
    run: goTo("/cards"),
  },
  {
    id: "nav:categories",
    label: "Go to Categories",
    group: "navigate",
    icon: Tag,
    run: goTo("/categories"),
  },
  {
    id: "nav:subcategories",
    label: "Go to Subcategories",
    group: "navigate",
    icon: Tags,
    run: goTo("/subcategories"),
  },
  {
    id: "nav:item-templates",
    label: "Go to Item Templates",
    group: "navigate",
    icon: Package,
    keywords: ["autocomplete"],
    run: goTo("/item-templates"),
  },
  {
    id: "nav:security",
    label: "Go to Security",
    group: "navigate",
    icon: Shield,
    keywords: ["sessions", "login"],
    run: goTo("/security"),
  },
  {
    id: "nav:api-keys",
    label: "Go to API Keys",
    group: "navigate",
    icon: Key,
    keywords: ["token", "access"],
    run: goTo("/api-keys"),
  },
  {
    id: "nav:ynab",
    label: "Go to YNAB Settings",
    group: "navigate",
    icon: ArrowRightLeft,
    keywords: ["sync", "budget"],
    run: goTo("/settings/ynab"),
  },
  {
    id: "nav:change-password",
    label: "Change Password",
    group: "navigate",
    icon: KeyRound,
    keywords: ["password", "security"],
    run: goTo("/change-password"),
  },
  {
    id: "nav:users",
    label: "Go to User Management",
    group: "navigate",
    icon: Users,
    keywords: ["admin", "accounts"],
    requiresAdmin: true,
    run: goTo("/admin/users"),
  },
  {
    id: "nav:audit",
    label: "Go to Audit Log",
    group: "navigate",
    icon: ScrollText,
    keywords: ["history", "admin"],
    requiresAdmin: true,
    run: goTo("/audit"),
  },
  {
    id: "nav:trash",
    label: "Go to Trash",
    group: "navigate",
    icon: Trash2,
    keywords: ["recycle", "deleted", "admin"],
    requiresAdmin: true,
    run: goTo("/trash"),
  },
  {
    id: "nav:backup",
    label: "Go to Backup & Restore",
    group: "navigate",
    icon: Database,
    keywords: ["export", "import", "admin"],
    requiresAdmin: true,
    run: goTo("/admin/backup"),
  },

  // ---- Preferences ----
  {
    id: "pref:shortcuts-help",
    label: "Show Keyboard Shortcuts",
    group: "preferences",
    icon: LayoutList,
    keywords: ["help", "keys"],
    shortcut: "?",
    run: (ctx) => {
      ctx.close();
      ctx.openShortcutsHelp();
    },
  },
  {
    id: "pref:theme-light",
    label: "Light Theme",
    group: "preferences",
    icon: Sun,
    keywords: ["appearance", "color", "mode"],
    run: (ctx) => {
      ctx.close();
      ctx.setTheme("light");
    },
  },
  {
    id: "pref:theme-dark",
    label: "Dark Theme",
    group: "preferences",
    icon: Moon,
    keywords: ["appearance", "color", "mode"],
    run: (ctx) => {
      ctx.close();
      ctx.setTheme("dark");
    },
  },
  {
    id: "pref:theme-system",
    label: "System Theme",
    group: "preferences",
    icon: SunMoon,
    keywords: ["appearance", "color", "mode"],
    run: (ctx) => {
      ctx.close();
      ctx.setTheme("system");
    },
  },
  {
    id: "pref:sign-out",
    label: "Sign Out",
    group: "preferences",
    icon: LogOut,
    keywords: ["logout", "exit"],
    run: async (ctx) => {
      ctx.close();
      try {
        await ctx.logout();
      } catch (err) {
        console.error("Logout failed; navigating to login anyway.", err);
      }
      ctx.navigate("/login");
    },
  },

  // ---- Reports (uses ?report= slug) ----
  ...REPORT_COMMANDS.map<Command>((report) => ({
    id: `report:${report.slug}`,
    label: `Open "${report.name}" Report`,
    group: "reports",
    icon:
      report.slug === "duplicate-detection"
        ? ScanSearch
        : report.slug === "item-similarity"
          ? Sparkles
          : report.slug === "item-cost-over-time"
            ? TrendingUp
            : report.slug === "uncategorized-items"
              ? FolderTree
              : report.slug === "spending-by-location"
                ? Search
                : report.slug === "out-of-balance"
                  ? RotateCcw
                  : BarChart3,
    keywords: ["report", "analytics", "chart"],
    run: goTo(`/reports?report=${report.slug}`),
  })),
];


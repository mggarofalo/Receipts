/**
 * Design-system icon set (RECEIPTS-592 / Phase 18).
 *
 * Option B from the epic: the 30 named icons map onto `lucide-react` with a
 * `strokeWidth` of 1.6 to match the shell's stroke vocabulary. Consumers use
 * `Icon.<Name>` rather than importing lucide directly so the mapping stays in
 * one place and the stroke weight is consistent.
 *
 * Bundle name → lucide-react component:
 *   Dashboard      → LayoutDashboard      Copy           → Copy
 *   Receipt        → Receipt              Edit           → Pencil
 *   Scan           → ScanLine             Trash          → Trash2
 *   Chart          → ChartColumn          Check          → Check
 *   Tag            → Tag                  X              → X
 *   Wallet         → Wallet               Filter         → Filter
 *   Card           → CreditCard           Calendar       → Calendar
 *   Link           → Link                 AlertTriangle  → TriangleAlert
 *   Settings       → Settings             Info           → Info
 *   Users          → Users                Command        → Command
 *   Plus           → Plus                 Upload         → Upload
 *   Search         → Search               Camera         → Camera
 *   Arrow          → ArrowRight           Inbox          → Inbox
 *   Sparkle        → Sparkles             Sliders        → SlidersHorizontal
 *   Clock          → Clock                ChevronR       → ChevronRight
 *   ChevronD       → ChevronDown
 *
 * Default render sizes by context: 15px for nav/buttons, 16px for the command
 * palette, 18px for the warning banner. Pass `size`/`className` to override.
 */
import { forwardRef } from "react";
import {
  ArrowRight as LArrowRight,
  Calendar as LCalendar,
  Camera as LCamera,
  ChartColumn as LChartColumn,
  Check as LCheck,
  ChevronDown as LChevronDown,
  ChevronRight as LChevronRight,
  Clock as LClock,
  Command as LCommand,
  Copy as LCopy,
  CreditCard as LCreditCard,
  Filter as LFilter,
  Inbox as LInbox,
  Info as LInfo,
  LayoutDashboard as LLayoutDashboard,
  Link as LLink,
  Pencil as LPencil,
  Plus as LPlus,
  Receipt as LReceipt,
  ScanLine as LScanLine,
  Search as LSearch,
  Settings as LSettings,
  SlidersHorizontal as LSlidersHorizontal,
  Sparkles as LSparkles,
  Tag as LTag,
  Trash2 as LTrash2,
  TriangleAlert as LTriangleAlert,
  Upload as LUpload,
  Users as LUsers,
  Wallet as LWallet,
  X as LX,
  type LucideIcon,
  type LucideProps,
} from "lucide-react";

/** Wrap a lucide icon so it defaults to the design system's 1.6 stroke. */
function designIcon(Base: LucideIcon, name: string) {
  const Wrapped = forwardRef<SVGSVGElement, LucideProps>(
    ({ strokeWidth = 1.6, ...props }, ref) => (
      <Base ref={ref} strokeWidth={strokeWidth} {...props} />
    ),
  );
  Wrapped.displayName = `Icon.${name}`;
  return Wrapped;
}

export type IconComponent = ReturnType<typeof designIcon>;

export const Icon = {
  Dashboard: designIcon(LLayoutDashboard, "Dashboard"),
  Receipt: designIcon(LReceipt, "Receipt"),
  Scan: designIcon(LScanLine, "Scan"),
  Chart: designIcon(LChartColumn, "Chart"),
  Tag: designIcon(LTag, "Tag"),
  Wallet: designIcon(LWallet, "Wallet"),
  Card: designIcon(LCreditCard, "Card"),
  Link: designIcon(LLink, "Link"),
  Settings: designIcon(LSettings, "Settings"),
  Users: designIcon(LUsers, "Users"),
  Plus: designIcon(LPlus, "Plus"),
  Search: designIcon(LSearch, "Search"),
  Arrow: designIcon(LArrowRight, "Arrow"),
  Copy: designIcon(LCopy, "Copy"),
  Edit: designIcon(LPencil, "Edit"),
  Trash: designIcon(LTrash2, "Trash"),
  Check: designIcon(LCheck, "Check"),
  X: designIcon(LX, "X"),
  Filter: designIcon(LFilter, "Filter"),
  Calendar: designIcon(LCalendar, "Calendar"),
  AlertTriangle: designIcon(LTriangleAlert, "AlertTriangle"),
  Info: designIcon(LInfo, "Info"),
  Command: designIcon(LCommand, "Command"),
  Upload: designIcon(LUpload, "Upload"),
  Camera: designIcon(LCamera, "Camera"),
  Inbox: designIcon(LInbox, "Inbox"),
  Sparkle: designIcon(LSparkles, "Sparkle"),
  Sliders: designIcon(LSlidersHorizontal, "Sliders"),
  Clock: designIcon(LClock, "Clock"),
  ChevronR: designIcon(LChevronRight, "ChevronR"),
  ChevronD: designIcon(LChevronDown, "ChevronD"),
} as const;

export type IconName = keyof typeof Icon;

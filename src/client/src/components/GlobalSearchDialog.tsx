import { useCallback } from "react";
import { useNavigate } from "react-router";
import {
  Building2,
  FileText,
  Package,
  ArrowRightLeft,
  Home,
} from "lucide-react";
import {
  CommandDialog,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
  CommandSeparator,
} from "@/components/ui/command";
import { useKeyboardShortcut } from "@/hooks/useKeyboardShortcut";
import { formatCurrency } from "@/lib/format";
import { useCards } from "@/hooks/useCards";
import { useReceipts } from "@/hooks/useReceipts";
import { useReceiptItems } from "@/hooks/useReceiptItems";
import { useTransactions } from "@/hooks/useTransactions";

interface GlobalSearchDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

const navItems = [
  { label: "Home", path: "/", icon: Home },
  { label: "Cards", path: "/cards", icon: Building2 },
  { label: "Receipts", path: "/receipts", icon: FileText },
];

export function GlobalSearchDialog({
  open,
  onOpenChange,
}: GlobalSearchDialogProps) {
  const navigate = useNavigate();
  const { data: cards } = useCards();
  const { data: receipts } = useReceipts();
  const { data: receiptItems } = useReceiptItems();
  const { data: transactions } = useTransactions();

  const toggleOpen = useCallback(() => {
    onOpenChange(!open);
  }, [open, onOpenChange]);

  useKeyboardShortcut({ key: "k", handler: toggleOpen });

  function select(path: string) {
    onOpenChange(false);
    navigate(path);
  }

  return (
    <CommandDialog open={open} onOpenChange={onOpenChange}>
      <CommandInput placeholder="Type to search..." />
      <CommandList>
        <CommandEmpty>No results found.</CommandEmpty>

        <CommandGroup heading="Navigation">
          {navItems.map((item) => (
            <CommandItem
              key={item.path}
              value={`nav:${item.label}`}
              onSelect={() => select(item.path)}
            >
              <item.icon className="mr-2 h-4 w-4" />
              {item.label}
            </CommandItem>
          ))}
        </CommandGroup>

        {cards?.length ? (
          <>
            <CommandSeparator />
            <CommandGroup heading="Cards">
              {(cards ?? []).slice(0, 8).map((a) => (
                <CommandItem
                  key={a.id}
                  value={`card:${a.name} ${a.cardCode}`}
                  onSelect={() => select("/cards")}
                >
                  <Building2 className="mr-2 h-4 w-4 text-muted-foreground" />
                  <span className="font-medium">{a.name}</span>
                  <span className="ml-2 font-mono text-xs text-muted-foreground">
                    {a.cardCode}
                  </span>
                </CommandItem>
              ))}
            </CommandGroup>
          </>
        ) : null}

        {receipts?.length ? (
          <>
            <CommandSeparator />
            <CommandGroup heading="Receipts">
              {(receipts ?? []).slice(0, 8).map((r) => (
                <CommandItem
                  key={r.id}
                  value={`receipt:${r.location}`}
                  onSelect={() => select("/receipts")}
                >
                  <FileText className="mr-2 h-4 w-4 text-muted-foreground" />
                  <span>{r.location}</span>
                </CommandItem>
              ))}
            </CommandGroup>
          </>
        ) : null}

        {receiptItems?.length ? (
          <>
            <CommandSeparator />
            <CommandGroup heading="Receipt Items">
              {(receiptItems ?? []).slice(0, 8).map((i) => (
                <CommandItem
                  key={i.id}
                  value={`item:${i.description} ${i.receiptItemCode} ${i.category}`}
                  onSelect={() => select(`/receipts/${i.receiptId}`)}
                >
                  <Package className="mr-2 h-4 w-4 text-muted-foreground" />
                  <span>{i.description}</span>
                  <span className="ml-2 font-mono text-xs text-muted-foreground">
                    {i.receiptItemCode}
                  </span>
                </CommandItem>
              ))}
            </CommandGroup>
          </>
        ) : null}

        {transactions?.length ? (
          <>
            <CommandSeparator />
            <CommandGroup heading="Transactions">
              {(transactions ?? []).slice(0, 8).map((t) => (
                <CommandItem
                  key={t.id}
                  value={`txn:${t.date} ${t.amount}`}
                  onSelect={() => select(`/receipts/${t.receiptId}`)}
                >
                  <ArrowRightLeft className="mr-2 h-4 w-4 text-muted-foreground" />
                  <span>{formatCurrency(Number(t.amount ?? 0))}</span>
                  <span className="ml-2 text-xs text-muted-foreground">
                    {t.date}
                  </span>
                </CommandItem>
              ))}
            </CommandGroup>
          </>
        ) : null}
      </CommandList>
    </CommandDialog>
  );
}

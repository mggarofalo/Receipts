import { useCallback } from "react";
import { useNavigate } from "react-router";
import {
  Building2,
  FileText,
  Package,
  ArrowRightLeft,
  Plane,
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
import { useAccounts } from "@/hooks/useAccounts";
import { useReceipts } from "@/hooks/useReceipts";
import { useReceiptItems } from "@/hooks/useReceiptItems";
import { useTransactions } from "@/hooks/useTransactions";

interface AccountResponse {
  id: string;
  accountCode: string;
  name: string;
}

interface ReceiptResponse {
  id: string;
  description?: string | null;
  location: string;
}

interface ReceiptItemResponse {
  id: string;
  receiptItemCode: string;
  description: string;
  category: string;
}

interface TransactionResponse {
  id: string;
  amount: number;
  date: string;
}

interface GlobalSearchDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

const navItems = [
  { label: "Home", path: "/", icon: Home },
  { label: "Accounts", path: "/accounts", icon: Building2 },
  { label: "Receipts", path: "/receipts", icon: FileText },
  { label: "Receipt Items", path: "/receipt-items", icon: Package },
  { label: "Transactions", path: "/transactions", icon: ArrowRightLeft },
  { label: "Trips", path: "/trips", icon: Plane },
];

export function GlobalSearchDialog({
  open,
  onOpenChange,
}: GlobalSearchDialogProps) {
  const navigate = useNavigate();
  const { data: accountsResponse } = useAccounts();
  const { data: receiptsResponse } = useReceipts();
  const { data: receiptItemsResponse } = useReceiptItems();
  const { data: transactionsResponse } = useTransactions();

  const accounts = accountsResponse?.data as AccountResponse[] | undefined;
  const receipts = receiptsResponse?.data as ReceiptResponse[] | undefined;
  const receiptItems = receiptItemsResponse?.data as ReceiptItemResponse[] | undefined;
  const transactions = transactionsResponse?.data as TransactionResponse[] | undefined;

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

        {accounts?.length ? (
          <>
            <CommandSeparator />
            <CommandGroup heading="Accounts">
              {(accounts ?? []).slice(0, 8).map((a) => (
                <CommandItem
                  key={a.id}
                  value={`account:${a.name} ${a.accountCode}`}
                  onSelect={() => select("/accounts")}
                >
                  <Building2 className="mr-2 h-4 w-4 text-muted-foreground" />
                  <span className="font-medium">{a.name}</span>
                  <span className="ml-2 font-mono text-xs text-muted-foreground">
                    {a.accountCode}
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
                  value={`receipt:${r.description ?? ""} ${r.location}`}
                  onSelect={() => select("/receipts")}
                >
                  <FileText className="mr-2 h-4 w-4 text-muted-foreground" />
                  <span>{r.description || r.location}</span>
                  {r.description && (
                    <span className="ml-2 text-xs text-muted-foreground">
                      {r.location}
                    </span>
                  )}
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
                  onSelect={() => select("/receipt-items")}
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
                  onSelect={() => select("/transactions")}
                >
                  <ArrowRightLeft className="mr-2 h-4 w-4 text-muted-foreground" />
                  <span>{formatCurrency(t.amount)}</span>
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

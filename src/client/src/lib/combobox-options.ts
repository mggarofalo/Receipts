import type { ComboboxOption } from "@/components/ui/combobox";

interface AccountLike {
  id: string;
  name: string;
  accountCode: string;
}

interface ReceiptLike {
  id: string;
  description?: string | null;
  location: string;
  date: string;
}

export function accountToOption(account: AccountLike): ComboboxOption {
  return {
    value: account.id,
    label: account.name,
    sublabel: account.accountCode,
  };
}

export function receiptToOption(receipt: ReceiptLike): ComboboxOption {
  return {
    value: receipt.id,
    label: receipt.description || receipt.location,
    sublabel: `${receipt.location} — ${receipt.date}`,
  };
}

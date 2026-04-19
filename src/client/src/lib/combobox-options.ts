import type { ComboboxOption } from "@/components/ui/combobox";

interface CardLike {
  id: string;
  name: string;
  cardCode: string;
}

interface AccountLike {
  id: string;
  name: string;
}

interface ReceiptLike {
  id: string;
  location: string;
  date: string;
}

export function cardToOption(card: CardLike): ComboboxOption {
  return {
    value: card.id,
    label: card.name,
    sublabel: card.cardCode,
  };
}

export function accountToOption(account: AccountLike): ComboboxOption {
  return {
    value: account.id,
    label: account.name,
  };
}

export function receiptToOption(receipt: ReceiptLike): ComboboxOption {
  return {
    value: receipt.id,
    label: receipt.location,
    sublabel: `${receipt.location} — ${receipt.date}`,
  };
}

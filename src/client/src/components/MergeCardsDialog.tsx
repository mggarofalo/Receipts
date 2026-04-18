import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { AlertCircle } from "lucide-react";
import {
  useMergeCards,
  isMergeCardsConflict,
  type MergeCardsConflict,
  type YnabMappingConflict,
} from "@/hooks/useCards";
import { useAccounts, useCreateAccount } from "@/hooks/useAccounts";

export interface SelectedCardSummary {
  id: string;
  name: string;
  cardCode: string;
}

interface MergeCardsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  selectedCards: SelectedCardSummary[];
  onMergeComplete?: () => void;
}

type TargetMode = "existing" | "new";

export function MergeCardsDialog({
  open,
  onOpenChange,
  selectedCards,
  onMergeComplete,
}: MergeCardsDialogProps) {
  const [targetMode, setTargetMode] = useState<TargetMode>("existing");
  const [targetAccountId, setTargetAccountId] = useState<string>("");
  const [newAccountName, setNewAccountName] = useState<string>("");
  const [conflict, setConflict] = useState<MergeCardsConflict | null>(null);
  const [winnerAccountId, setWinnerAccountId] = useState<string | null>(null);

  const { data: accountsData } = useAccounts(0, 500, "name", "asc", true);
  const createAccount = useCreateAccount();
  const mergeCards = useMergeCards();

  function handleOpenChange(next: boolean) {
    if (!next) {
      setTargetMode("existing");
      setTargetAccountId("");
      setNewAccountName("");
      setConflict(null);
      setWinnerAccountId(null);
    }
    onOpenChange(next);
  }

  const accounts = accountsData ?? [];

  const isSubmitDisabled =
    selectedCards.length < 2 ||
    (targetMode === "existing" && !targetAccountId) ||
    (targetMode === "new" && newAccountName.trim().length === 0) ||
    mergeCards.isPending ||
    createAccount.isPending ||
    (conflict !== null && !winnerAccountId);

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();

    let resolvedTargetId = targetAccountId;

    if (targetMode === "new") {
      const created = await createAccount.mutateAsync({
        name: newAccountName.trim(),
        isActive: true,
      });
      if (!created?.id) return;
      resolvedTargetId = created.id;
    }

    try {
      await mergeCards.mutateAsync({
        targetAccountId: resolvedTargetId,
        sourceCardIds: selectedCards.map((c) => c.id),
        ynabMappingWinnerAccountId: winnerAccountId,
      });
      handleOpenChange(false);
      onMergeComplete?.();
    } catch (err) {
      if (isMergeCardsConflict(err)) {
        setConflict(err);
        if (targetMode === "new") {
          setTargetAccountId(resolvedTargetId);
          setTargetMode("existing");
        }
        return;
      }
      // non-conflict errors are surfaced via toast in the hook
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Merge cards into account</DialogTitle>
          <DialogDescription>
            Repoints the selected cards and their transactions to the target
            account. Any source accounts left without cards will be removed.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1">
            <div className="text-sm font-medium">
              Merging {selectedCards.length} card
              {selectedCards.length === 1 ? "" : "s"}
            </div>
            <ul className="rounded-md border bg-muted/30 p-2 text-sm max-h-32 overflow-y-auto">
              {selectedCards.map((card) => (
                <li key={card.id} className="py-0.5">
                  <span className="font-mono text-xs">{card.cardCode}</span>{" "}
                  <span>{card.name}</span>
                </li>
              ))}
            </ul>
          </div>

          <fieldset className="space-y-2" disabled={conflict !== null}>
            <legend className="text-sm font-medium">Target account</legend>
            <div className="flex items-center gap-4 text-sm">
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="target-mode"
                  value="existing"
                  checked={targetMode === "existing"}
                  onChange={() => setTargetMode("existing")}
                />
                <span>Existing account</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="target-mode"
                  value="new"
                  checked={targetMode === "new"}
                  onChange={() => setTargetMode("new")}
                />
                <span>New account</span>
              </label>
            </div>

            {targetMode === "existing" ? (
              <div className="space-y-1">
                <Label htmlFor="target-account">Select account</Label>
                <Select value={targetAccountId} onValueChange={setTargetAccountId}>
                  <SelectTrigger id="target-account" aria-label="Target account">
                    <SelectValue placeholder="Choose an account" />
                  </SelectTrigger>
                  <SelectContent>
                    {accounts.map((a) => (
                      <SelectItem key={a.id} value={a.id}>
                        {a.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            ) : (
              <div className="space-y-1">
                <Label htmlFor="new-account-name">New account name</Label>
                <Input
                  id="new-account-name"
                  value={newAccountName}
                  onChange={(e) => setNewAccountName(e.target.value)}
                  placeholder="e.g. Apple Card"
                />
              </div>
            )}
          </fieldset>

          {conflict && (
            <Alert variant="destructive">
              <AlertCircle className="h-4 w-4" />
              <AlertDescription>
                <div className="font-medium mb-2">YNAB mapping conflict</div>
                <p className="text-sm mb-2">{conflict.message}</p>
                <fieldset className="space-y-1">
                  <legend className="text-sm font-medium mb-1">
                    Keep which mapping?
                  </legend>
                  {conflict.conflicts.map((c: YnabMappingConflict) => (
                    <label key={c.accountId} className="flex items-start gap-2 text-sm">
                      <input
                        type="radio"
                        name="mapping-winner"
                        value={c.accountId}
                        checked={winnerAccountId === c.accountId}
                        onChange={() => setWinnerAccountId(c.accountId)}
                        className="mt-1"
                      />
                      <span>
                        <span className="font-medium">{c.accountName || "(target)"}</span>
                        {" → "}
                        <span className="font-mono text-xs">{c.ynabAccountName}</span>
                      </span>
                    </label>
                  ))}
                </fieldset>
              </AlertDescription>
            </Alert>
          )}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => handleOpenChange(false)}
              disabled={mergeCards.isPending}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={isSubmitDisabled}>
              {mergeCards.isPending
                ? "Merging..."
                : conflict
                  ? "Resubmit with resolution"
                  : "Merge"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

import {
  usePushYnabTransactions,
  type ReceiptYnabSyncStatusValue,
} from "@/hooks/useYnab";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { YnabSyncBadge } from "@/components/YnabSyncBadge";
import { Alert, AlertDescription } from "@/components/ui/alert";

interface YnabPushButtonProps {
  receiptId: string;
  hasTransactions: boolean;
  persistedSyncStatus?: ReceiptYnabSyncStatusValue;
}

export function YnabPushButton({
  receiptId,
  hasTransactions,
  persistedSyncStatus,
}: YnabPushButtonProps) {
  const pushMutation = usePushYnabTransactions();

  const handlePush = () => {
    pushMutation.mutate(receiptId);
  };

  const result = pushMutation.data;
  const mutationSucceeded = result?.success === true;
  const mutationFailed = result != null && result.success === false;

  // Effective status: a fresh mutation result trumps whatever was persisted.
  // Otherwise fall back to the status fetched on page load.
  const effectiveStatus: ReceiptYnabSyncStatusValue | undefined =
    mutationSucceeded
      ? "Synced"
      : mutationFailed
        ? "Failed"
        : persistedSyncStatus;

  const isSynced = effectiveStatus === "Synced";

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-3">
        <Button
          variant="outline"
          size="sm"
          onClick={handlePush}
          disabled={pushMutation.isPending || isSynced || !hasTransactions}
        >
          {pushMutation.isPending ? (
            <>
              <Spinner className="mr-2 h-3 w-3" />
              Pushing to YNAB...
            </>
          ) : mutationSucceeded ? (
            "Pushed to YNAB"
          ) : isSynced ? (
            "Already pushed"
          ) : (
            "Push to YNAB"
          )}
        </Button>

        <YnabSyncBadge status={effectiveStatus} />
      </div>

      {result && !result.success && result.error && (
        <Alert variant="destructive">
          <AlertDescription>{result.error}</AlertDescription>
        </Alert>
      )}

      {result &&
        !result.success &&
        result.unmappedCategories &&
        result.unmappedCategories.length > 0 && (
          <Alert variant="destructive">
            <AlertDescription>
              Unmapped categories:{" "}
              {result.unmappedCategories.join(", ")}. Map them in{" "}
              <a href="/settings/ynab" className="underline">
                YNAB Settings
              </a>
              .
            </AlertDescription>
          </Alert>
        )}

      {mutationSucceeded && result != null && result.pushedTransactions.length > 0 && (
        <div className="text-sm text-muted-foreground">
          {result.pushedTransactions.length} transaction(s) pushed
          {result.pushedTransactions.some((t) => t.subTransactionCount > 1) &&
            " with category splits"}
        </div>
      )}
    </div>
  );
}

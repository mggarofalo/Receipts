import {
  useCreateTransaction,
  useUpdateTransaction,
  useDeleteTransactions,
} from "@/hooks/useTransactions";
import { TransactionForm } from "@/components/TransactionForm";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

interface TransactionResponse {
  id: string;
  receiptId: string;
  accountId: string;
  amount: number;
  date: string;
}

interface TransactionDialogsProps {
  createOpen: boolean;
  onCreateOpenChange: (open: boolean) => void;
  editTransaction: TransactionResponse | null;
  onEditClose: () => void;
  deleteOpen: boolean;
  onDeleteOpenChange: (open: boolean) => void;
  selectedIds: string[];
  onDeleteComplete: () => void;
}

export function TransactionDialogs({
  createOpen,
  onCreateOpenChange,
  editTransaction,
  onEditClose,
  deleteOpen,
  onDeleteOpenChange,
  selectedIds,
  onDeleteComplete,
}: TransactionDialogsProps) {
  const createTransaction = useCreateTransaction();
  const updateTransaction = useUpdateTransaction();
  const deleteTransactions = useDeleteTransactions();

  return (
    <>
      <Dialog open={createOpen} onOpenChange={onCreateOpenChange}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Transaction</DialogTitle>
          </DialogHeader>
          <TransactionForm
            mode="create"
            isSubmitting={createTransaction.isPending}
            onCancel={() => onCreateOpenChange(false)}
            onSubmit={(values) => {
              createTransaction.mutate(
                {
                  receiptId: values.receiptId,
                  accountId: values.accountId,
                  body: { amount: values.amount, date: values.date },
                },
                { onSuccess: () => onCreateOpenChange(false) },
              );
            }}
          />
        </DialogContent>
      </Dialog>

      <Dialog
        open={editTransaction !== null}
        onOpenChange={(open) => {
          if (!open) onEditClose();
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Transaction</DialogTitle>
          </DialogHeader>
          {editTransaction && (
            <TransactionForm
              mode="edit"
              defaultValues={{
                receiptId: editTransaction.receiptId,
                accountId: editTransaction.accountId,
                amount: editTransaction.amount,
                date: editTransaction.date,
              }}
              isSubmitting={updateTransaction.isPending}
              onCancel={onEditClose}
              onSubmit={(values) => {
                updateTransaction.mutate(
                  {
                    receiptId: values.receiptId,
                    accountId: values.accountId,
                    body: {
                      id: editTransaction.id,
                      amount: values.amount,
                      date: values.date,
                    },
                  },
                  { onSuccess: onEditClose },
                );
              }}
            />
          )}
        </DialogContent>
      </Dialog>

      <Dialog open={deleteOpen} onOpenChange={onDeleteOpenChange}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Transactions</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selectedIds.length} transaction(s)?
            This action can be undone by restoring.
          </p>
          <div className="flex justify-end gap-2 pt-4">
            <Button
              variant="outline"
              onClick={() => onDeleteOpenChange(false)}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deleteTransactions.isPending}
              onClick={() => {
                onDeleteComplete();
                deleteTransactions.mutate(selectedIds);
              }}
            >
              {deleteTransactions.isPending && <Spinner size="sm" />}
              {deleteTransactions.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}

import {
  useCreateAccount,
  useUpdateAccount,
  useDeleteAccounts,
} from "@/hooks/useAccounts";
import { AccountForm } from "@/components/AccountForm";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

interface AccountResponse {
  id: string;
  accountCode: string;
  name: string;
  isActive: boolean;
}

interface AccountDialogsProps {
  createOpen: boolean;
  onCreateOpenChange: (open: boolean) => void;
  editAccount: AccountResponse | null;
  onEditClose: () => void;
  deleteOpen: boolean;
  onDeleteOpenChange: (open: boolean) => void;
  selectedIds: string[];
  onDeleteComplete: () => void;
}

export function AccountDialogs({
  createOpen,
  onCreateOpenChange,
  editAccount,
  onEditClose,
  deleteOpen,
  onDeleteOpenChange,
  selectedIds,
  onDeleteComplete,
}: AccountDialogsProps) {
  const createAccount = useCreateAccount();
  const updateAccount = useUpdateAccount();
  const deleteAccounts = useDeleteAccounts();

  return (
    <>
      <Dialog open={createOpen} onOpenChange={onCreateOpenChange}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Account</DialogTitle>
          </DialogHeader>
          <AccountForm
            mode="create"
            isSubmitting={createAccount.isPending}
            onCancel={() => onCreateOpenChange(false)}
            onSubmit={(values) => {
              createAccount.mutate(values, {
                onSuccess: () => onCreateOpenChange(false),
              });
            }}
          />
        </DialogContent>
      </Dialog>

      <Dialog
        open={editAccount !== null}
        onOpenChange={(open) => !open && onEditClose()}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Account</DialogTitle>
          </DialogHeader>
          {editAccount && (
            <AccountForm
              mode="edit"
              defaultValues={{
                accountCode: editAccount.accountCode,
                name: editAccount.name,
                isActive: editAccount.isActive,
              }}
              isSubmitting={updateAccount.isPending}
              onCancel={onEditClose}
              onSubmit={(values) => {
                updateAccount.mutate(
                  { id: editAccount.id, ...values },
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
            <DialogTitle>Delete Accounts</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selectedIds.length} account(s)?
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
              disabled={deleteAccounts.isPending}
              onClick={() => {
                onDeleteComplete();
                deleteAccounts.mutate(selectedIds);
              }}
            >
              {deleteAccounts.isPending && <Spinner size="sm" />}
              {deleteAccounts.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}

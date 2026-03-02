import {
  useCreateReceipt,
  useUpdateReceipt,
  useDeleteReceipts,
} from "@/hooks/useReceipts";
import { ReceiptForm } from "@/components/ReceiptForm";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

interface ReceiptResponse {
  id: string;
  description?: string | null;
  location: string;
  date: string;
  taxAmount: number;
}

interface ReceiptDialogsProps {
  createOpen: boolean;
  onCreateOpenChange: (open: boolean) => void;
  editReceipt: ReceiptResponse | null;
  onEditClose: () => void;
  deleteOpen: boolean;
  onDeleteOpenChange: (open: boolean) => void;
  selectedIds: string[];
  onDeleteComplete: () => void;
}

export function ReceiptDialogs({
  createOpen,
  onCreateOpenChange,
  editReceipt,
  onEditClose,
  deleteOpen,
  onDeleteOpenChange,
  selectedIds,
  onDeleteComplete,
}: ReceiptDialogsProps) {
  const createReceipt = useCreateReceipt();
  const updateReceipt = useUpdateReceipt();
  const deleteReceipts = useDeleteReceipts();

  return (
    <>
      <Dialog open={createOpen} onOpenChange={onCreateOpenChange}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Receipt</DialogTitle>
          </DialogHeader>
          <ReceiptForm
            mode="create"
            isSubmitting={createReceipt.isPending}
            onCancel={() => onCreateOpenChange(false)}
            onSubmit={(values) => {
              createReceipt.mutate(
                {
                  ...values,
                  description: values.description || null,
                },
                { onSuccess: () => onCreateOpenChange(false) },
              );
            }}
          />
        </DialogContent>
      </Dialog>

      <Dialog
        open={editReceipt !== null}
        onOpenChange={(open) => !open && onEditClose()}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Receipt</DialogTitle>
          </DialogHeader>
          {editReceipt && (
            <ReceiptForm
              mode="edit"
              defaultValues={{
                description: editReceipt.description ?? "",
                location: editReceipt.location,
                date: editReceipt.date,
                taxAmount: editReceipt.taxAmount,
              }}
              isSubmitting={updateReceipt.isPending}
              onCancel={onEditClose}
              onSubmit={(values) => {
                updateReceipt.mutate(
                  {
                    id: editReceipt.id,
                    ...values,
                    description: values.description || null,
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
            <DialogTitle>Delete Receipts</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selectedIds.length} receipt(s)?
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
              disabled={deleteReceipts.isPending}
              onClick={() => {
                onDeleteComplete();
                deleteReceipts.mutate(selectedIds);
              }}
            >
              {deleteReceipts.isPending && <Spinner size="sm" />}
              {deleteReceipts.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}

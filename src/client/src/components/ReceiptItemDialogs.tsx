import {
  useCreateReceiptItem,
  useUpdateReceiptItem,
  useDeleteReceiptItems,
} from "@/hooks/useReceiptItems";
import { ReceiptItemForm } from "@/components/ReceiptItemForm";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

interface ReceiptItemResponse {
  id: string;
  receiptId: string;
  receiptItemCode: string;
  description: string;
  quantity: number;
  unitPrice: number;
  category: string;
  subcategory: string;
  pricingMode: "quantity" | "flat";
}

interface ReceiptItemDialogsProps {
  createOpen: boolean;
  onCreateOpenChange: (open: boolean) => void;
  editItem: ReceiptItemResponse | null;
  onEditClose: () => void;
  deleteOpen: boolean;
  onDeleteOpenChange: (open: boolean) => void;
  selectedIds: string[];
  onDeleteComplete: () => void;
}

export function ReceiptItemDialogs({
  createOpen,
  onCreateOpenChange,
  editItem,
  onEditClose,
  deleteOpen,
  onDeleteOpenChange,
  selectedIds,
  onDeleteComplete,
}: ReceiptItemDialogsProps) {
  const createItem = useCreateReceiptItem();
  const updateItem = useUpdateReceiptItem();
  const deleteItems = useDeleteReceiptItems();

  return (
    <>
      <Dialog open={createOpen} onOpenChange={onCreateOpenChange}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Receipt Item</DialogTitle>
          </DialogHeader>
          <ReceiptItemForm
            mode="create"
            isSubmitting={createItem.isPending}
            onCancel={() => onCreateOpenChange(false)}
            onSubmit={(values) => {
              const { receiptId, ...body } = values;
              createItem.mutate(
                { receiptId, body },
                { onSuccess: () => onCreateOpenChange(false) },
              );
            }}
          />
        </DialogContent>
      </Dialog>

      <Dialog
        open={editItem !== null}
        onOpenChange={(open) => {
          if (!open) onEditClose();
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Receipt Item</DialogTitle>
          </DialogHeader>
          {editItem && (
            <ReceiptItemForm
              mode="edit"
              defaultValues={{
                receiptId: editItem.receiptId,
                receiptItemCode: editItem.receiptItemCode,
                description: editItem.description,
                pricingMode: editItem.pricingMode ?? "quantity",
                quantity: editItem.quantity,
                unitPrice: editItem.unitPrice,
                category: editItem.category,
                subcategory: editItem.subcategory,
              }}
              isSubmitting={updateItem.isPending}
              onCancel={onEditClose}
              onSubmit={(values) => {
                const { receiptId, ...rest } = values;
                updateItem.mutate(
                  {
                    receiptId,
                    body: { id: editItem.id, ...rest },
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
            <DialogTitle>Delete Receipt Items</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selectedIds.length} item(s)? This
            action can be undone by restoring.
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
              disabled={deleteItems.isPending}
              onClick={() => {
                onDeleteComplete();
                deleteItems.mutate(selectedIds);
              }}
            >
              {deleteItems.isPending && <Spinner size="sm" />}
              {deleteItems.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}

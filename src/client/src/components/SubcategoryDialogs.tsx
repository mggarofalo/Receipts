import {
  useCreateSubcategory,
  useUpdateSubcategory,
  useDeleteSubcategories,
} from "@/hooks/useSubcategories";
import { SubcategoryForm } from "@/components/SubcategoryForm";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

interface SubcategoryResponse {
  id: string;
  name: string;
  categoryId: string;
  description?: string | null;
}

interface SubcategoryDialogsProps {
  createOpen: boolean;
  onCreateOpenChange: (open: boolean) => void;
  editSubcategory: SubcategoryResponse | null;
  onEditClose: () => void;
  deleteOpen: boolean;
  onDeleteOpenChange: (open: boolean) => void;
  selectedIds: string[];
  onDeleteComplete: () => void;
}

export function SubcategoryDialogs({
  createOpen,
  onCreateOpenChange,
  editSubcategory,
  onEditClose,
  deleteOpen,
  onDeleteOpenChange,
  selectedIds,
  onDeleteComplete,
}: SubcategoryDialogsProps) {
  const createSubcategory = useCreateSubcategory();
  const updateSubcategory = useUpdateSubcategory();
  const deleteSubcategories = useDeleteSubcategories();

  return (
    <>
      <Dialog open={createOpen} onOpenChange={onCreateOpenChange}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Subcategory</DialogTitle>
          </DialogHeader>
          <SubcategoryForm
            mode="create"
            isSubmitting={createSubcategory.isPending}
            onCancel={() => onCreateOpenChange(false)}
            onSubmit={(values) => {
              createSubcategory.mutate(values, {
                onSuccess: () => onCreateOpenChange(false),
              });
            }}
          />
        </DialogContent>
      </Dialog>

      <Dialog
        open={editSubcategory !== null}
        onOpenChange={(open) => !open && onEditClose()}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Subcategory</DialogTitle>
          </DialogHeader>
          {editSubcategory && (
            <SubcategoryForm
              mode="edit"
              defaultValues={{
                name: editSubcategory.name,
                categoryId: editSubcategory.categoryId,
                description: editSubcategory.description ?? "",
              }}
              isSubmitting={updateSubcategory.isPending}
              onCancel={onEditClose}
              onSubmit={(values) => {
                updateSubcategory.mutate(
                  { id: editSubcategory.id, ...values },
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
            <DialogTitle>Delete Subcategories</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selectedIds.length}{" "}
            subcategory(ies)? This action can be undone by restoring.
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
              disabled={deleteSubcategories.isPending}
              onClick={() => {
                onDeleteComplete();
                deleteSubcategories.mutate(selectedIds);
              }}
            >
              {deleteSubcategories.isPending && <Spinner size="sm" />}
              {deleteSubcategories.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}

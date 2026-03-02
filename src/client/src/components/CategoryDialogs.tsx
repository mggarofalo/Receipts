import {
  useCreateCategory,
  useUpdateCategory,
  useDeleteCategories,
} from "@/hooks/useCategories";
import { CategoryForm } from "@/components/CategoryForm";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

interface CategoryResponse {
  id: string;
  name: string;
  description?: string | null;
}

interface CategoryDialogsProps {
  createOpen: boolean;
  onCreateOpenChange: (open: boolean) => void;
  editCategory: CategoryResponse | null;
  onEditClose: () => void;
  deleteOpen: boolean;
  onDeleteOpenChange: (open: boolean) => void;
  selectedIds: string[];
  onDeleteComplete: () => void;
}

export function CategoryDialogs({
  createOpen,
  onCreateOpenChange,
  editCategory,
  onEditClose,
  deleteOpen,
  onDeleteOpenChange,
  selectedIds,
  onDeleteComplete,
}: CategoryDialogsProps) {
  const createCategory = useCreateCategory();
  const updateCategory = useUpdateCategory();
  const deleteCategories = useDeleteCategories();

  return (
    <>
      <Dialog open={createOpen} onOpenChange={onCreateOpenChange}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Category</DialogTitle>
          </DialogHeader>
          <CategoryForm
            mode="create"
            isSubmitting={createCategory.isPending}
            onCancel={() => onCreateOpenChange(false)}
            onSubmit={(values) => {
              createCategory.mutate(values, {
                onSuccess: () => onCreateOpenChange(false),
              });
            }}
          />
        </DialogContent>
      </Dialog>

      <Dialog
        open={editCategory !== null}
        onOpenChange={(open) => !open && onEditClose()}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Category</DialogTitle>
          </DialogHeader>
          {editCategory && (
            <CategoryForm
              mode="edit"
              defaultValues={{
                name: editCategory.name,
                description: editCategory.description ?? "",
              }}
              isSubmitting={updateCategory.isPending}
              onCancel={onEditClose}
              onSubmit={(values) => {
                updateCategory.mutate(
                  { id: editCategory.id, ...values },
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
            <DialogTitle>Delete Categories</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selectedIds.length} category(ies)?
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
              disabled={deleteCategories.isPending}
              onClick={() => {
                onDeleteComplete();
                deleteCategories.mutate(selectedIds);
              }}
            >
              {deleteCategories.isPending && <Spinner size="sm" />}
              {deleteCategories.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}

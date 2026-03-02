import {
  useCreateItemTemplate,
  useUpdateItemTemplate,
  useDeleteItemTemplates,
} from "@/hooks/useItemTemplates";
import { ItemTemplateForm } from "@/components/ItemTemplateForm";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

interface ItemTemplateResponse {
  id: string;
  name: string;
  description?: string | null;
  defaultCategory?: string | null;
  defaultSubcategory?: string | null;
  defaultUnitPrice?: number | null;
  defaultUnitPriceCurrency?: string | null;
  defaultPricingMode?: string | null;
  defaultItemCode?: string | null;
}

interface ItemTemplateDialogsProps {
  createOpen: boolean;
  onCreateOpenChange: (open: boolean) => void;
  editTemplate: ItemTemplateResponse | null;
  onEditClose: () => void;
  deleteOpen: boolean;
  onDeleteOpenChange: (open: boolean) => void;
  selectedIds: string[];
  onDeleteComplete: () => void;
}

export function ItemTemplateDialogs({
  createOpen,
  onCreateOpenChange,
  editTemplate,
  onEditClose,
  deleteOpen,
  onDeleteOpenChange,
  selectedIds,
  onDeleteComplete,
}: ItemTemplateDialogsProps) {
  const createItemTemplate = useCreateItemTemplate();
  const updateItemTemplate = useUpdateItemTemplate();
  const deleteItemTemplates = useDeleteItemTemplates();

  return (
    <>
      <Dialog open={createOpen} onOpenChange={onCreateOpenChange}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>Create Item Template</DialogTitle>
          </DialogHeader>
          <ItemTemplateForm
            mode="create"
            isSubmitting={createItemTemplate.isPending}
            onCancel={() => onCreateOpenChange(false)}
            onSubmit={(values) => {
              createItemTemplate.mutate(
                {
                  name: values.name,
                  description: values.description || null,
                  defaultCategory: values.defaultCategory || null,
                  defaultSubcategory: values.defaultSubcategory || null,
                  defaultUnitPrice: values.defaultUnitPrice ?? null,
                  defaultPricingMode: values.defaultPricingMode || null,
                  defaultItemCode: values.defaultItemCode || null,
                },
                { onSuccess: () => onCreateOpenChange(false) },
              );
            }}
          />
        </DialogContent>
      </Dialog>

      <Dialog
        open={editTemplate !== null}
        onOpenChange={(open) => !open && onEditClose()}
      >
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>Edit Item Template</DialogTitle>
          </DialogHeader>
          {editTemplate && (
            <ItemTemplateForm
              mode="edit"
              defaultValues={{
                name: editTemplate.name,
                description: editTemplate.description ?? "",
                defaultCategory: editTemplate.defaultCategory ?? "",
                defaultSubcategory: editTemplate.defaultSubcategory ?? "",
                defaultUnitPrice: editTemplate.defaultUnitPrice ?? undefined,
                defaultPricingMode: editTemplate.defaultPricingMode ?? "",
                defaultItemCode: editTemplate.defaultItemCode ?? "",
              }}
              isSubmitting={updateItemTemplate.isPending}
              onCancel={onEditClose}
              onSubmit={(values) => {
                updateItemTemplate.mutate(
                  {
                    id: editTemplate.id,
                    name: values.name,
                    description: values.description || null,
                    defaultCategory: values.defaultCategory || null,
                    defaultSubcategory: values.defaultSubcategory || null,
                    defaultUnitPrice: values.defaultUnitPrice ?? null,
                    defaultPricingMode: values.defaultPricingMode || null,
                    defaultItemCode: values.defaultItemCode || null,
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
            <DialogTitle>Delete Item Templates</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selectedIds.length} item
            template(s)? This action can be undone by restoring.
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
              disabled={deleteItemTemplates.isPending}
              onClick={() => {
                onDeleteComplete();
                deleteItemTemplates.mutate(selectedIds);
              }}
            >
              {deleteItemTemplates.isPending && <Spinner size="sm" />}
              {deleteItemTemplates.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}

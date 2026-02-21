import { useState, useMemo } from "react";
import {
  useReceiptItems,
  useCreateReceiptItem,
  useUpdateReceiptItem,
  useDeleteReceiptItems,
} from "@/hooks/useReceiptItems";
import { ReceiptItemForm } from "@/components/ReceiptItemForm";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

interface ReceiptItemResponse {
  id: string;
  receiptItemCode: string;
  description: string;
  quantity: number;
  unitPrice: number;
  category: string;
  subcategory: string;
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(amount);
}

function ReceiptItems() {
  const { data: items, isLoading } = useReceiptItems();
  const createItem = useCreateReceiptItem();
  const updateItem = useUpdateReceiptItem();
  const deleteItems = useDeleteReceiptItems();

  const [search, setSearch] = useState("");
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [createOpen, setCreateOpen] = useState(false);
  const [editItem, setEditItem] = useState<ReceiptItemResponse | null>(null);
  const [editReceiptId, setEditReceiptId] = useState("");
  const [deleteOpen, setDeleteOpen] = useState(false);

  const filtered = useMemo(() => {
    if (!items) return [];
    const term = search.toLowerCase();
    return (items as ReceiptItemResponse[]).filter(
      (item) =>
        item.description.toLowerCase().includes(term) ||
        item.receiptItemCode.toLowerCase().includes(term) ||
        item.category.toLowerCase().includes(term),
    );
  }, [items, search]);

  const grandTotal = useMemo(
    () => filtered.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0),
    [filtered],
  );

  function toggleSelect(id: string) {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  function toggleAll() {
    if (selected.size === filtered.length) {
      setSelected(new Set());
    } else {
      setSelected(new Set(filtered.map((item) => item.id)));
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <div className="h-10 w-64 animate-pulse rounded bg-muted" />
          <div className="h-10 w-32 animate-pulse rounded bg-muted" />
        </div>
        <div className="space-y-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="h-12 animate-pulse rounded bg-muted" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <Input
          placeholder="Search items..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="max-w-sm"
        />
        <div className="flex gap-2">
          {selected.size > 0 && (
            <Button
              variant="destructive"
              onClick={() => setDeleteOpen(true)}
            >
              Delete ({selected.size})
            </Button>
          )}
          <Button onClick={() => setCreateOpen(true)}>New Item</Button>
        </div>
      </div>

      {filtered.length === 0 ? (
        <div className="py-12 text-center text-muted-foreground">
          {search
            ? "No receipt items match your search."
            : "No receipt items yet. Create one to get started."}
        </div>
      ) : (
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-12">
                  <input
                    type="checkbox"
                    checked={
                      selected.size === filtered.length && filtered.length > 0
                    }
                    onChange={toggleAll}
                    className="h-4 w-4 rounded border-gray-300"
                  />
                </TableHead>
                <TableHead>Code</TableHead>
                <TableHead>Description</TableHead>
                <TableHead className="text-right">Qty</TableHead>
                <TableHead className="text-right">Unit Price</TableHead>
                <TableHead className="text-right">Total</TableHead>
                <TableHead>Category</TableHead>
                <TableHead>Subcategory</TableHead>
                <TableHead className="w-24">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filtered.map((item) => (
                <TableRow key={item.id}>
                  <TableCell>
                    <input
                      type="checkbox"
                      checked={selected.has(item.id)}
                      onChange={() => toggleSelect(item.id)}
                      className="h-4 w-4 rounded border-gray-300"
                    />
                  </TableCell>
                  <TableCell className="font-mono">
                    {item.receiptItemCode}
                  </TableCell>
                  <TableCell>{item.description}</TableCell>
                  <TableCell className="text-right">{item.quantity}</TableCell>
                  <TableCell className="text-right">
                    {formatCurrency(item.unitPrice)}
                  </TableCell>
                  <TableCell className="text-right">
                    {formatCurrency(item.quantity * item.unitPrice)}
                  </TableCell>
                  <TableCell>{item.category}</TableCell>
                  <TableCell>{item.subcategory}</TableCell>
                  <TableCell>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setEditItem(item)}
                    >
                      Edit
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
            <TableFooter>
              <TableRow>
                <TableCell colSpan={5} className="text-right font-medium">
                  Grand Total
                </TableCell>
                <TableCell className="text-right font-bold">
                  {formatCurrency(grandTotal)}
                </TableCell>
                <TableCell colSpan={3} />
              </TableRow>
            </TableFooter>
          </Table>
        </div>
      )}

      <Dialog open={createOpen} onOpenChange={setCreateOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Receipt Item</DialogTitle>
          </DialogHeader>
          <ReceiptItemForm
            mode="create"
            isSubmitting={createItem.isPending}
            onCancel={() => setCreateOpen(false)}
            onSubmit={(values) => {
              const { receiptId, ...body } = values;
              createItem.mutate(
                { receiptId, body },
                { onSuccess: () => setCreateOpen(false) },
              );
            }}
          />
        </DialogContent>
      </Dialog>

      <Dialog
        open={editItem !== null}
        onOpenChange={(open) => {
          if (!open) {
            setEditItem(null);
            setEditReceiptId("");
          }
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
                receiptId: editReceiptId,
                receiptItemCode: editItem.receiptItemCode,
                description: editItem.description,
                quantity: editItem.quantity,
                unitPrice: editItem.unitPrice,
                category: editItem.category,
                subcategory: editItem.subcategory,
              }}
              isSubmitting={updateItem.isPending}
              onCancel={() => {
                setEditItem(null);
                setEditReceiptId("");
              }}
              onSubmit={(values) => {
                const { receiptId, ...rest } = values;
                updateItem.mutate(
                  {
                    receiptId,
                    body: { id: editItem.id, ...rest },
                  },
                  {
                    onSuccess: () => {
                      setEditItem(null);
                      setEditReceiptId("");
                    },
                  },
                );
              }}
            />
          )}
        </DialogContent>
      </Dialog>

      <Dialog open={deleteOpen} onOpenChange={setDeleteOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Receipt Items</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selected.size} item(s)? This
            action can be undone by restoring.
          </p>
          <div className="flex justify-end gap-2 pt-4">
            <Button variant="outline" onClick={() => setDeleteOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deleteItems.isPending}
              onClick={() => {
                deleteItems.mutate([...selected], {
                  onSuccess: () => {
                    setSelected(new Set());
                    setDeleteOpen(false);
                  },
                });
              }}
            >
              {deleteItems.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}

export default ReceiptItems;

import { useState, useMemo } from "react";
import {
  useReceipts,
  useCreateReceipt,
  useUpdateReceipt,
  useDeleteReceipts,
} from "@/hooks/useReceipts";
import { ReceiptForm } from "@/components/ReceiptForm";
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
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

interface ReceiptResponse {
  id: string;
  description?: string | null;
  location: string;
  date: string;
  taxAmount: number;
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(amount);
}

function Receipts() {
  const { data: receipts, isLoading } = useReceipts();
  const createReceipt = useCreateReceipt();
  const updateReceipt = useUpdateReceipt();
  const deleteReceipts = useDeleteReceipts();

  const [search, setSearch] = useState("");
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [createOpen, setCreateOpen] = useState(false);
  const [editReceipt, setEditReceipt] = useState<ReceiptResponse | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);

  const filtered = useMemo(() => {
    if (!receipts) return [];
    const term = search.toLowerCase();
    const list = (receipts as ReceiptResponse[]).filter(
      (r) =>
        (r.description?.toLowerCase().includes(term) ?? false) ||
        r.location.toLowerCase().includes(term),
    );
    return list.sort(
      (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime(),
    );
  }, [receipts, search]);

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
      setSelected(new Set(filtered.map((r) => r.id)));
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
          placeholder="Search receipts..."
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
          <Button onClick={() => setCreateOpen(true)}>New Receipt</Button>
        </div>
      </div>

      {filtered.length === 0 ? (
        <div className="py-12 text-center text-muted-foreground">
          {search
            ? "No receipts match your search."
            : "No receipts yet. Create one to get started."}
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
                <TableHead>Description</TableHead>
                <TableHead>Location</TableHead>
                <TableHead>Date</TableHead>
                <TableHead className="text-right">Tax Amount</TableHead>
                <TableHead className="w-24">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filtered.map((receipt) => (
                <TableRow key={receipt.id}>
                  <TableCell>
                    <input
                      type="checkbox"
                      checked={selected.has(receipt.id)}
                      onChange={() => toggleSelect(receipt.id)}
                      className="h-4 w-4 rounded border-gray-300"
                    />
                  </TableCell>
                  <TableCell>
                    {receipt.description || (
                      <span className="text-muted-foreground italic">
                        No description
                      </span>
                    )}
                  </TableCell>
                  <TableCell>{receipt.location}</TableCell>
                  <TableCell>{receipt.date}</TableCell>
                  <TableCell className="text-right">
                    {formatCurrency(receipt.taxAmount)}
                  </TableCell>
                  <TableCell>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setEditReceipt(receipt)}
                    >
                      Edit
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      <Dialog open={createOpen} onOpenChange={setCreateOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Receipt</DialogTitle>
          </DialogHeader>
          <ReceiptForm
            mode="create"
            isSubmitting={createReceipt.isPending}
            onCancel={() => setCreateOpen(false)}
            onSubmit={(values) => {
              createReceipt.mutate(
                {
                  ...values,
                  description: values.description || null,
                },
                { onSuccess: () => setCreateOpen(false) },
              );
            }}
          />
        </DialogContent>
      </Dialog>

      <Dialog
        open={editReceipt !== null}
        onOpenChange={(open) => !open && setEditReceipt(null)}
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
              onCancel={() => setEditReceipt(null)}
              onSubmit={(values) => {
                updateReceipt.mutate(
                  {
                    id: editReceipt.id,
                    ...values,
                    description: values.description || null,
                  },
                  { onSuccess: () => setEditReceipt(null) },
                );
              }}
            />
          )}
        </DialogContent>
      </Dialog>

      <Dialog open={deleteOpen} onOpenChange={setDeleteOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Receipts</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selected.size} receipt(s)? This
            action can be undone by restoring.
          </p>
          <div className="flex justify-end gap-2 pt-4">
            <Button variant="outline" onClick={() => setDeleteOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deleteReceipts.isPending}
              onClick={() => {
                deleteReceipts.mutate([...selected], {
                  onSuccess: () => {
                    setSelected(new Set());
                    setDeleteOpen(false);
                  },
                });
              }}
            >
              {deleteReceipts.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}

export default Receipts;

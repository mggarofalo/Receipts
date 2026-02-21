import { useState, useMemo } from "react";
import {
  useTransactions,
  useCreateTransaction,
  useUpdateTransaction,
  useDeleteTransactions,
} from "@/hooks/useTransactions";
import { TransactionForm } from "@/components/TransactionForm";
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

interface TransactionResponse {
  id: string;
  amount: number;
  date: string;
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(amount);
}

function Transactions() {
  const { data: transactions, isLoading } = useTransactions();
  const createTransaction = useCreateTransaction();
  const updateTransaction = useUpdateTransaction();
  const deleteTransactions = useDeleteTransactions();

  const [search, setSearch] = useState("");
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [createOpen, setCreateOpen] = useState(false);
  const [editTransaction, setEditTransaction] =
    useState<TransactionResponse | null>(null);
  const [editReceiptId, setEditReceiptId] = useState("");
  const [editAccountId, setEditAccountId] = useState("");
  const [deleteOpen, setDeleteOpen] = useState(false);

  const filtered = useMemo(() => {
    if (!transactions) return [];
    const term = search.toLowerCase();
    const list = (transactions as TransactionResponse[]).filter(
      (t) =>
        t.date.includes(term) ||
        t.amount.toString().includes(term),
    );
    return list.sort(
      (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime(),
    );
  }, [transactions, search]);

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
      setSelected(new Set(filtered.map((t) => t.id)));
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
          placeholder="Search transactions..."
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
          <Button onClick={() => setCreateOpen(true)}>
            New Transaction
          </Button>
        </div>
      </div>

      {filtered.length === 0 ? (
        <div className="py-12 text-center text-muted-foreground">
          {search
            ? "No transactions match your search."
            : "No transactions yet. Create one to get started."}
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
                <TableHead className="text-right">Amount</TableHead>
                <TableHead>Date</TableHead>
                <TableHead className="w-24">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filtered.map((txn) => (
                <TableRow key={txn.id}>
                  <TableCell>
                    <input
                      type="checkbox"
                      checked={selected.has(txn.id)}
                      onChange={() => toggleSelect(txn.id)}
                      className="h-4 w-4 rounded border-gray-300"
                    />
                  </TableCell>
                  <TableCell className="text-right">
                    {formatCurrency(txn.amount)}
                  </TableCell>
                  <TableCell>{txn.date}</TableCell>
                  <TableCell>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setEditTransaction(txn)}
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
            <DialogTitle>Create Transaction</DialogTitle>
          </DialogHeader>
          <TransactionForm
            mode="create"
            isSubmitting={createTransaction.isPending}
            onCancel={() => setCreateOpen(false)}
            onSubmit={(values) => {
              createTransaction.mutate(
                {
                  receiptId: values.receiptId,
                  accountId: values.accountId,
                  body: { amount: values.amount, date: values.date },
                },
                { onSuccess: () => setCreateOpen(false) },
              );
            }}
          />
        </DialogContent>
      </Dialog>

      <Dialog
        open={editTransaction !== null}
        onOpenChange={(open) => {
          if (!open) {
            setEditTransaction(null);
            setEditReceiptId("");
            setEditAccountId("");
          }
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
                receiptId: editReceiptId,
                accountId: editAccountId,
                amount: editTransaction.amount,
                date: editTransaction.date,
              }}
              isSubmitting={updateTransaction.isPending}
              onCancel={() => {
                setEditTransaction(null);
                setEditReceiptId("");
                setEditAccountId("");
              }}
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
                  {
                    onSuccess: () => {
                      setEditTransaction(null);
                      setEditReceiptId("");
                      setEditAccountId("");
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
            <DialogTitle>Delete Transactions</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selected.size} transaction(s)?
            This action can be undone by restoring.
          </p>
          <div className="flex justify-end gap-2 pt-4">
            <Button variant="outline" onClick={() => setDeleteOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deleteTransactions.isPending}
              onClick={() => {
                deleteTransactions.mutate([...selected], {
                  onSuccess: () => {
                    setSelected(new Set());
                    setDeleteOpen(false);
                  },
                });
              }}
            >
              {deleteTransactions.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}

export default Transactions;

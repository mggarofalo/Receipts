import { useState } from "react";
import {
  useCreateAdjustment,
  useUpdateAdjustment,
  useDeleteAdjustments,
} from "@/hooks/useAdjustments";
import {
  AdjustmentForm,
  type AdjustmentFormValues,
} from "@/components/AdjustmentForm";
import { formatAdjustmentType } from "@/lib/adjustment-types";
import {
  parseProblemDetails,
  extractFieldErrors,
} from "@/lib/problem-details";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Spinner } from "@/components/ui/spinner";
import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { formatCurrency } from "@/lib/format";
import { Pencil } from "lucide-react";

interface AdjustmentRow {
  id: string;
  receiptId: string;
  type: string;
  amount: number;
  description?: string | null;
}

interface AdjustmentsCardProps {
  receiptId: string;
  adjustments: AdjustmentRow[];
  adjustmentTotal: number;
}

export function AdjustmentsCard({
  receiptId,
  adjustments,
  adjustmentTotal,
}: AdjustmentsCardProps) {
  const createAdjustment = useCreateAdjustment();
  const updateAdjustment = useUpdateAdjustment();
  const deleteAdjustments = useDeleteAdjustments();

  const [createOpen, setCreateOpen] = useState(false);
  const [editAdj, setEditAdj] = useState<AdjustmentRow | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [selectedAdjs, setSelectedAdjs] = useState<Set<string>>(new Set());
  const [serverErrors, setServerErrors] = useState<Record<string, string>>({});

  function handleCreate(values: AdjustmentFormValues) {
    setServerErrors({});
    createAdjustment.mutate(
      {
        receiptId,
        body: {
          type: values.type,
          amount: values.amount,
          description: values.description || null,
        },
      },
      {
        onSuccess: () => setCreateOpen(false),
        onError: (error) => {
          const problem = parseProblemDetails(error);
          if (problem) setServerErrors(extractFieldErrors(problem));
        },
      },
    );
  }

  function handleUpdate(values: AdjustmentFormValues) {
    if (!editAdj) return;
    setServerErrors({});
    updateAdjustment.mutate(
      {
        body: {
          id: editAdj.id,
          type: values.type,
          amount: values.amount,
          description: values.description || null,
        },
      },
      {
        onSuccess: () => setEditAdj(null),
        onError: (error) => {
          const problem = parseProblemDetails(error);
          if (problem) setServerErrors(extractFieldErrors(problem));
        },
      },
    );
  }

  function toggleSelect(id: string) {
    setSelectedAdjs((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  return (
    <>
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Adjustments ({adjustments.length})</CardTitle>
            <div className="flex gap-2">
              {selectedAdjs.size > 0 && (
                <Button
                  variant="destructive"
                  size="sm"
                  onClick={() => setDeleteOpen(true)}
                >
                  Delete ({selectedAdjs.size})
                </Button>
              )}
              <Button size="sm" onClick={() => { setServerErrors({}); setCreateOpen(true); }}>
                Add Adjustment
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {adjustments.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              No adjustments for this receipt.
            </p>
          ) : (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-12">
                      <input
                        type="checkbox"
                        aria-label="Select all adjustments"
                        checked={
                          selectedAdjs.size === adjustments.length &&
                          adjustments.length > 0
                        }
                        onChange={() => {
                          if (selectedAdjs.size === adjustments.length) {
                            setSelectedAdjs(new Set());
                          } else {
                            setSelectedAdjs(new Set(adjustments.map((a) => a.id)));
                          }
                        }}
                        className="h-4 w-4 rounded border-gray-300"
                      />
                    </TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead>Description</TableHead>
                    <TableHead className="text-right">Amount</TableHead>
                    <TableHead className="w-24">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {adjustments.map((adj) => (
                    <TableRow key={adj.id}>
                      <TableCell>
                        <input
                          type="checkbox"
                          aria-label={`Select ${adj.type} adjustment`}
                          checked={selectedAdjs.has(adj.id)}
                          onChange={() => toggleSelect(adj.id)}
                          className="h-4 w-4 rounded border-gray-300"
                        />
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{formatAdjustmentType(adj.type)}</Badge>
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {adj.description ?? "\u2014"}
                      </TableCell>
                      <TableCell className="text-right">
                        {formatCurrency(adj.amount)}
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="icon"
                          aria-label="Edit"
                          onClick={() => {
                            setServerErrors({});
                            setEditAdj(adj);
                          }}
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
                <TableFooter>
                  <TableRow>
                    <TableCell colSpan={3} className="text-right font-medium">
                      Adjustment Total
                    </TableCell>
                    <TableCell className="text-right font-bold">
                      {formatCurrency(adjustmentTotal)}
                    </TableCell>
                    <TableCell />
                  </TableRow>
                </TableFooter>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Create Dialog */}
      <Dialog open={createOpen} onOpenChange={setCreateOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add Adjustment</DialogTitle>
          </DialogHeader>
          <AdjustmentForm
            mode="create"
            isSubmitting={createAdjustment.isPending}
            serverErrors={serverErrors}
            onCancel={() => setCreateOpen(false)}
            onSubmit={handleCreate}
          />
        </DialogContent>
      </Dialog>

      {/* Edit Dialog */}
      <Dialog
        open={editAdj !== null}
        onOpenChange={(open) => !open && setEditAdj(null)}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit Adjustment</DialogTitle>
          </DialogHeader>
          {editAdj && (
            <AdjustmentForm
              mode="edit"
              defaultValues={{
                type: editAdj.type,
                amount: editAdj.amount,
                description: editAdj.description ?? "",
              }}
              isSubmitting={updateAdjustment.isPending}
              serverErrors={serverErrors}
              onCancel={() => setEditAdj(null)}
              onSubmit={handleUpdate}
            />
          )}
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteOpen} onOpenChange={setDeleteOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Adjustments</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Are you sure you want to delete {selectedAdjs.size} adjustment(s)?
            This action can be undone by restoring.
          </p>
          <div className="flex justify-end gap-2 pt-4">
            <Button variant="outline" onClick={() => setDeleteOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={deleteAdjustments.isPending}
              onClick={() => {
                const ids = [...selectedAdjs];
                setSelectedAdjs(new Set());
                setDeleteOpen(false);
                deleteAdjustments.mutate(ids);
              }}
            >
              {deleteAdjustments.isPending && <Spinner size="sm" />}
              {deleteAdjustments.isPending ? "Deleting..." : "Delete"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}

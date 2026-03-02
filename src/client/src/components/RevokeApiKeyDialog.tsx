import { useMutation, useQueryClient } from "@tanstack/react-query";
import client from "@/lib/api-client";
import { showSuccess, showError } from "@/lib/toast";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

interface RevokeApiKeyDialogProps {
  keyId: string | null;
  onClose: () => void;
}

export function RevokeApiKeyDialog({
  keyId,
  onClose,
}: RevokeApiKeyDialogProps) {
  const queryClient = useQueryClient();

  const revokeMutation = useMutation({
    mutationFn: async (id: string) => {
      const { error } = await client.DELETE("/api/apikeys/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["apiKeys"] });
      showSuccess("API key revoked.");
      onClose();
    },
    onError: () => {
      showError("Failed to revoke API key.");
    },
  });

  return (
    <Dialog
      open={keyId !== null}
      onOpenChange={(open) => {
        if (!open) onClose();
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Revoke API Key</DialogTitle>
          <DialogDescription>
            This action cannot be undone. The API key will be immediately
            invalidated.
          </DialogDescription>
        </DialogHeader>
        <div className="flex gap-2 justify-end">
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button
            variant="destructive"
            disabled={revokeMutation.isPending}
            onClick={() => {
              if (keyId) revokeMutation.mutate(keyId);
            }}
          >
            {revokeMutation.isPending && <Spinner size="sm" />}
            {revokeMutation.isPending ? "Revoking..." : "Revoke"}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}

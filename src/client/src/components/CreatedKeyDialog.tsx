import { showSuccess, showError } from "@/lib/toast";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Alert, AlertDescription } from "@/components/ui/alert";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

interface CreatedKeyDialogProps {
  rawKey: string | null;
  onClose: () => void;
}

export function CreatedKeyDialog({ rawKey, onClose }: CreatedKeyDialogProps) {
  async function handleCopyKey() {
    if (!rawKey) return;
    try {
      await navigator.clipboard.writeText(rawKey);
      showSuccess("API key copied to clipboard.");
    } catch {
      showError("Failed to copy to clipboard.");
    }
  }

  return (
    <Dialog
      open={rawKey !== null}
      onOpenChange={(open) => {
        if (!open) onClose();
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>API Key Created</DialogTitle>
        </DialogHeader>
        <Alert>
          <AlertDescription>
            Save this key now. You won&apos;t be able to see it again.
          </AlertDescription>
        </Alert>
        <Input
          readOnly
          value={rawKey ?? ""}
          onClick={(e) => (e.target as HTMLInputElement).select()}
          className="font-mono text-sm"
        />
        <Button onClick={handleCopyKey} className="w-full">
          Copy to Clipboard
        </Button>
      </DialogContent>
    </Dialog>
  );
}

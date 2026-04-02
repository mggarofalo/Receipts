import { useState, useRef } from "react";
import { useMutation } from "@tanstack/react-query";
import { usePageTitle } from "@/hooks/usePageTitle";
import { getAccessToken } from "@/lib/auth";
import { showSuccess, showError } from "@/lib/toast";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Spinner } from "@/components/ui/spinner";
import { DatabaseBackup, Upload, Download, AlertTriangle } from "lucide-react";

const baseUrl = import.meta.env.VITE_API_URL ?? "";

function formatFileSize(bytes: number): string {
  if (bytes === 0) return "0 B";
  const units = ["B", "KB", "MB", "GB", "TB"];
  const i = Math.min(
    Math.floor(Math.log(bytes) / Math.log(1024)),
    units.length - 1,
  );
  return `${(bytes / Math.pow(1024, i)).toFixed(i === 0 ? 0 : 1)} ${units[i]}`;
}

function BackupRestore() {
  usePageTitle("Backup & Restore");
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [confirmImportOpen, setConfirmImportOpen] = useState(false);

  const exportMutation = useMutation({
    mutationFn: async () => {
      const token = getAccessToken();
      const res = await fetch(`${baseUrl}/api/backup/export`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
        },
        signal: AbortSignal.timeout(300_000), // 5-minute timeout for large exports
      });

      if (!res.ok) {
        if (res.status === 403) throw new Error("You do not have permission to export backups.");
        throw new Error(`Export failed (${res.status}).`);
      }

      const blob = await res.blob();
      const disposition = res.headers.get("Content-Disposition");
      let filename = `receipts-backup-${new Date().toISOString().slice(0, 10)}.sqlite`;
      if (disposition) {
        const match = disposition.match(/filename="?([^";\n]+)"?/);
        if (match) filename = match[1];
      }

      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = filename;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    },
    onSuccess: () => {
      showSuccess("Backup exported successfully.");
    },
    onError: (error: Error) => {
      showError(error.message);
    },
  });

  const importMutation = useMutation({
    mutationFn: async (file: File) => {
      const token = getAccessToken();
      const formData = new FormData();
      formData.append("file", file);

      const res = await fetch(`${baseUrl}/api/backup/import`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
        },
        body: formData,
        signal: AbortSignal.timeout(300_000), // 5-minute timeout for large imports
      });

      if (!res.ok) {
        if (res.status === 400) throw new Error("Invalid or corrupt backup file.");
        if (res.status === 403) throw new Error("You do not have permission to import backups.");
        throw new Error(`Import failed (${res.status}).`);
      }

      return res.json() as Promise<{
        recordsImported: number;
        recordsUpdated: number;
        recordsSkipped: number;
      }>;
    },
    onSuccess: (data) => {
      showSuccess(
        `Import complete: ${data.recordsImported} imported, ${data.recordsUpdated} updated, ${data.recordsSkipped} skipped.`,
      );
      setSelectedFile(null);
      setConfirmImportOpen(false);
      if (fileInputRef.current) fileInputRef.current.value = "";
    },
    onError: (error: Error) => {
      showError(error.message);
      setConfirmImportOpen(false);
    },
  });

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0] ?? null;
    setSelectedFile(file);
  }

  function handleImportClick() {
    if (!selectedFile) return;
    setConfirmImportOpen(true);
  }

  function handleConfirmImport() {
    if (!selectedFile) return;
    importMutation.mutate(selectedFile);
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Backup & Restore</h1>
        <p className="text-sm text-muted-foreground">
          Export or import a portable SQLite backup of your data
        </p>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Export Card */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Download className="h-5 w-5" />
              Export Backup
            </CardTitle>
            <CardDescription>
              Download a complete SQLite backup of all your data. This file can
              be used to restore your data on another instance or recover from
              data loss.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button
              onClick={() => exportMutation.mutate()}
              disabled={exportMutation.isPending}
              className="w-full"
            >
              {exportMutation.isPending && <Spinner size="sm" />}
              {exportMutation.isPending ? "Exporting..." : "Export Backup"}
            </Button>
          </CardContent>
        </Card>

        {/* Import Card */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Upload className="h-5 w-5" />
              Import Backup
            </CardTitle>
            <CardDescription>
              Upload a previously exported SQLite backup file. Existing records
              will be updated and new records will be added.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label htmlFor="backup-file" className="sr-only">
                Select backup file
              </label>
              <input
                ref={fileInputRef}
                id="backup-file"
                type="file"
                accept=".sqlite,.db"
                onChange={handleFileChange}
                className="block w-full text-sm text-muted-foreground file:mr-4 file:rounded-md file:border-0 file:bg-primary file:px-4 file:py-2 file:text-sm file:font-medium file:text-primary-foreground hover:file:bg-primary/90 file:cursor-pointer"
              />
              {selectedFile && (
                <p className="mt-2 text-sm text-muted-foreground">
                  Selected: {selectedFile.name} ({formatFileSize(selectedFile.size)})
                </p>
              )}
            </div>
            <Button
              onClick={handleImportClick}
              disabled={!selectedFile || importMutation.isPending}
              variant="outline"
              className="w-full"
            >
              {importMutation.isPending && <Spinner size="sm" />}
              {importMutation.isPending ? "Importing..." : "Import Backup"}
            </Button>
          </CardContent>
        </Card>
      </div>

      <Alert>
        <DatabaseBackup className="h-4 w-4" />
        <AlertDescription>
          Backups include all receipts, transactions, accounts, categories, and
          related data. User accounts and authentication settings are not
          included in backups for security reasons.
        </AlertDescription>
      </Alert>

      {/* Import Confirmation Dialog */}
      <Dialog
        open={confirmImportOpen}
        onOpenChange={(open) => {
          if (!importMutation.isPending) setConfirmImportOpen(open);
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-yellow-500" />
              Confirm Import
            </DialogTitle>
            <DialogDescription>
              Importing a backup will update existing records and add new ones.
              This action may modify your current data.
            </DialogDescription>
          </DialogHeader>
          {selectedFile && (
            <p className="text-sm text-muted-foreground">
              File: {selectedFile.name} ({formatFileSize(selectedFile.size)})
            </p>
          )}
          <div className="flex gap-2 justify-end">
            <Button
              variant="outline"
              onClick={() => setConfirmImportOpen(false)}
              disabled={importMutation.isPending}
            >
              Cancel
            </Button>
            <Button
              onClick={handleConfirmImport}
              disabled={importMutation.isPending}
            >
              {importMutation.isPending && <Spinner size="sm" />}
              {importMutation.isPending ? "Importing..." : "Confirm Import"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}

export default BackupRestore;

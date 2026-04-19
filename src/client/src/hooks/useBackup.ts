import { useMutation } from "@tanstack/react-query";
import { getAccessToken } from "@/lib/auth";
import { showSuccess, showError } from "@/lib/toast";

const baseUrl = import.meta.env.VITE_API_URL ?? "";

export function useBackupExport() {
  return useMutation({
    mutationFn: async () => {
      const token = getAccessToken();
      const res = await fetch(`${baseUrl}/api/backup/export`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
        },
        signal: AbortSignal.timeout(300_000),
      });

      if (!res.ok) {
        if (res.status === 403)
          throw new Error("You do not have permission to export backups.");
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
}

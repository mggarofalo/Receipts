import { useMutation } from "@tanstack/react-query";
import client from "@/lib/api-client";

export function useReceiptScan() {
  return useMutation({
    mutationFn: async (file: File) => {
      const { data, error } = await client.POST("/api/receipts/scan", {
        // openapi-fetch expects the body shape from the spec; we cast because
        // the typed body is `{ file?: string }` but at runtime we pass a File.
        body: { file } as unknown as { file?: string },
        bodySerializer: (body) => {
          const fd = new FormData();
          fd.append("file", (body as unknown as { file: File }).file);
          return fd;
        },
      });
      if (error) throw error;
      return data;
    },
  });
}

import { useState, useRef, useEffect, useCallback } from "react";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Spinner } from "@/components/ui/spinner";
import { Upload, AlertCircle, FileText } from "lucide-react";

const ACCEPTED_TYPES = ["image/jpeg", "image/png", "application/pdf"];
const MAX_FILE_SIZE = 20 * 1024 * 1024; // 20 MB

interface ReceiptImageUploadProps {
  onScan: (file: File) => void;
  isLoading: boolean;
  error: string | null;
}

export function ReceiptImageUpload({
  onScan,
  isLoading,
  error,
}: ReceiptImageUploadProps) {
  const [dragOver, setDragOver] = useState(false);
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [validationError, setValidationError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const prevPreviewRef = useRef<string | null>(null);

  // Revoke old object URL on unmount
  useEffect(() => {
    return () => {
      if (prevPreviewRef.current) {
        URL.revokeObjectURL(prevPreviewRef.current);
      }
    };
  }, []);

  const validateFile = useCallback((f: File): string | null => {
    if (!ACCEPTED_TYPES.includes(f.type)) {
      return "Only JPEG, PNG, and PDF files are supported.";
    }
    if (f.size > MAX_FILE_SIZE) {
      return "File size must be 20 MB or less.";
    }
    return null;
  }, []);

  const handleFile = useCallback(
    (f: File) => {
      const err = validateFile(f);
      if (err) {
        setValidationError(err);
        setFile(null);
        // Revoke old preview URL
        if (prevPreviewRef.current) {
          URL.revokeObjectURL(prevPreviewRef.current);
          prevPreviewRef.current = null;
        }
        setPreview(null);
        return;
      }
      setValidationError(null);
      setFile(f);
      // Revoke old preview URL
      if (prevPreviewRef.current) {
        URL.revokeObjectURL(prevPreviewRef.current);
        prevPreviewRef.current = null;
      }
      // Only create image preview for image files, not PDFs
      if (f.type === "application/pdf") {
        setPreview("pdf");
      } else {
        const url = URL.createObjectURL(f);
        prevPreviewRef.current = url;
        setPreview(url);
      }
    },
    [validateFile],
  );

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
  }, []);

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      setDragOver(false);
      const dropped = e.dataTransfer.files[0];
      if (dropped) handleFile(dropped);
    },
    [handleFile],
  );

  const handleInputChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const selected = e.target.files?.[0];
      if (selected) handleFile(selected);
    },
    [handleFile],
  );

  const handleScan = useCallback(() => {
    if (file) onScan(file);
  }, [file, onScan]);

  const displayError = validationError ?? error;

  return (
    <div className="space-y-4">
      {displayError && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{displayError}</AlertDescription>
        </Alert>
      )}

      <div
        role="button"
        tabIndex={0}
        aria-label="Drop zone for receipt file"
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        onClick={() => inputRef.current?.click()}
        onKeyDown={(e) => {
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            inputRef.current?.click();
          }
        }}
        className={`flex flex-col items-center justify-center rounded-lg border-2 border-dashed p-8 transition-colors ${
          dragOver
            ? "border-primary bg-primary/5"
            : "border-muted-foreground/25 hover:border-primary/50"
        } ${isLoading ? "pointer-events-none opacity-60" : "cursor-pointer"}`}
      >
        {isLoading ? (
          <div className="flex flex-col items-center gap-3">
            <Spinner size="lg" />
            <p className="text-sm text-muted-foreground">
              Processing receipt...
            </p>
          </div>
        ) : preview === "pdf" ? (
          <div className="flex flex-col items-center gap-3">
            <FileText className="h-16 w-16 text-muted-foreground" />
            <p className="text-sm text-muted-foreground">{file?.name}</p>
          </div>
        ) : preview ? (
          <div className="flex flex-col items-center gap-3">
            <img
              src={preview}
              alt="Receipt preview"
              className="max-h-64 rounded-md object-contain"
            />
            <p className="text-sm text-muted-foreground">{file?.name}</p>
          </div>
        ) : (
          <div className="flex flex-col items-center gap-3">
            <Upload className="h-10 w-10 text-muted-foreground" />
            <div className="text-center">
              <p className="text-sm font-medium">
                Drop a receipt image or PDF here
              </p>
              <p className="text-xs text-muted-foreground">
                JPEG, PNG, or PDF, up to 20 MB
              </p>
            </div>
          </div>
        )}
      </div>

      <input
        ref={inputRef}
        type="file"
        accept="image/jpeg,image/png,application/pdf"
        className="hidden"
        onChange={handleInputChange}
        data-testid="file-input"
      />

      <div className="flex gap-2">
        <Button
          variant="outline"
          onClick={(e) => {
            e.stopPropagation();
            inputRef.current?.click();
          }}
          disabled={isLoading}
        >
          Choose File
        </Button>
        <Button onClick={handleScan} disabled={!file || isLoading}>
          {isLoading ? (
            <>
              <Spinner size="sm" className="mr-2" />
              Processing...
            </>
          ) : (
            "Scan Receipt"
          )}
        </Button>
      </div>
    </div>
  );
}

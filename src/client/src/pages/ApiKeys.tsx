import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import client from "@/lib/api-client";
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
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

const createKeySchema = z.object({
  name: z.string().min(1, "Name is required"),
  expiresAt: z.string().optional(),
});

type CreateKeyFormValues = z.infer<typeof createKeySchema>;

function getKeyStatus(key: {
  isRevoked: boolean;
  expiresAt?: string | null;
}): "active" | "expired" | "revoked" {
  if (key.isRevoked) return "revoked";
  if (key.expiresAt && new Date(key.expiresAt) < new Date()) return "expired";
  return "active";
}

function statusBadgeVariant(
  status: "active" | "expired" | "revoked",
): "default" | "secondary" | "destructive" | "outline" {
  switch (status) {
    case "active":
      return "default";
    case "expired":
      return "secondary";
    case "revoked":
      return "destructive";
  }
}

function formatDate(dateStr: string | null | undefined): string {
  if (!dateStr) return "-";
  return new Date(dateStr).toLocaleDateString();
}

function ApiKeys() {
  const queryClient = useQueryClient();
  const [createOpen, setCreateOpen] = useState(false);
  const [createdKey, setCreatedKey] = useState<string | null>(null);
  const [revokeId, setRevokeId] = useState<string | null>(null);

  const { data: apiKeys = [], isLoading } = useQuery({
    queryKey: ["apiKeys"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/apikeys");
      if (error) throw error;
      return data ?? [];
    },
  });

  const createMutation = useMutation({
    mutationFn: async (values: CreateKeyFormValues) => {
      const { data, error } = await client.POST("/api/apikeys", {
        body: {
          name: values.name,
          expiresAt: values.expiresAt || undefined,
        },
      });
      if (error) throw error;
      return data;
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ["apiKeys"] });
      if (data) {
        setCreatedKey(data.rawKey);
      }
      setCreateOpen(false);
    },
    onError: () => {
      showError("Failed to create API key.");
    },
  });

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
      setRevokeId(null);
    },
    onError: () => {
      showError("Failed to revoke API key.");
    },
  });

  const createForm = useForm<CreateKeyFormValues>({
    resolver: zodResolver(createKeySchema),
    defaultValues: { name: "", expiresAt: "" },
  });

  function handleCreateOpen() {
    createForm.reset();
    setCreateOpen(true);
  }

  function handleCreateSubmit(values: CreateKeyFormValues) {
    createMutation.mutate(values);
  }

  async function handleCopyKey() {
    if (!createdKey) return;
    try {
      await navigator.clipboard.writeText(createdKey);
      showSuccess("API key copied to clipboard.");
    } catch {
      showError("Failed to copy to clipboard.");
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">API Keys</h1>
          <p className="text-sm text-muted-foreground">
            Manage API keys for programmatic access
          </p>
        </div>
        <Button onClick={handleCreateOpen}>Create API Key</Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Your API Keys</CardTitle>
          <CardDescription>
            API keys allow external applications to authenticate with your
            account.
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <p className="text-muted-foreground">Loading...</p>
          ) : apiKeys.length === 0 ? (
            <p className="text-muted-foreground">
              No API keys yet. Create one to get started.
            </p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Name</TableHead>
                  <TableHead>Created</TableHead>
                  <TableHead>Last Used</TableHead>
                  <TableHead>Expires</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {apiKeys.map((key) => {
                  const status = getKeyStatus(key);
                  return (
                    <TableRow key={key.id}>
                      <TableCell className="font-medium">{key.name}</TableCell>
                      <TableCell>{formatDate(key.createdAt)}</TableCell>
                      <TableCell>{formatDate(key.lastUsedAt)}</TableCell>
                      <TableCell>{formatDate(key.expiresAt)}</TableCell>
                      <TableCell>
                        <Badge variant={statusBadgeVariant(status)}>
                          {status}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        {status === "active" && (
                          <Button
                            variant="destructive"
                            size="sm"
                            onClick={() => setRevokeId(key.id)}
                          >
                            Revoke
                          </Button>
                        )}
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Create API Key Dialog */}
      <Dialog open={createOpen} onOpenChange={setCreateOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create API Key</DialogTitle>
            <DialogDescription>
              Generate a new API key for programmatic access.
            </DialogDescription>
          </DialogHeader>
          <Form {...createForm}>
            <form
              onSubmit={createForm.handleSubmit(handleCreateSubmit)}
              className="space-y-4"
            >
              <FormField
                control={createForm.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Name</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="e.g. Paperless Integration"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={createForm.control}
                name="expiresAt"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Expiration Date (optional)</FormLabel>
                    <FormControl>
                      <Input type="date" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <Button
                type="submit"
                className="w-full"
                disabled={createMutation.isPending}
              >
                {createMutation.isPending ? "Creating..." : "Create Key"}
              </Button>
            </form>
          </Form>
        </DialogContent>
      </Dialog>

      {/* Created Key Display Dialog */}
      <Dialog
        open={createdKey !== null}
        onOpenChange={(open) => {
          if (!open) setCreatedKey(null);
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
            value={createdKey ?? ""}
            onClick={(e) => (e.target as HTMLInputElement).select()}
            className="font-mono text-sm"
          />
          <Button onClick={handleCopyKey} className="w-full">
            Copy to Clipboard
          </Button>
        </DialogContent>
      </Dialog>

      {/* Revoke Confirmation Dialog */}
      <Dialog
        open={revokeId !== null}
        onOpenChange={(open) => {
          if (!open) setRevokeId(null);
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
            <Button variant="outline" onClick={() => setRevokeId(null)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              disabled={revokeMutation.isPending}
              onClick={() => {
                if (revokeId) revokeMutation.mutate(revokeId);
              }}
            >
              {revokeMutation.isPending ? "Revoking..." : "Revoke"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}

export default ApiKeys;

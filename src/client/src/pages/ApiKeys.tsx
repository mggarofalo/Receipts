import { useState, useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
import client from "@/lib/api-client";
import { CreateApiKeyDialog } from "@/components/CreateApiKeyDialog";
import { CreatedKeyDialog } from "@/components/CreatedKeyDialog";
import { RevokeApiKeyDialog } from "@/components/RevokeApiKeyDialog";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { TableSkeleton } from "@/components/ui/table-skeleton";

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
  usePageTitle("API Keys");
  const [createOpen, setCreateOpen] = useState(false);
  const [createdKey, setCreatedKey] = useState<string | null>(null);
  const [revokeId, setRevokeId] = useState<string | null>(null);

  const anyDialogOpen = createOpen || createdKey !== null || revokeId !== null;

  const { data: apiKeys = [], isLoading } = useQuery({
    queryKey: ["apiKeys"],
    queryFn: async () => {
      const { data, error } = await client.GET("/api/apikeys");
      if (error) throw error;
      return data ?? [];
    },
  });

  useEffect(() => {
    function onNewItem() {
      setCreateOpen(true);
    }
    window.addEventListener("shortcut:new-item", onNewItem);
    return () => window.removeEventListener("shortcut:new-item", onNewItem);
  });

  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items: apiKeys as { id: string }[],
    getId: (k) => k.id,
    enabled: !anyDialogOpen && apiKeys.length > 0,
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">API Keys</h1>
          <p className="text-sm text-muted-foreground">
            Manage API keys for programmatic access
          </p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>Create API Key</Button>
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
            <TableSkeleton columns={5} rows={3} showToolbar={false} />
          ) : apiKeys.length === 0 ? (
            <p className="text-muted-foreground">
              No API keys yet. Create one to get started.
            </p>
          ) : (
            <div ref={tableRef}>
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
                  {apiKeys.map((key, index) => {
                    const status = getKeyStatus(key);
                    return (
                      <TableRow
                        key={key.id}
                        className={`cursor-pointer ${focusedId === key.id ? "bg-accent" : ""}`}
                        onClick={(e) => {
                          if (
                            (e.target as HTMLElement).closest(
                              "button, input, a, [role='button']",
                            )
                          )
                            return;
                          setFocusedIndex(index);
                        }}
                      >
                        <TableCell className="font-medium">
                          {key.name}
                        </TableCell>
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
            </div>
          )}
        </CardContent>
      </Card>

      <CreateApiKeyDialog
        open={createOpen}
        onOpenChange={setCreateOpen}
        onKeyCreated={setCreatedKey}
      />
      <CreatedKeyDialog
        rawKey={createdKey}
        onClose={() => setCreatedKey(null)}
      />
      <RevokeApiKeyDialog
        keyId={revokeId}
        onClose={() => setRevokeId(null)}
      />
    </div>
  );
}

export default ApiKeys;

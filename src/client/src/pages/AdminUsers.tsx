import { useState } from "react";
import { useUserRoles, useAssignRole, useRemoveRole } from "@/hooks/useRoles";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
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

const AVAILABLE_ROLES = ["Admin", "User"];

function AdminUsers() {
  const [inputId, setInputId] = useState("");
  const [userId, setUserId] = useState<string | null>(null);
  const { data: rolesData, isLoading, isError } = useUserRoles(userId);
  const assignRole = useAssignRole();
  const removeRole = useRemoveRole();

  function handleLookup() {
    const trimmed = inputId.trim();
    if (trimmed) setUserId(trimmed);
  }

  const currentRoles = rolesData?.roles ?? [];
  const unassignedRoles = AVAILABLE_ROLES.filter(
    (r) => !currentRoles.includes(r),
  );

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">User Management</h1>

      <div className="flex items-end gap-4">
        <div className="flex-1 max-w-md space-y-2">
          <Label htmlFor="userId">User ID</Label>
          <Input
            id="userId"
            placeholder="Enter user UUID..."
            value={inputId}
            onChange={(e) => setInputId(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && handleLookup()}
          />
        </div>
        <Button onClick={handleLookup} disabled={!inputId.trim()}>
          Look Up
        </Button>
      </div>

      {isLoading && (
        <div className="space-y-4">
          <div className="h-48 animate-pulse rounded bg-muted" />
        </div>
      )}

      {isError && userId && (
        <div className="py-12 text-center text-muted-foreground">
          User not found or unable to fetch roles.
        </div>
      )}

      {rolesData && userId && (
        <>
          <Card>
            <CardHeader>
              <CardTitle>Current Roles</CardTitle>
              <CardDescription>
                Roles assigned to user{" "}
                <span className="font-mono text-xs">{userId}</span>
              </CardDescription>
            </CardHeader>
            <CardContent>
              {currentRoles.length === 0 ? (
                <p className="text-sm text-muted-foreground">
                  No roles assigned.
                </p>
              ) : (
                <div className="rounded-md border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Role</TableHead>
                        <TableHead className="w-[100px]">Action</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {currentRoles.map((role) => (
                        <TableRow key={role}>
                          <TableCell>
                            <Badge variant="default">{role}</Badge>
                          </TableCell>
                          <TableCell>
                            <Button
                              variant="destructive"
                              size="sm"
                              disabled={removeRole.isPending}
                              onClick={() =>
                                removeRole.mutate({ userId, role })
                              }
                            >
                              Remove
                            </Button>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              )}
            </CardContent>
          </Card>

          {unassignedRoles.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Assign Role</CardTitle>
                <CardDescription>
                  Add a role to this user.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="flex gap-2">
                  {unassignedRoles.map((role) => (
                    <Button
                      key={role}
                      variant="outline"
                      disabled={assignRole.isPending}
                      onClick={() => assignRole.mutate({ userId, role })}
                    >
                      Assign {role}
                    </Button>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}
        </>
      )}
    </div>
  );
}

export default AdminUsers;

import { useState } from "react";
import { useUsers } from "@/hooks/useUsers";
import { useAuth } from "@/hooks/useAuth";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
import { CreateUserDialog } from "@/components/CreateUserDialog";
import { EditUserDialog } from "@/components/EditUserDialog";
import type { EditableUser } from "@/components/EditUserDialog";
import { ResetPasswordDialog } from "@/components/ResetPasswordDialog";
import { DeactivateUserDialog } from "@/components/DeactivateUserDialog";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { TableSkeleton } from "@/components/ui/table-skeleton";

function formatDate(dateStr: string | null | undefined) {
  if (!dateStr) return "-";
  return new Date(dateStr).toLocaleDateString();
}

function AdminUsers() {
  usePageTitle("User Management");
  const { user: currentUser } = useAuth();

  const [page, setPage] = useState(1);
  const pageSize = 20;
  const { data, isLoading } = useUsers(page, pageSize);

  const [createOpen, setCreateOpen] = useState(false);
  const [editUser, setEditUser] = useState<EditableUser | null>(null);
  const [resetUserId, setResetUserId] = useState<string | null>(null);
  const [deactivateUser, setDeactivateUser] = useState<{
    id: string;
    email: string;
  } | null>(null);

  const items = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  const listItems = items.map((u) => ({ id: u.id }));
  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items: listItems,
    getId: (u) => u.id,
    enabled: listItems.length > 0,
  });

  function openEdit(user: (typeof items)[0]) {
    const primaryRole = user.roles[0] ?? "User";
    setEditUser({
      id: user.id,
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      role: primaryRole,
      isDisabled: user.isDisabled,
    });
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">User Management</h1>
        <Button onClick={() => setCreateOpen(true)}>Create User</Button>
      </div>

      {isLoading && <TableSkeleton rows={5} columns={7} />}

      {!isLoading && items.length === 0 && (
        <div className="py-12 text-center text-muted-foreground">
          No users found.
        </div>
      )}

      {!isLoading && items.length > 0 && (
        <>
          <div className="rounded-md border" ref={tableRef}>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Name</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Role</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Created</TableHead>
                  <TableHead>Last Login</TableHead>
                  <TableHead className="w-[200px]">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {items.map((user, index) => {
                  const isSelf = currentUser?.email === user.email;
                  const name =
                    [user.firstName, user.lastName]
                      .filter(Boolean)
                      .join(" ") || "-";
                  return (
                    <TableRow
                      key={user.id}
                      className={`cursor-pointer ${focusedId === user.id ? "bg-accent" : ""}`}
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
                      <TableCell className="font-medium">{name}</TableCell>
                      <TableCell>{user.email}</TableCell>
                      <TableCell>
                        {user.roles.map((role) => (
                          <Badge
                            key={role}
                            variant={
                              role === "Admin" ? "default" : "secondary"
                            }
                            className="mr-1"
                          >
                            {role}
                          </Badge>
                        ))}
                      </TableCell>
                      <TableCell>
                        <Badge
                          variant={user.isDisabled ? "destructive" : "outline"}
                        >
                          {user.isDisabled ? "Disabled" : "Active"}
                        </Badge>
                      </TableCell>
                      <TableCell>{formatDate(user.createdAt)}</TableCell>
                      <TableCell>{formatDate(user.lastLoginAt)}</TableCell>
                      <TableCell>
                        <div className="flex gap-1">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => openEdit(user)}
                          >
                            Edit
                          </Button>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => setResetUserId(user.id)}
                          >
                            Reset PW
                          </Button>
                          <Button
                            variant="destructive"
                            size="sm"
                            disabled={isSelf || user.isDisabled}
                            onClick={() =>
                              setDeactivateUser({
                                id: user.id,
                                email: user.email,
                              })
                            }
                          >
                            Disable
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </div>

          {totalPages > 1 && (
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">
                Page {page} of {totalPages} ({totalCount} users)
              </p>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={page <= 1}
                  onClick={() => setPage(page - 1)}
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={page >= totalPages}
                  onClick={() => setPage(page + 1)}
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </>
      )}

      <CreateUserDialog open={createOpen} onOpenChange={setCreateOpen} />
      <EditUserDialog user={editUser} onClose={() => setEditUser(null)} />
      <ResetPasswordDialog
        userId={resetUserId}
        onClose={() => setResetUserId(null)}
      />
      <DeactivateUserDialog
        user={deactivateUser}
        onClose={() => setDeactivateUser(null)}
      />
    </div>
  );
}

export default AdminUsers;

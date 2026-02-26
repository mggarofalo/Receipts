import { useState, useMemo } from "react";
import { useUsers } from "@/hooks/useUsers";
import { useUserRoles, useAssignRole, useRemoveRole } from "@/hooks/useRoles";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useFuzzySearch } from "@/hooks/useFuzzySearch";
import { usePagination } from "@/hooks/usePagination";
import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
import type { FuseSearchConfig } from "@/lib/search";
import { FuzzySearchInput } from "@/components/FuzzySearchInput";
import { SearchHighlight } from "@/components/SearchHighlight";
import { getMatchIndices } from "@/lib/search-highlight";
import { NoResults } from "@/components/NoResults";
import { Pagination } from "@/components/Pagination";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { TableSkeleton } from "@/components/ui/table-skeleton";
import { Spinner } from "@/components/ui/spinner";

// Must match AppRoles.All on the backend (src/Common/AppRoles.cs)
const AVAILABLE_ROLES = ["Admin", "User"];

interface UserSummary {
  id: string;
  email: string;
  roles: string[];
  isDisabled: boolean;
  createdAt: string;
}

const SEARCH_CONFIG: FuseSearchConfig<UserSummary> = {
  keys: [
    { name: "email", weight: 2 },
    { name: "roles", weight: 1 },
  ],
};

function AdminUsers() {
  usePageTitle("User Management");
  const { data: usersData, isLoading } = useUsers();
  const [manageUser, setManageUser] = useState<UserSummary | null>(null);
  const assignRole = useAssignRole();
  const removeRole = useRemoveRole();

  // Fetch roles for the selected user (for real-time updates in dialog)
  const { data: rolesData } = useUserRoles(manageUser?.id ?? null);

  const anyDialogOpen = manageUser !== null;

  const data = useMemo(() => {
    const items = (usersData as { items?: UserSummary[] } | undefined)?.items ?? [];
    return [...items].sort((a, b) => a.email.localeCompare(b.email));
  }, [usersData]);

  const { search, setSearch, results, totalCount, clearSearch } =
    useFuzzySearch({ data, config: SEARCH_CONFIG });

  const matchMap = useMemo(() => {
    const map = new Map<string, (typeof results)[number]>();
    for (const r of results) {
      map.set(r.item.id, r);
    }
    return map;
  }, [results]);

  const filteredResults = useMemo(
    () => results.map((r) => r.item),
    [results],
  );

  const {
    paginatedItems,
    currentPage,
    pageSize,
    totalItems,
    totalPages,
    setPage,
    setPageSize,
  } = usePagination({ items: filteredResults });

  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items: paginatedItems,
    getId: (u) => u.id,
    enabled: !anyDialogOpen,
    onOpen: (u) => setManageUser(u),
  });

  // Use live roles from the query if available, otherwise fall back to the snapshot
  const currentRoles = rolesData?.roles ?? manageUser?.roles ?? [];
  const unassignedRoles = AVAILABLE_ROLES.filter(
    (r) => !currentRoles.includes(r),
  );

  if (isLoading) {
    return <TableSkeleton columns={4} />;
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold tracking-tight">User Management</h1>
      <div className="flex items-center justify-between">
        <FuzzySearchInput
          aria-label="Search users"
          value={search}
          onChange={setSearch}
          placeholder="Search users..."
          resultCount={filteredResults.length}
          totalCount={totalCount}
          className="max-w-sm"
        />
      </div>

      {filteredResults.length === 0 ? (
        search ? (
          <NoResults
            searchTerm={search}
            onClearSearch={clearSearch}
            onSelectSuggestion={setSearch}
            entityName="users"
          />
        ) : (
          <div className="py-12 text-center text-muted-foreground">
            No users found.
          </div>
        )
      ) : (
        <>
          <div className="rounded-md border" ref={tableRef}>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Email</TableHead>
                  <TableHead>Roles</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="w-32">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {paginatedItems.map((user, index) => {
                  const result = matchMap.get(user.id);
                  const matches = result?.matches;
                  return (
                    <TableRow
                      key={user.id}
                      className={`cursor-pointer ${focusedId === user.id ? "bg-accent" : ""}`}
                      onClick={(e) => {
                        if ((e.target as HTMLElement).closest("button, input, a, [role='button']")) return;
                        setFocusedIndex(index);
                      }}
                    >
                      <TableCell>
                        <SearchHighlight
                          text={user.email}
                          indices={getMatchIndices(matches, "email")}
                        />
                      </TableCell>
                      <TableCell>
                        <div className="flex gap-1 flex-wrap">
                          {user.roles.map((role) => (
                            <Badge key={role} variant="default">
                              {role}
                            </Badge>
                          ))}
                          {user.roles.length === 0 && (
                            <span className="text-sm text-muted-foreground">None</span>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge variant={user.isDisabled ? "destructive" : "secondary"}>
                          {user.isDisabled ? "Disabled" : "Active"}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => setManageUser(user)}
                        >
                          Manage Roles
                        </Button>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </div>
          <Pagination
            currentPage={currentPage}
            totalItems={totalItems}
            pageSize={pageSize}
            totalPages={totalPages}
            onPageChange={setPage}
            onPageSizeChange={setPageSize}
          />
        </>
      )}

      <Dialog
        open={manageUser !== null}
        onOpenChange={(open) => {
          if (!open) setManageUser(null);
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Manage Roles</DialogTitle>
          </DialogHeader>
          {manageUser && (
            <div className="space-y-4">
              <p className="text-sm text-muted-foreground">
                {manageUser.email}
              </p>

              <div>
                <h4 className="text-sm font-medium mb-2">Current Roles</h4>
                {currentRoles.length === 0 ? (
                  <p className="text-sm text-muted-foreground">No roles assigned.</p>
                ) : (
                  <div className="flex gap-2 flex-wrap">
                    {currentRoles.map((role: string) => (
                      <div key={role} className="flex items-center gap-1">
                        <Badge variant="default">{role}</Badge>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="h-6 px-2 text-destructive hover:text-destructive"
                          disabled={removeRole.isPending}
                          onClick={() =>
                            removeRole.mutate({ userId: manageUser.id, role })
                          }
                        >
                          {removeRole.isPending ? (
                            <Spinner size="sm" />
                          ) : (
                            "Remove"
                          )}
                        </Button>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              {unassignedRoles.length > 0 && (
                <div>
                  <h4 className="text-sm font-medium mb-2">Assign Role</h4>
                  <div className="flex gap-2">
                    {unassignedRoles.map((role) => (
                      <Button
                        key={role}
                        variant="outline"
                        size="sm"
                        disabled={assignRole.isPending}
                        onClick={() =>
                          assignRole.mutate({ userId: manageUser.id, role })
                        }
                      >
                        {assignRole.isPending && <Spinner size="sm" />}
                        {assignRole.isPending ? "Assigning..." : `Assign ${role}`}
                      </Button>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}

export default AdminUsers;

import {
  useMyAuthAuditLog,
  useRecentAuthAuditLogs,
  useFailedAuthAttempts,
} from "@/hooks/useAuthAudit";
import { usePermission } from "@/hooks/usePermission";
import type { AuthAuditLog } from "@/lib/audit-utils";
import { AuthAuditTable } from "@/components/AuthAuditTable";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

function SecurityLog() {
  const { isAdmin } = usePermission();
  const myLogs = useMyAuthAuditLog(100);
  const recentLogs = useRecentAuthAuditLogs(100);
  const failedLogs = useFailedAuthAttempts(100);

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold">Security Log</h1>

      <Tabs defaultValue="my-activity">
        <TabsList>
          <TabsTrigger value="my-activity">My Activity</TabsTrigger>
          {isAdmin() && (
            <TabsTrigger value="all-events">All Events</TabsTrigger>
          )}
          {isAdmin() && (
            <TabsTrigger value="failed-logins">Failed Logins</TabsTrigger>
          )}
        </TabsList>

        <TabsContent value="my-activity">
          <AuthAuditTable
            logs={(myLogs.data ?? []) as AuthAuditLog[]}
            isLoading={myLogs.isLoading}
          />
        </TabsContent>

        {isAdmin() && (
          <TabsContent value="all-events">
            <AuthAuditTable
              logs={(recentLogs.data ?? []) as AuthAuditLog[]}
              isLoading={recentLogs.isLoading}
              showUsername
            />
          </TabsContent>
        )}

        {isAdmin() && (
          <TabsContent value="failed-logins">
            <AuthAuditTable
              logs={(failedLogs.data ?? []) as AuthAuditLog[]}
              isLoading={failedLogs.isLoading}
              showUsername
            />
          </TabsContent>
        )}
      </Tabs>
    </div>
  );
}

export default SecurityLog;

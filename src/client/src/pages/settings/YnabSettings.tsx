import { usePageTitle } from "@/hooks/usePageTitle";
import { useAccounts } from "@/hooks/useAccounts";
import {
  useYnabBudgets,
  useSelectedYnabBudget,
  useSelectYnabBudget,
  useYnabAccounts,
  useYnabAccountMappings,
  useCreateYnabAccountMapping,
  useUpdateYnabAccountMapping,
  useDeleteYnabAccountMapping,
} from "@/hooks/useYnab";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Alert, AlertDescription } from "@/components/ui/alert";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";

const UNMAPPED_VALUE = "__unmapped__";

export default function YnabSettings() {
  usePageTitle("YNAB Settings");

  const { budgets, isLoading: budgetsLoading, isError: budgetsError } = useYnabBudgets();
  const { selectedBudgetId, isLoading: settingsLoading } = useSelectedYnabBudget();
  const selectBudget = useSelectYnabBudget();

  const { data: receiptsAccounts, isLoading: accountsLoading } = useAccounts(0, 200);
  const { accounts: ynabAccounts, isLoading: ynabAccountsLoading } = useYnabAccounts();
  const { mappings, isLoading: mappingsLoading } = useYnabAccountMappings();
  const createMapping = useCreateYnabAccountMapping();
  const updateMapping = useUpdateYnabAccountMapping();
  const deleteMapping = useDeleteYnabAccountMapping();

  const isLoading = budgetsLoading || settingsLoading;
  const notConfigured = budgetsError;
  const mappingSectionLoading = accountsLoading || ynabAccountsLoading || mappingsLoading;

  function handleBudgetChange(budgetId: string) {
    selectBudget.mutate(budgetId);
  }

  function handleYnabAccountChange(receiptsAccountId: string, ynabAccountId: string) {
    const existingMapping = mappings.find(
      (m) => m.receiptsAccountId === receiptsAccountId,
    );

    if (ynabAccountId === UNMAPPED_VALUE) {
      if (existingMapping) {
        deleteMapping.mutate(existingMapping.id);
      }
      return;
    }

    const ynabAccount = ynabAccounts.find((a) => a.id === ynabAccountId);
    if (!ynabAccount || !selectedBudgetId) return;

    if (existingMapping) {
      updateMapping.mutate({
        id: existingMapping.id,
        ynabAccountId: ynabAccount.id,
        ynabAccountName: ynabAccount.name,
        ynabBudgetId: selectedBudgetId,
      });
    } else {
      createMapping.mutate({
        receiptsAccountId,
        ynabAccountId: ynabAccount.id,
        ynabAccountName: ynabAccount.name,
        ynabBudgetId: selectedBudgetId,
      });
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">YNAB Settings</h1>
        <p className="text-muted-foreground">
          Configure your YNAB integration for transaction sync.
        </p>
      </div>

      {notConfigured && (
        <Alert variant="destructive">
          <AlertDescription>
            YNAB is not configured. Set the <code>YNAB_PAT</code> environment
            variable with your YNAB personal access token to enable the
            integration.
          </AlertDescription>
        </Alert>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Budget Selection</CardTitle>
          <CardDescription>
            Select the YNAB budget to use for syncing transactions.
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="flex items-center gap-2">
              <Spinner className="h-4 w-4" />
              <span className="text-sm text-muted-foreground">Loading budgets...</span>
            </div>
          ) : notConfigured ? (
            <p className="text-sm text-muted-foreground">
              Configure YNAB to see available budgets.
            </p>
          ) : (
            <Select
              value={selectedBudgetId ?? ""}
              onValueChange={handleBudgetChange}
              disabled={selectBudget.isPending}
            >
              <SelectTrigger className="w-full max-w-sm">
                <SelectValue placeholder="Select a budget" />
              </SelectTrigger>
              <SelectContent>
                {budgets.map((budget) => (
                  <SelectItem key={budget.id} value={budget.id}>
                    {budget.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Account Mapping</CardTitle>
          <CardDescription>
            Map your receipts accounts to YNAB accounts for transaction sync.
          </CardDescription>
        </CardHeader>
        <CardContent>
          {mappingSectionLoading ? (
            <div className="flex items-center gap-2">
              <Spinner className="h-4 w-4" />
              <span className="text-sm text-muted-foreground">Loading accounts...</span>
            </div>
          ) : notConfigured || !selectedBudgetId ? (
            <p className="text-sm text-muted-foreground">
              {notConfigured
                ? "Configure YNAB to map accounts."
                : "Select a budget above to map accounts."}
            </p>
          ) : !receiptsAccounts || receiptsAccounts.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              No receipts accounts found. Create accounts first.
            </p>
          ) : (
            <div className="space-y-4">
              {receiptsAccounts.map((account) => {
                const mapping = mappings.find(
                  (m) => m.receiptsAccountId === account.id,
                );
                const currentYnabAccountId = mapping?.ynabAccountId ?? UNMAPPED_VALUE;

                return (
                  <div
                    key={account.id}
                    className="flex items-center gap-4"
                  >
                    <span className="min-w-[200px] text-sm font-medium">
                      {account.name}
                    </span>
                    <Select
                      value={currentYnabAccountId}
                      onValueChange={(value) =>
                        handleYnabAccountChange(account.id!, value)
                      }
                      disabled={
                        createMapping.isPending ||
                        updateMapping.isPending ||
                        deleteMapping.isPending
                      }
                    >
                      <SelectTrigger className="w-full max-w-sm">
                        <SelectValue placeholder="Select a YNAB account" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value={UNMAPPED_VALUE}>
                          <span className="text-muted-foreground">Not mapped</span>
                        </SelectItem>
                        {ynabAccounts.map((ynabAccount) => (
                          <SelectItem
                            key={ynabAccount.id}
                            value={ynabAccount.id}
                          >
                            {ynabAccount.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    {mapping && (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => deleteMapping.mutate(mapping.id)}
                        disabled={deleteMapping.isPending}
                      >
                        Remove
                      </Button>
                    )}
                  </div>
                );
              })}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

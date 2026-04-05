import { usePageTitle } from "@/hooks/usePageTitle";
import {
  useYnabBudgets,
  useSelectedYnabBudget,
  useSelectYnabBudget,
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
import { Spinner } from "@/components/ui/spinner";

export default function YnabSettings() {
  usePageTitle("YNAB Settings");

  const { budgets, isLoading: budgetsLoading, isError: budgetsError } = useYnabBudgets();
  const { selectedBudgetId, isLoading: settingsLoading } = useSelectedYnabBudget();
  const selectBudget = useSelectYnabBudget();

  const isLoading = budgetsLoading || settingsLoading;
  const notConfigured = budgetsError;

  function handleBudgetChange(budgetId: string) {
    selectBudget.mutate(budgetId);
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
    </div>
  );
}

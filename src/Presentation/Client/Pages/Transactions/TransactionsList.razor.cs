using Client.Common;
using Microsoft.AspNetCore.Components;
using Shared.ViewModels.Core;

namespace Client.Pages.Transactions;

public partial class TransactionsList
{
    private List<TransactionVM> transactions = [];
    private string searchString = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadTransactions();
    }

    private async Task LoadTransactions()
    {
        List<TransactionVM>? result = await TransactionService.GetAllTransactionsAsync();

        if (result == null)
        {
            Snackbar.ShowErrorMessage("An error occurred while retrieving transactions. Please try again.");
            transactions = [];
        }
        else
        {
            transactions = result;
        }
    }

    private void AddTransaction()
    {
        NavigationManager.NavigateTo("/transactions/create");
    }

    private void EditTransaction(TransactionVM transaction)
    {
        NavigationManager.NavigateTo($"/transactions/edit/{transaction.Id}");
    }

    private async Task DeleteTransaction(TransactionVM transaction)
    {
        bool? confirm = await DialogService.ShowMessageBox(
            "Confirm Delete",
            "Are you sure you want to delete this transaction?",
            yesText: "Delete", cancelText: "Cancel");

        if (confirm == true)
        {
            try
            {
                await TransactionService.DeleteTransactionsAsync([transaction.Id!.Value]);
                await LoadTransactions();
                Snackbar.ShowSuccessMessage("Transaction deleted successfully");
            }
            catch (Exception ex)
            {
                Snackbar.ShowErrorMessage(ex.Message);
            }
        }
    }

    private bool FilterFunc(TransactionVM transaction)
    {
        if (string.IsNullOrWhiteSpace(searchString))
        {
            return true;
        }

        if (transaction.Amount.HasValue && transaction.Amount.Value.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (transaction.Date.HasValue && transaction.Date.Value.ToString("yyyy-MM-dd").Contains(searchString, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}

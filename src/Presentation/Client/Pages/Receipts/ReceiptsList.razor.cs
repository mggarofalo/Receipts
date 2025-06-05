using Client.Common;
using Microsoft.AspNetCore.Components;
using Shared.ViewModels.Core;

namespace Client.Pages.Receipts;

public partial class ReceiptsList
{
    private List<ReceiptVM> receipts = [];
    private string searchString = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadReceipts();
    }

    private async Task LoadReceipts()
    {
        List<ReceiptVM>? result = await ReceiptService.GetAllReceiptsAsync();

        if (result == null)
        {
            Snackbar.ShowErrorMessage("An error occurred while retrieving receipts. Please try again.");
            receipts = [];
        }
        else
        {
            receipts = result;
        }
    }

    private void AddReceipt()
    {
        NavigationManager.NavigateTo("/receipts/create");
    }

    private void EditReceipt(ReceiptVM receipt)
    {
        NavigationManager.NavigateTo($"/receipts/edit/{receipt.Id}");
    }

    private async Task DeleteReceipt(ReceiptVM receipt)
    {
        bool? confirm = await DialogService.ShowMessageBox(
            "Confirm Delete",
            "Are you sure you want to delete this receipt?",
            yesText: "Delete", cancelText: "Cancel");

        if (confirm == true)
        {
            try
            {
                await ReceiptService.DeleteReceiptsAsync([receipt.Id!.Value]);
                await LoadReceipts();
                Snackbar.ShowSuccessMessage("Receipt deleted successfully");
            }
            catch (Exception ex)
            {
                Snackbar.ShowErrorMessage(ex.Message);
            }
        }
    }

    private bool FilterFunc(ReceiptVM receipt)
    {
        if (string.IsNullOrWhiteSpace(searchString))
        {
            return true;
        }

        if (receipt.Description != null && receipt.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (receipt.Location != null && receipt.Location.Contains(searchString, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}

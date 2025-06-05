using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.ViewModels.Core;
using Shared.Validators;
using FluentValidation.Results;
using Client.Common;

namespace Client.Pages.Receipts;

public partial class ReceiptCreate
{
    private MudForm? form;
    private readonly ReceiptVM _receipt = new();
    private bool success;

    private async Task SubmitReceipt()
    {
        try
        {
            if (!await ValidateForm())
            {
                return;
            }

            if (!await ValidateReceipt())
            {
                return;
            }

            await ReceiptService.CreateReceiptsAsync([_receipt]);
            Snackbar.ShowSuccessMessage("Receipt created successfully");
            NavigateToReceipts();
        }
        catch (Exception ex)
        {
            Snackbar.ShowErrorMessage(ex.Message);
        }
    }

    private async Task<bool> ValidateForm()
    {
        if (form is null)
        {
            Snackbar.ShowErrorMessage("Form is null");
            return false;
        }

        await form.Validate();

        return form.IsValid;
    }

    private async Task<bool> ValidateReceipt()
    {
        ReceiptValidator validator = new();
        ValidationResult validationResult = await validator.ValidateAsync(_receipt);

        if (!validationResult.IsValid)
        {
            Snackbar.ShowValidationErrors(validationResult);
            return false;
        }

        return true;
    }

    private void NavigateToReceipts()
    {
        NavigationManager.NavigateTo("/receipts");
    }
}

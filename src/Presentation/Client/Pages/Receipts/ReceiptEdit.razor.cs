using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.ViewModels.Core;
using Shared.Validators;
using FluentValidation.Results;
using Client.Common;

namespace Client.Pages.Receipts;

public partial class ReceiptEdit
{
    [Parameter]
    public Guid? Id { get; set; }

    private MudForm? form;
    private ReceiptVM _receipt = new();
    private bool success;

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            ReceiptVM? result = await ReceiptService.GetReceiptByIdAsync(Id.Value);

            if (result == null)
            {
                Snackbar.ShowErrorMessage("Receipt not found");
                NavigateToReceipts();
                return;
            }

            _receipt = result;
        }
    }

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

            if (Id.HasValue)
            {
                _receipt.Id = Id.Value;
                await ReceiptService.UpdateReceiptsAsync([_receipt]);
                Snackbar.ShowSuccessMessage("Receipt updated successfully");
            }
            else
            {
                await ReceiptService.CreateReceiptsAsync([_receipt]);
                Snackbar.ShowSuccessMessage("Receipt created successfully");
            }

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

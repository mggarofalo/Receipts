using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.ViewModels.Core;
using Shared.Validators;
using FluentValidation.Results;
using Client.Common;

namespace Client.Pages.Transactions;

public partial class TransactionCreate
{
    private MudForm? form;
    private readonly TransactionVM _transaction = new();
    private bool success;

    private async Task SubmitTransaction()
    {
        try
        {
            if (!await ValidateForm())
            {
                return;
            }

            if (!await ValidateTransaction())
            {
                return;
            }

            await TransactionService.CreateTransactionsAsync([_transaction]);
            Snackbar.ShowSuccessMessage("Transaction created successfully");
            NavigateToTransactions();
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

    private async Task<bool> ValidateTransaction()
    {
        TransactionValidator validator = new();
        ValidationResult validationResult = await validator.ValidateAsync(_transaction);

        if (!validationResult.IsValid)
        {
            Snackbar.ShowValidationErrors(validationResult);
            return false;
        }

        return true;
    }

    private void NavigateToTransactions()
    {
        NavigationManager.NavigateTo("/transactions");
    }
}

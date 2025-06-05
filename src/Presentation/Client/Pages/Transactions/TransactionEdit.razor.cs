using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.ViewModels.Core;
using Shared.Validators;
using FluentValidation.Results;
using Client.Common;

namespace Client.Pages.Transactions;

public partial class TransactionEdit
{
    [Parameter]
    public Guid? Id { get; set; }

    private MudForm? form;
    private TransactionVM _transaction = new();
    private bool success;

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            TransactionVM? result = await TransactionService.GetTransactionByIdAsync(Id.Value);

            if (result == null)
            {
                Snackbar.ShowErrorMessage("Transaction not found");
                NavigateToTransactions();
                return;
            }

            _transaction = result;
        }
    }

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

            if (Id.HasValue)
            {
                _transaction.Id = Id.Value;
                await TransactionService.UpdateTransactionsAsync([_transaction]);
                Snackbar.ShowSuccessMessage("Transaction updated successfully");
            }
            else
            {
                await TransactionService.CreateTransactionsAsync([_transaction]);
                Snackbar.ShowSuccessMessage("Transaction created successfully");
            }

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

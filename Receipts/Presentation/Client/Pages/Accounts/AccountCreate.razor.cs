using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.ViewModels.Core;
using Shared.Validators;
using FluentValidation.Results;
using Client.Common;

namespace Client.Pages.Accounts;

public partial class AccountCreate
{
	private MudForm? form;
	private readonly AccountVM _account = new();
	private bool success;

	private async Task SubmitAccount()
	{
		try
		{
			if (!await ValidateForm())
			{
				return;
			}

			if (!await ValidateAccount())
			{
				return;
			}

			await AccountService.CreateAccountsAsync([_account]);
			Snackbar.ShowSuccessMessage("Account created successfully");
			NavigateToAccounts();
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

	private async Task<bool> ValidateAccount()
	{
		AccountValidator validator = new();
		ValidationResult validationResult = await validator.ValidateAsync(_account);

		if (!validationResult.IsValid)
		{
			Snackbar.ShowValidationErrors(validationResult);
			return false;
		}

		return true;
	}

	private void NavigateToAccounts()
	{
		NavigationManager.NavigateTo("/accounts");
	}
}

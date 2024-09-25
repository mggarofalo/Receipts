using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.ViewModels.Core;
using Shared.Validators;
using FluentValidation.Results;
using Client.Common;

namespace Client.Pages.Accounts;

public partial class AccountEdit
{
	[Parameter]
	public Guid? Id { get; set; }

	private MudForm? form;
	private AccountVM _account = new();
	private bool success;

	protected override async Task OnInitializedAsync()
	{
		if (Id.HasValue)
		{
			AccountVM? result = await AccountService.GetAccountByIdAsync(Id.Value);

			if (result == null)
			{
				Snackbar.ShowErrorMessage("Account not found");
				NavigateToAccounts();
				return;
			}

			_account = result;
		}
	}

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

			if (Id.HasValue)
			{
				_account.Id = Id.Value;
				await AccountService.UpdateAccountsAsync([_account]);
				Snackbar.ShowSuccessMessage("Account updated successfully");
			}
			else
			{
				await AccountService.CreateAccountsAsync([_account]);
				Snackbar.ShowSuccessMessage("Account created successfully");
			}

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
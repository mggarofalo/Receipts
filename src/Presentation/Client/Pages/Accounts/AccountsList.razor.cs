using Client.Common;
using Microsoft.AspNetCore.Components;
using Shared.ViewModels.Core;

namespace Client.Pages.Accounts;

public partial class AccountsList
{
	private List<AccountVM> accounts = [];
	private string searchString = "";

	protected override async Task OnInitializedAsync()
	{
		await LoadAccounts();
	}

	private async Task LoadAccounts()
	{
		List<AccountVM>? result = await AccountService.GetAllAccountsAsync();

		if (result == null)
		{
			Snackbar.ShowErrorMessage("An error occurred while retrieving accounts. Please try again.");
			accounts = [];
		}
		else
		{
			accounts = result;
		}
	}

	private void AddAccount()
	{
		NavigationManager.NavigateTo("/accounts/create");
	}

	private void EditAccount(AccountVM account)
	{
		NavigationManager.NavigateTo($"/accounts/edit/{account.Id}");
	}

	private async Task DeleteAccount(AccountVM account)
	{
		bool? confirm = await DialogService.ShowMessageBox(
			"Confirm Delete",
			$"Are you sure you want to delete this account?",
			yesText: "Delete", cancelText: "Cancel");

		if (confirm == true)
		{
			try
			{
				await AccountService.DeleteAccountsAsync([account.Id!.Value]);
				await LoadAccounts();
				Snackbar.ShowSuccessMessage("Account deleted successfully");
			}
			catch (Exception ex)
			{
				Snackbar.ShowErrorMessage(ex.Message);
			}
		}
	}

	private bool FilterFunc(AccountVM account)
	{
		if (string.IsNullOrWhiteSpace(searchString))
		{
			return true;
		}

		if (account.AccountCode != null && account.AccountCode.Contains(searchString, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		if (account.Name != null && account.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		return false;
	}
}

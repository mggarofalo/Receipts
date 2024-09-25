using Client.Interfaces.Services.Core;
using Shared.HttpClientApiExtensions.Core;
using Shared.ViewModels.Core;

namespace Client.Services.Core;

public class AccountService(HttpClient httpClient) : IAccountService
{
	public async Task<List<AccountVM>?> CreateAccountsAsync(List<AccountVM> models, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.CreateAccountsAsync(models, cancellationToken);
	}

	public async Task<AccountVM?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.GetAccountByIdAsync(id, cancellationToken);
	}

	public async Task<List<AccountVM>?> GetAllAccountsAsync(CancellationToken cancellationToken = default)
	{
		return await httpClient
			.GetAllAccountsAsync(cancellationToken);
	}

	public async Task<bool> UpdateAccountsAsync(List<AccountVM> models, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.UpdateAccountsAsync(models, cancellationToken);
	}

	public async Task<bool> DeleteAccountsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.DeleteAccountsAsync(ids, cancellationToken);
	}
}

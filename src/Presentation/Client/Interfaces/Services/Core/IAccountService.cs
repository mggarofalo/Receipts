using Shared.ViewModels.Core;

namespace Client.Interfaces.Services.Core;

public interface IAccountService
{
	Task<List<AccountVM>?> CreateAccountsAsync(List<AccountVM> models, CancellationToken cancellationToken = default);
	Task<AccountVM?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<List<AccountVM>?> GetAllAccountsAsync(CancellationToken cancellationToken = default);
	Task<bool> UpdateAccountsAsync(List<AccountVM> models, CancellationToken cancellationToken = default);
	Task<bool> DeleteAccountsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
}

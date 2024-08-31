using Application.Interfaces;

namespace Application.Commands.Account;

public record UpdateAccountCommand : ICommand<bool>
{
	public IReadOnlyList<Domain.Core.Account> Accounts { get; }
	public const string AccountsCannotBeEmptyExceptionMessage = "Accounts list cannot be empty.";

	public UpdateAccountCommand(List<Domain.Core.Account> accounts)
	{
		ArgumentNullException.ThrowIfNull(accounts);

		if (accounts.Count == 0)
		{
			throw new ArgumentException(AccountsCannotBeEmptyExceptionMessage, nameof(accounts));
		}

		Accounts = accounts.AsReadOnly();
	}
}

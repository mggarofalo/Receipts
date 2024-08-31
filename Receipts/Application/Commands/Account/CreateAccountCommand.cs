using Application.Interfaces;

namespace Application.Commands.Account;

public record CreateAccountCommand : ICommand<List<Domain.Core.Account>>
{
	public IReadOnlyList<Domain.Core.Account> Accounts { get; }
	public const string AccountsCannotBeEmptyExceptionMessage = "Accounts list cannot be empty.";

	public CreateAccountCommand(List<Domain.Core.Account> accounts)
	{
		ArgumentNullException.ThrowIfNull(accounts);

		if (accounts.Count == 0)
		{
			throw new ArgumentException(AccountsCannotBeEmptyExceptionMessage, nameof(accounts));
		}

		Accounts = accounts.AsReadOnly();
	}
}

using Application.Interfaces;

namespace Application.Commands.Account.Create;

public record CreateAccountCommand : ICommand<List<Domain.Core.Account>>
{
	public IReadOnlyList<Domain.Core.Account> Accounts { get; }

	public const string AccountsListCannotBeEmpty = "Accounts list cannot be empty.";

	public CreateAccountCommand(List<Domain.Core.Account> accounts)
	{
		ArgumentNullException.ThrowIfNull(accounts);

		if (accounts.Count == 0)
		{
			throw new ArgumentException(AccountsListCannotBeEmpty, nameof(accounts));
		}

		Accounts = accounts.AsReadOnly();
	}
}

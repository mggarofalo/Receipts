using Application.Interfaces;

namespace Application.Commands.Account.Update;

public record UpdateAccountCommand : ICommand<bool>
{
	public IReadOnlyList<Domain.Core.Account> Accounts { get; }

	public const string AccountsListCannotBeEmpty = "Accounts list cannot be empty.";

	public UpdateAccountCommand(List<Domain.Core.Account> accounts)
	{
		ArgumentNullException.ThrowIfNull(accounts);

		if (accounts.Count == 0)
		{
			throw new ArgumentException(AccountsListCannotBeEmpty, nameof(accounts));
		}

		Accounts = accounts.AsReadOnly();
	}
}

using Application.Interfaces;

namespace Application.Queries.Core.Card;

public record GetCardsByAccountIdQuery : IQuery<List<Domain.Core.Card>>
{
	public Guid AccountId { get; }
	public const string AccountIdCannotBeEmptyExceptionMessage = "Account Id cannot be empty.";

	public GetCardsByAccountIdQuery(Guid accountId)
	{
		if (accountId == Guid.Empty)
		{
			throw new ArgumentException(AccountIdCannotBeEmptyExceptionMessage, nameof(accountId));
		}

		AccountId = accountId;
	}
}

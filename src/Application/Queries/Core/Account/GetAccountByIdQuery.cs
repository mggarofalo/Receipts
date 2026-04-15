using Application.Interfaces;

namespace Application.Queries.Core.Account;

public record GetAccountByIdQuery : IQuery<Domain.Core.Account?>
{
	public Guid Id { get; }
	public const string IdCannotBeEmptyExceptionMessage = "Account Id cannot be empty.";

	public GetAccountByIdQuery(Guid id)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(IdCannotBeEmptyExceptionMessage, nameof(id));
		}

		Id = id;
	}
}

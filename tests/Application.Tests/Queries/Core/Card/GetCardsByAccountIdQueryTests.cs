using Application.Queries.Core.Card;

namespace Application.Tests.Queries.Core.Card;

public class GetCardsByAccountIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid accountId = Guid.NewGuid();
		GetCardsByAccountIdQuery query = new(accountId);
		Assert.Equal(accountId, query.AccountId);
	}

	[Fact]
	public void Query_WithEmptyAccountId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetCardsByAccountIdQuery(Guid.Empty));
		Assert.StartsWith(GetCardsByAccountIdQuery.AccountIdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("accountId", exception.ParamName);
	}
}

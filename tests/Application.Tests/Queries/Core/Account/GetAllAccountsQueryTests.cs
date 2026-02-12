using Application.Queries.Core.Account;

namespace Application.Tests.Queries.Core.Account;

public class GetAllAccountsQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetAllAccountsQuery query = new();
		Assert.NotNull(query);
	}
}
using Application.Queries.Account;

namespace Application.Tests.Queries.Account;

public class GetAllAccountsQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetAllAccountsQuery query = new();
		Assert.NotNull(query);
	}
}
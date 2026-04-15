using Application.Models;
using Application.Queries.Core.Account;

namespace Application.Tests.Queries.Core.Account;

public class GetAllAccountsQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetAllAccountsQuery query = new(0, 50, SortParams.Default);
		Assert.NotNull(query);
	}
}

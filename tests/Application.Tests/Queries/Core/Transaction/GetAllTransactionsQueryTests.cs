using Application.Queries.Core.Transaction;

namespace Application.Tests.Queries.Core.Transaction;

public class GetAllTransactionsQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetAllTransactionsQuery query = new();
		Assert.NotNull(query);
	}
}
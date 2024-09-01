using Application.Queries.Transaction;

namespace Application.Tests.Queries.Transaction;

public class GetAllTransactionsQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetAllTransactionsQuery query = new();
		Assert.NotNull(query);
	}
}
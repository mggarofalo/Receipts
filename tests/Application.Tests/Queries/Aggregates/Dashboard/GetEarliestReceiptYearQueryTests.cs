using Application.Queries.Aggregates.Dashboard;

namespace Application.Tests.Queries.Aggregates.Dashboard;

public class GetEarliestReceiptYearQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetEarliestReceiptYearQuery query = new();
		Assert.NotNull(query);
	}
}

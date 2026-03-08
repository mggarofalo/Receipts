using Application.Queries.Aggregates.Dashboard;

namespace Application.Tests.Queries.Aggregates.Dashboard;

public class GetSpendingByAccountQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetSpendingByAccountQuery query = new(DateOnly.FromDateTime(DateTime.Today.AddDays(-30)), DateOnly.FromDateTime(DateTime.Today));
		Assert.NotNull(query);
	}
}

using Application.Queries.Aggregates.Dashboard;

namespace Application.Tests.Queries.Aggregates.Dashboard;

public class GetSpendingOverTimeQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetSpendingOverTimeQuery query = new(DateOnly.FromDateTime(DateTime.Today.AddDays(-30)), DateOnly.FromDateTime(DateTime.Today), "monthly");
		Assert.NotNull(query);
	}
}

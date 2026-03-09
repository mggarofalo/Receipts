using Application.Queries.Aggregates.Dashboard;

namespace Application.Tests.Queries.Aggregates.Dashboard;

public class GetSpendingByCategoryQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetSpendingByCategoryQuery query = new(DateOnly.FromDateTime(DateTime.Today.AddDays(-30)), DateOnly.FromDateTime(DateTime.Today), 10);
		Assert.NotNull(query);
	}
}

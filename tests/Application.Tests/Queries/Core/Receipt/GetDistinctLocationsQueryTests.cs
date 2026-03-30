using Application.Queries.Core.Receipt;

namespace Application.Tests.Queries.Core.Receipt;

public class GetDistinctLocationsQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetDistinctLocationsQuery query = new("Walmart", 20);
		Assert.NotNull(query);
	}

	[Fact]
	public void Query_CanBeCreated_WithNullQuery()
	{
		GetDistinctLocationsQuery query = new(null, 20);
		Assert.NotNull(query);
		Assert.Null(query.Query);
	}

	[Fact]
	public void Query_PreservesProperties()
	{
		GetDistinctLocationsQuery query = new("Wal", 10);
		Assert.Equal("Wal", query.Query);
		Assert.Equal(10, query.Limit);
	}
}

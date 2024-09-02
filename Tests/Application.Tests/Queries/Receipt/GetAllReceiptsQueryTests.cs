using Application.Queries.Receipt;

namespace Application.Tests.Queries.Receipt;

public class GetAllReceiptsQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetAllReceiptsQuery query = new();
		Assert.NotNull(query);
	}
}
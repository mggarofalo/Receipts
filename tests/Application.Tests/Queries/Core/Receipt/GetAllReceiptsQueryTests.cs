using Application.Queries.Core.Receipt;

namespace Application.Tests.Queries.Core.Receipt;

public class GetAllReceiptsQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetAllReceiptsQuery query = new();
		Assert.NotNull(query);
	}
}
using Application.Queries.Core.ItemTemplate;

namespace Application.Tests.Queries.Core.ItemTemplate;

public class GetAllItemTemplatesQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetAllItemTemplatesQuery query = new(0, 50);
		Assert.NotNull(query);
	}
}

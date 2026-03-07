using Application.Models;
using Application.Queries.Core.ItemTemplate;

namespace Application.Tests.Queries.Core.ItemTemplate;

public class GetDeletedItemTemplatesQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetDeletedItemTemplatesQuery query = new(0, 50, SortParams.Default);
		Assert.NotNull(query);
	}
}

using Application.Queries.Core.Category;

namespace Application.Tests.Queries.Core.Category;

public class GetAllCategoriesQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetAllCategoriesQuery query = new(0, 50);
		Assert.NotNull(query);
	}
}

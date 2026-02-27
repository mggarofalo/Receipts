using Application.Queries.Core.Subcategory;

namespace Application.Tests.Queries.Core.Subcategory;

public class GetAllSubcategoriesQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetAllSubcategoriesQuery query = new();
		Assert.NotNull(query);
	}
}

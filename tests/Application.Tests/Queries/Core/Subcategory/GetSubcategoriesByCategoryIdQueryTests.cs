using Application.Models;
using Application.Queries.Core.Subcategory;

namespace Application.Tests.Queries.Core.Subcategory;

public class GetSubcategoriesByCategoryIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid categoryId = Guid.NewGuid();
		GetSubcategoriesByCategoryIdQuery query = new(categoryId, 0, 50, SortParams.Default);
		Assert.Equal(categoryId, query.CategoryId);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetSubcategoriesByCategoryIdQuery(Guid.Empty, 0, 50, SortParams.Default));
		Assert.StartsWith(GetSubcategoriesByCategoryIdQuery.CategoryIdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("categoryId", exception.ParamName);
	}
}

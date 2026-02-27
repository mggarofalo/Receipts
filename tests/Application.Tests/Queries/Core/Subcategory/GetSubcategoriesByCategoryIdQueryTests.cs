using Application.Queries.Core.Subcategory;

namespace Application.Tests.Queries.Core.Subcategory;

public class GetSubcategoriesByCategoryIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid categoryId = Guid.NewGuid();
		GetSubcategoriesByCategoryIdQuery query = new(categoryId);
		Assert.Equal(categoryId, query.CategoryId);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetSubcategoriesByCategoryIdQuery(Guid.Empty));
		Assert.StartsWith(GetSubcategoriesByCategoryIdQuery.CategoryIdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("categoryId", exception.ParamName);
	}
}

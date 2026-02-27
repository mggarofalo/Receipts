using Application.Queries.Core.Subcategory;

namespace Application.Tests.Queries.Core.Subcategory;

public class GetSubcategoryByIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid id = Guid.NewGuid();
		GetSubcategoryByIdQuery query = new(id);
		Assert.Equal(id, query.Id);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetSubcategoryByIdQuery(Guid.Empty));
		Assert.StartsWith(GetSubcategoryByIdQuery.IdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("id", exception.ParamName);
	}
}

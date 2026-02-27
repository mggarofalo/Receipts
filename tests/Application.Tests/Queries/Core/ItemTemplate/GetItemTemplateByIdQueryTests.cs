using Application.Queries.Core.ItemTemplate;

namespace Application.Tests.Queries.Core.ItemTemplate;

public class GetItemTemplateByIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid id = Guid.NewGuid();
		GetItemTemplateByIdQuery query = new(id);
		Assert.Equal(id, query.Id);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetItemTemplateByIdQuery(Guid.Empty));
		Assert.StartsWith(GetItemTemplateByIdQuery.IdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("id", exception.ParamName);
	}
}

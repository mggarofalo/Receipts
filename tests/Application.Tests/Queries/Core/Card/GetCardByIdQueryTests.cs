using Application.Queries.Core.Card;

namespace Application.Tests.Queries.Core.Card;

public class GetCardByIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid id = Guid.NewGuid();
		GetCardByIdQuery query = new(id);
		Assert.Equal(id, query.Id);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetCardByIdQuery(Guid.Empty));
		Assert.StartsWith(GetCardByIdQuery.IdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("id", exception.ParamName);
	}
}
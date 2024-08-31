using Application.Queries.Transaction;

namespace Application.Tests.Queries.Transaction;

public class GetTransactionByIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_WithValidId_ReturnsValidQuery()
	{
		Guid id = Guid.NewGuid();
		GetTransactionByIdQuery query = new(id);
		Assert.Equal(id, query.Id);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetTransactionByIdQuery(Guid.Empty));
		Assert.Equal(GetTransactionByIdQuery.IdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("id", exception.ParamName);
	}
}
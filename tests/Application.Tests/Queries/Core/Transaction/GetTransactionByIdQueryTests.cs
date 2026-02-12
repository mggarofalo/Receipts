using Application.Queries.Core.Transaction;

namespace Application.Tests.Queries.Core.Transaction;

public class GetTransactionByIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid id = Guid.NewGuid();
		GetTransactionByIdQuery query = new(id);
		Assert.Equal(id, query.Id);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetTransactionByIdQuery(Guid.Empty));
		Assert.StartsWith(GetTransactionByIdQuery.IdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("id", exception.ParamName);
	}
}
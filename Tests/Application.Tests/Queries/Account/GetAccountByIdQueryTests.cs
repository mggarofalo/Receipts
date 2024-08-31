using Application.Queries.Account;

namespace Application.Tests.Queries.Account;

public class GetAccountByIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_WithValidId_ReturnsValidQuery()
	{
		Guid id = Guid.NewGuid();
		GetAccountByIdQuery query = new(id);
		Assert.Equal(id, query.Id);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetAccountByIdQuery(Guid.Empty));
		Assert.Equal(GetAccountByIdQuery.IdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("id", exception.ParamName);
	}
}
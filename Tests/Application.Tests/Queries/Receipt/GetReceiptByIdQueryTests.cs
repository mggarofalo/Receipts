using Application.Queries.Receipt;

namespace Application.Tests.Queries.Receipt;

public class GetReceiptByIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_WithValidId_ReturnsValidQuery()
	{
		Guid id = Guid.NewGuid();
		GetReceiptByIdQuery query = new(id);
		Assert.Equal(id, query.Id);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetReceiptByIdQuery(Guid.Empty));
		Assert.Equal(GetReceiptByIdQuery.IdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("id", exception.ParamName);
	}
}
using Application.Queries.ReceiptItem;

namespace Application.Tests.Queries.ReceiptItem;

public class GetReceiptItemByIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_WithValidId_ReturnsValidQuery()
	{
		Guid id = Guid.NewGuid();
		GetReceiptItemByIdQuery query = new(id);
		Assert.Equal(id, query.Id);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetReceiptItemByIdQuery(Guid.Empty));
		Assert.Equal(GetReceiptItemByIdQuery.IdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("id", exception.ParamName);
	}
}
using Application.Queries.Core.ReceiptItem;

namespace Application.Tests.Queries.Core.ReceiptItem;

public class GetReceiptItemByIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid id = Guid.NewGuid();
		GetReceiptItemByIdQuery query = new(id);
		Assert.Equal(id, query.Id);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetReceiptItemByIdQuery(Guid.Empty));
		Assert.StartsWith(GetReceiptItemByIdQuery.IdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("id", exception.ParamName);
	}
}
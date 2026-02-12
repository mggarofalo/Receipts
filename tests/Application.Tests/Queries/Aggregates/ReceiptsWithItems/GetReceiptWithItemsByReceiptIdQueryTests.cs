using Application.Queries.Aggregates.ReceiptsWithItems;

namespace Application.Tests.Queries.Aggregates.ReceiptsWithItems;

public class GetReceiptWithItemsByReceiptIdQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid id = Guid.NewGuid();
		GetReceiptWithItemsByReceiptIdQuery query = new(id);
		Assert.Equal(id, query.ReceiptId);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetReceiptWithItemsByReceiptIdQuery(Guid.Empty));
		Assert.StartsWith(GetReceiptWithItemsByReceiptIdQuery.ReceiptIdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("receiptId", exception.ParamName);
	}
}

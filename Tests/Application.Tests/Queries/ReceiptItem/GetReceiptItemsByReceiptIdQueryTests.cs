using Application.Queries.ReceiptItem;

namespace Application.Tests.Queries.ReceiptItem;

public class GetReceiptItemsByReceiptIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid receiptId = Guid.NewGuid();
		GetReceiptItemsByReceiptIdQuery query = new(receiptId);
		Assert.Equal(receiptId, query.ReceiptId);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetReceiptItemsByReceiptIdQuery(Guid.Empty));
		Assert.StartsWith(GetReceiptItemsByReceiptIdQuery.ReceiptIdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("receiptId", exception.ParamName);
	}
}
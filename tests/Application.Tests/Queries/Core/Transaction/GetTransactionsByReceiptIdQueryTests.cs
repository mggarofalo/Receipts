using Application.Queries.Core.Transaction;

namespace Application.Tests.Queries.Core.Transaction;

public class GetTransactionsByReceiptIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid receiptId = Guid.NewGuid();
		GetTransactionsByReceiptIdQuery query = new(receiptId);
		Assert.Equal(receiptId, query.ReceiptId);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetTransactionsByReceiptIdQuery(Guid.Empty));
		Assert.StartsWith(GetTransactionsByReceiptIdQuery.ReceiptIdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("receiptId", exception.ParamName);
	}
}
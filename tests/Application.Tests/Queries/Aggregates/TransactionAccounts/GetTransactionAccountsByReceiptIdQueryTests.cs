using Application.Queries.Aggregates.TransactionAccounts;

namespace Application.Tests.Queries.Aggregates.TransactionAccounts;

public class GetTransactionAccountsByReceiptIdQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid id = Guid.NewGuid();
		GetTransactionAccountsByReceiptIdQuery query = new(id);
		Assert.Equal(id, query.ReceiptId);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetTransactionAccountsByReceiptIdQuery(Guid.Empty));
		Assert.StartsWith(GetTransactionAccountsByReceiptIdQuery.ReceiptIdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("receiptId", exception.ParamName);
	}
}
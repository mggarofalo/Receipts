using Application.Queries.Aggregates.TransactionAccounts;

namespace Application.Tests.Queries.Aggregates.TransactionAccounts;

public class GetTransactionAccountByTransactionIdQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid id = Guid.NewGuid();
		GetTransactionAccountByTransactionIdQuery query = new(id);
		Assert.Equal(id, query.TransactionId);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetTransactionAccountByTransactionIdQuery(Guid.Empty));
		Assert.StartsWith(GetTransactionAccountByTransactionIdQuery.TransactionIdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("transactionId", exception.ParamName);
	}
}

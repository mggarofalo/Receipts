using Domain.Aggregates;
using Domain.Core;

namespace Domain.Tests.Aggregates;

public class TransactionAccountTests
{
	[Fact]
	public void TransactionAccount_ShouldHaveRequiredProperties()
	{
		// Arrange
		Transaction transaction = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Money(100), DateOnly.FromDateTime(DateTime.Now));
		Account account = new(Guid.NewGuid(), "Test Account", "Test Description");

		// Act
		TransactionAccount transactionAccount = new()
		{
			Transaction = transaction,
			Account = account
		};

		// Assert
		Assert.NotNull(transactionAccount.Transaction);
		Assert.NotNull(transactionAccount.Account);
	}
}
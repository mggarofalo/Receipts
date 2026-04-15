using Domain.Aggregates;
using Domain.Core;
using FluentAssertions;
using SampleData.Domain.Core;

namespace Domain.Tests.Aggregates;

public class TransactionAccountTests
{
	[Fact]
	public void TransactionAccount_ShouldHaveRequiredProperties()
	{
		// Arrange
		Transaction transaction = TransactionGenerator.Generate();
		Account account = AccountGenerator.Generate();

		// Act
		TransactionAccount transactionAccount = new()
		{
			Transaction = transaction,
			Account = account
		};

		// Assert
		Assert.NotNull(transactionAccount.Transaction);
		transactionAccount.Transaction.Should().BeSameAs(transaction);
		Assert.NotNull(transactionAccount.Account);
		transactionAccount.Account.Should().BeSameAs(account);
	}
}

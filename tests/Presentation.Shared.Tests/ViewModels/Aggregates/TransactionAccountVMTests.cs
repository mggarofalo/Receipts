using FluentAssertions;
using SampleData.ViewModels.Core;
using Shared.ViewModels.Aggregates;
using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.ViewModels.Aggregates;

public class TransactionAccountVMTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesTransactionAccountVM()
	{
		// Arrange
		AccountVM account = AccountVMGenerator.Generate();
		TransactionVM transaction = TransactionVMGenerator.Generate();

		// Act
		TransactionAccountVM transactionAccountVM = new()
		{
			Transaction = transaction,
			Account = account
		};

		// Assert
		transactionAccountVM.Transaction.Should().BeSameAs(transaction);
		transactionAccountVM.Account.Should().BeSameAs(account);
	}
}

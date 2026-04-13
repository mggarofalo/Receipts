using Application.Interfaces.Services;
using Application.Queries.Aggregates.TransactionAccounts;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Aggregates.TransactionAccounts;

public class GetTransactionAccountsByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnTransactionAccounts_WhenTransactionsAndAccountsExist()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<Domain.Core.Card> expectedAccounts = CardGenerator.GenerateList(3);
		List<Domain.Core.Transaction> expectedTransactions = TransactionGenerator.GenerateList(3);
		List<Domain.Aggregates.TransactionAccount> expectedResult = expectedTransactions
			.Zip(expectedAccounts, (t, a) => new Domain.Aggregates.TransactionAccount { Transaction = t, Account = a })
			.ToList();

		Mock<ITransactionService> mockTransactionService = new();
		mockTransactionService.Setup(r => r.GetTransactionAccountsByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionService.Object);
		GetTransactionAccountsByReceiptIdQuery query = new(receiptId);

		// Act
		List<Domain.Aggregates.TransactionAccount>? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		result.Should().HaveCount(expectedTransactions.Count);
		foreach (Domain.Aggregates.TransactionAccount resultTransactionAccount in result)
		{
			resultTransactionAccount.Transaction.Should().BeSameAs(expectedTransactions.First(t => t.Id == resultTransactionAccount.Transaction.Id));
			resultTransactionAccount.Account.Should().BeSameAs(expectedAccounts.First(a => a.Id == resultTransactionAccount.Account.Id));
		}
	}

	[Fact]
	public async Task Handle_ShouldReturnEmptyList_WhenReceiptDoesNotExist()
	{
		// Arrange
		Guid missingReceiptId = Guid.NewGuid();

		Mock<ITransactionService> mockTransactionService = new();
		mockTransactionService.Setup(r => r.GetTransactionAccountsByReceiptIdAsync(missingReceiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionService.Object);
		GetTransactionAccountsByReceiptIdQuery query = new(missingReceiptId);

		// Act
		List<Domain.Aggregates.TransactionAccount>? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		Assert.Empty(result);
	}

	[Fact]
	public async Task Handle_ShouldReturnEmptyList_WhenReceiptExistsButHasNoTransactions()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();

		Mock<ITransactionService> mockTransactionService = new();
		mockTransactionService.Setup(r => r.GetTransactionAccountsByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionService.Object);
		GetTransactionAccountsByReceiptIdQuery query = new(receiptId);

		// Act
		List<Domain.Aggregates.TransactionAccount>? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		Assert.Empty(result);
	}
}

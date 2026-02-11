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
		List<Domain.Core.Account> expectedAccounts = AccountGenerator.GenerateList(3);
		List<Domain.Core.Transaction> expectedTransactions = TransactionGenerator.GenerateList(3);

		Mock<ITransactionService> mockTransactionService = new();
		mockTransactionService.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync(expectedTransactions);

		Mock<IAccountService> mockAccountService = new();
		for (int i = 0; i < expectedTransactions.Count; i++)
		{
			mockAccountService.Setup(r => r.GetByTransactionIdAsync(expectedTransactions[i].Id, It.IsAny<CancellationToken>())).ReturnsAsync(expectedAccounts[i]);
		}

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionService.Object, mockAccountService.Object);
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
	public async Task Handle_ShouldReturnNull_WhenReceiptDoesNotExist()
	{
		// Arrange
		Guid missingReceiptId = Guid.NewGuid();

		Mock<ITransactionService> mockTransactionService = new();
		mockTransactionService.Setup(r => r.GetByReceiptIdAsync(missingReceiptId, It.IsAny<CancellationToken>())).ReturnsAsync((List<Domain.Core.Transaction>?)null);

		Mock<IAccountService> mockAccountService = new();

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionService.Object, mockAccountService.Object);
		GetTransactionAccountsByReceiptIdQuery query = new(missingReceiptId);

		// Act
		List<Domain.Aggregates.TransactionAccount>? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task Handle_ShouldReturnEmptyList_WhenTransactionsExistButAccountsDoNotExist()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<Domain.Core.Transaction> expectedTransactions = TransactionGenerator.GenerateList(3);

		Mock<ITransactionService> mockTransactionService = new();
		mockTransactionService.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync(expectedTransactions);

		Mock<IAccountService> mockAccountService = new();
		foreach (Domain.Core.Transaction transaction in expectedTransactions)
		{
			mockAccountService.Setup(r => r.GetByTransactionIdAsync(transaction.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Account?)null);
		}

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionService.Object, mockAccountService.Object);
		GetTransactionAccountsByReceiptIdQuery query = new(receiptId);

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
		List<Domain.Core.Transaction> expectedTransactions = [];

		Mock<ITransactionService> mockTransactionService = new();
		mockTransactionService.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync(expectedTransactions);

		Mock<IAccountService> mockAccountService = new();

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionService.Object, mockAccountService.Object);
		GetTransactionAccountsByReceiptIdQuery query = new(receiptId);

		// Act
		List<Domain.Aggregates.TransactionAccount>? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		Assert.Empty(result);
	}
}

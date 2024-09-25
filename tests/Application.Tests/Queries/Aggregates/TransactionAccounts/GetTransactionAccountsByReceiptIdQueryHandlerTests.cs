using Application.Interfaces.Repositories;
using Application.Queries.Aggregates.TransactionAccounts;
using Moq;
using SampleData.Domain.Core;
using Xunit.Sdk;

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

		Mock<ITransactionRepository> mockTransactionRepository = new();
		mockTransactionRepository.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync(expectedTransactions);

		Mock<IAccountRepository> mockAccountRepository = new();
		for (int i = 0; i < expectedTransactions.Count; i++)
		{
			mockAccountRepository.Setup(r => r.GetByTransactionIdAsync(expectedTransactions[i].Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(expectedAccounts[i]);
		}

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionRepository.Object, mockAccountRepository.Object);
		GetTransactionAccountsByReceiptIdQuery query = new(receiptId);

		// Act
		List<Domain.Aggregates.TransactionAccount>? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(expectedTransactions.Count, result.Count);
		foreach (Domain.Aggregates.TransactionAccount resultTransactionAccount in result)
		{
			Assert.Equal(expectedTransactions.First(t => t.Id == resultTransactionAccount.Transaction.Id), resultTransactionAccount.Transaction);
			Assert.Equal(expectedAccounts.First(a => a.Id == resultTransactionAccount.Account.Id), resultTransactionAccount.Account);
		}
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenReceiptDoesNotExist()
	{
		// Arrange
		Guid missingReceiptId = Guid.NewGuid();

		Mock<ITransactionRepository> mockTransactionRepository = new();
		mockTransactionRepository.Setup(r => r.GetByReceiptIdAsync(missingReceiptId, It.IsAny<CancellationToken>())).ReturnsAsync((List<Domain.Core.Transaction>?)null);

		Mock<IAccountRepository> mockAccountRepository = new();

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionRepository.Object, mockAccountRepository.Object);
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

		Mock<ITransactionRepository> mockTransactionRepository = new();
		mockTransactionRepository.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync(expectedTransactions);

		Mock<IAccountRepository> mockAccountRepository = new();
		foreach (Domain.Core.Transaction transaction in expectedTransactions)
		{
			mockAccountRepository.Setup(r => r.GetByTransactionIdAsync(transaction.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Account?)null);
		}

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionRepository.Object, mockAccountRepository.Object);
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

		Mock<ITransactionRepository> mockTransactionRepository = new();
		mockTransactionRepository.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync(expectedTransactions);

		Mock<IAccountRepository> mockAccountRepository = new();

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionRepository.Object, mockAccountRepository.Object);
		GetTransactionAccountsByReceiptIdQuery query = new(receiptId);

		// Act
		List<Domain.Aggregates.TransactionAccount>? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		Assert.Empty(result);
	}
}
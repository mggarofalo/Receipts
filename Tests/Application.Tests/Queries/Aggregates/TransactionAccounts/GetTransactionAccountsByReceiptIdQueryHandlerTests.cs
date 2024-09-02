using Application.Interfaces.Repositories;
using Application.Queries.Aggregates.TransactionAccounts;
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
		List<Domain.Core.Account> accounts = AccountGenerator.GenerateList(3);
		List<Domain.Core.Transaction> transactions = [];

		foreach (Domain.Core.Account account in accounts)
		{
			transactions.Add(TransactionGenerator.Generate(receiptId, account.Id));
		}

		Mock<ITransactionRepository> mockTransactionRepository = new();
		mockTransactionRepository.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync(transactions);

		Mock<IAccountRepository> mockAccountRepository = new();
		foreach (Domain.Core.Account account in accounts)
		{
			mockAccountRepository.Setup(r => r.GetByIdAsync(account.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(account);
		}

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionRepository.Object, mockAccountRepository.Object);
		GetTransactionAccountsByReceiptIdQuery query = new(receiptId);

		// Act
		List<Domain.Aggregates.TransactionAccount>? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(transactions.Count, result.Count);
		foreach (Domain.Aggregates.TransactionAccount transactionAccount in result)
		{
			Assert.Equal(transactions.First(t => t.Id == transactionAccount.Transaction.Id), transactionAccount.Transaction);
			Assert.Equal(accounts.First(a => a.Id == transactionAccount.Account.Id), transactionAccount.Account);
		}

		mockTransactionRepository.Verify(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>()), Times.Once);
		mockAccountRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(transactions.Count));

		foreach (Domain.Core.Transaction transaction in transactions)
		{
			mockAccountRepository.Verify(r => r.GetByIdAsync(transaction.AccountId!, It.IsAny<CancellationToken>()), Times.Once);
		}
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenReceiptDoesNotExist()
	{
		// Arrange
		Guid missingReceiptId = Guid.NewGuid();
		List<Domain.Core.Transaction> emptyTransactions = [];

		Mock<ITransactionRepository> mockTransactionRepository = new();
		mockTransactionRepository.Setup(r => r.GetByReceiptIdAsync(missingReceiptId, It.IsAny<CancellationToken>())).ReturnsAsync((List<Domain.Core.Transaction>?)null);

		Mock<IAccountRepository> mockAccountRepository = new();

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionRepository.Object, mockAccountRepository.Object);
		GetTransactionAccountsByReceiptIdQuery query = new(missingReceiptId);

		// Act
		List<Domain.Aggregates.TransactionAccount>? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.Null(result);
		mockTransactionRepository.Verify(r => r.GetByReceiptIdAsync(missingReceiptId, It.IsAny<CancellationToken>()), Times.Once);
		mockAccountRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenTransactionsExistButAccountsDoNotExist()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<Domain.Core.Transaction> transactions = TransactionGenerator.GenerateList(3);
		List<Domain.Core.Account> accounts = [];

		Mock<ITransactionRepository> mockTransactionRepository = new();
		mockTransactionRepository.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync(transactions);

		Mock<IAccountRepository> mockAccountRepository = new();
		foreach (Domain.Core.Transaction transaction in transactions)
		{
			mockAccountRepository.Setup(r => r.GetByIdAsync(transaction.AccountId!, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Account?)null);
		}

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionRepository.Object, mockAccountRepository.Object);
		GetTransactionAccountsByReceiptIdQuery query = new(receiptId);

		// Act
		List<Domain.Aggregates.TransactionAccount>? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.Null(result);

		mockTransactionRepository.Verify(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>()), Times.Once);
		mockAccountRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldReturnEmptyList_WhenReceiptExistsButHasNoTransactions()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<Domain.Core.Transaction> emptyTransactions = [];

		Mock<ITransactionRepository> mockTransactionRepository = new();
		mockTransactionRepository.Setup(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>())).ReturnsAsync(emptyTransactions);

		Mock<IAccountRepository> mockAccountRepository = new();

		GetTransactionAccountsByReceiptIdQueryHandler handler = new(mockTransactionRepository.Object, mockAccountRepository.Object);
		GetTransactionAccountsByReceiptIdQuery query = new(receiptId);

		// Act
		List<Domain.Aggregates.TransactionAccount>? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		Assert.Empty(result);
		mockTransactionRepository.Verify(r => r.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>()), Times.Once);
		mockAccountRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}

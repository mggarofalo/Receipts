using Application.Interfaces.Repositories;
using Application.Queries.Aggregates.TransactionAccounts;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Aggregates.TransactionAccounts;

public class GetTransactionAccountByTransactionIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnTransactionAccount_WhenTransactionAndAccountExist()
	{
		// Arrange
		Domain.Core.Account account = AccountGenerator.Generate();
		Domain.Core.Transaction transaction = TransactionGenerator.Generate(accountId: account.Id);

		Mock<ITransactionRepository> mockTransactionRepository = new();
		mockTransactionRepository.Setup(r => r.GetByIdAsync(transaction.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);

		Mock<IAccountRepository> mockAccountRepository = new();
		mockAccountRepository.Setup(r => r.GetByIdAsync(account.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(account);

		GetTransactionAccountByTransactionIdQueryHandler handler = new(mockTransactionRepository.Object, mockAccountRepository.Object);
		GetTransactionAccountByTransactionIdQuery query = new(transaction.Id!.Value);

		// Act
		Domain.Aggregates.TransactionAccount? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(transaction, result.Transaction);
		Assert.Equal(account, result.Account);
		mockTransactionRepository.Verify(r => r.GetByIdAsync(transaction.Id!.Value, It.IsAny<CancellationToken>()), Times.Once);
		mockAccountRepository.Verify(r => r.GetByIdAsync(account.Id!.Value, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenTransactionDoesNotExist()
	{
		// Arrange
		Guid missingTransactionId = Guid.NewGuid();
		Mock<ITransactionRepository> mockTransactionRepository = new();
		mockTransactionRepository.Setup(r => r.GetByIdAsync(missingTransactionId, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Transaction?)null);

		Mock<IAccountRepository> mockAccountRepository = new();

		GetTransactionAccountByTransactionIdQueryHandler handler = new(mockTransactionRepository.Object, mockAccountRepository.Object);
		GetTransactionAccountByTransactionIdQuery query = new(missingTransactionId);

		// Act
		Domain.Aggregates.TransactionAccount? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.Null(result);
		mockTransactionRepository.Verify(r => r.GetByIdAsync(missingTransactionId, It.IsAny<CancellationToken>()), Times.Once);
		mockAccountRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenAccountDoesNotExist()
	{
		// Arrange
		Guid missingAccountId = Guid.NewGuid();
		Domain.Core.Transaction transaction = TransactionGenerator.Generate(accountId: missingAccountId);

		Mock<ITransactionRepository> mockTransactionRepository = new();
		mockTransactionRepository.Setup(r => r.GetByIdAsync(transaction.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);

		Mock<IAccountRepository> mockAccountRepository = new();
		mockAccountRepository.Setup(r => r.GetByIdAsync(missingAccountId, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Account?)null);

		GetTransactionAccountByTransactionIdQueryHandler handler = new(mockTransactionRepository.Object, mockAccountRepository.Object);
		GetTransactionAccountByTransactionIdQuery query = new(transaction.Id!.Value);

		// Act
		Domain.Aggregates.TransactionAccount? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.Null(result);
		mockTransactionRepository.Verify(r => r.GetByIdAsync(transaction.Id!.Value, It.IsAny<CancellationToken>()), Times.Once);
		mockAccountRepository.Verify(r => r.GetByIdAsync(missingAccountId, It.IsAny<CancellationToken>()), Times.Once);
	}
}

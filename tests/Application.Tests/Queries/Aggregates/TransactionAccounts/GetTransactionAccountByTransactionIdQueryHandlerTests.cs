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
		Domain.Core.Account expectedAccount = AccountGenerator.Generate();
		Domain.Core.Transaction expectedTransaction = TransactionGenerator.Generate();

		Mock<ITransactionRepository> mockTransactionRepository = new();
		mockTransactionRepository.Setup(r => r.GetByIdAsync(expectedTransaction.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(expectedTransaction);

		Mock<IAccountRepository> mockAccountRepository = new();
		mockAccountRepository.Setup(r => r.GetByTransactionIdAsync(expectedTransaction.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(expectedAccount);

		GetTransactionAccountByTransactionIdQueryHandler handler = new(mockTransactionRepository.Object, mockAccountRepository.Object);
		GetTransactionAccountByTransactionIdQuery query = new(expectedTransaction.Id!.Value);

		// Act
		Domain.Aggregates.TransactionAccount? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(expectedTransaction, result.Transaction);
		Assert.Equal(expectedAccount, result.Account);
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
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenAccountDoesNotExist()
	{
		// Arrange
		Guid missingAccountId = Guid.NewGuid();
		Domain.Core.Transaction expectedTransaction = TransactionGenerator.Generate();

		Mock<ITransactionRepository> mockTransactionRepository = new();
		mockTransactionRepository.Setup(r => r.GetByIdAsync(expectedTransaction.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(expectedTransaction);

		Mock<IAccountRepository> mockAccountRepository = new();
		mockAccountRepository.Setup(r => r.GetByIdAsync(missingAccountId, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Account?)null);

		GetTransactionAccountByTransactionIdQueryHandler handler = new(mockTransactionRepository.Object, mockAccountRepository.Object);
		GetTransactionAccountByTransactionIdQuery query = new(expectedTransaction.Id!.Value);

		// Act
		Domain.Aggregates.TransactionAccount? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.Null(result);
	}
}

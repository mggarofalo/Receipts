using Application.Interfaces.Services;
using Application.Queries.Aggregates.TransactionAccounts;
using FluentAssertions;
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

		Mock<ITransactionService> mockTransactionService = new();
		mockTransactionService.Setup(r => r.GetByIdAsync(expectedTransaction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(expectedTransaction);

		Mock<IAccountService> mockAccountService = new();
		mockAccountService.Setup(r => r.GetByTransactionIdAsync(expectedTransaction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(expectedAccount);

		GetTransactionAccountByTransactionIdQueryHandler handler = new(mockTransactionService.Object, mockAccountService.Object);
		GetTransactionAccountByTransactionIdQuery query = new(expectedTransaction.Id);

		// Act
		Domain.Aggregates.TransactionAccount? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		result.Transaction.Should().BeSameAs(expectedTransaction);
		result.Account.Should().BeSameAs(expectedAccount);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenTransactionDoesNotExist()
	{
		// Arrange
		Guid missingTransactionId = Guid.NewGuid();
		Mock<ITransactionService> mockTransactionService = new();
		mockTransactionService.Setup(r => r.GetByIdAsync(missingTransactionId, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Transaction?)null);

		Mock<IAccountService> mockAccountService = new();

		GetTransactionAccountByTransactionIdQueryHandler handler = new(mockTransactionService.Object, mockAccountService.Object);
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

		Mock<ITransactionService> mockTransactionService = new();
		mockTransactionService.Setup(r => r.GetByIdAsync(expectedTransaction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(expectedTransaction);

		Mock<IAccountService> mockAccountService = new();
		mockAccountService.Setup(r => r.GetByIdAsync(missingAccountId, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Account?)null);

		GetTransactionAccountByTransactionIdQueryHandler handler = new(mockTransactionService.Object, mockAccountService.Object);
		GetTransactionAccountByTransactionIdQuery query = new(expectedTransaction.Id);

		// Act
		Domain.Aggregates.TransactionAccount? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.Null(result);
	}
}

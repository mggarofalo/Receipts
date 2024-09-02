using Application.Interfaces.Repositories;
using Application.Queries.Transaction;
using Domain;
using Moq;

namespace Application.Tests.Queries.Transaction;

public class GetTransactionsByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnTransactions_WhenReceiptExistsAndHasItems()
	{
		Guid receiptId = Guid.NewGuid();

		List<Domain.Core.Transaction> transactions =
		[
			new(Guid.NewGuid(), receiptId, Guid.NewGuid(), new Money(100), new DateOnly(2024, 1, 1)),
			new(Guid.NewGuid(), receiptId, Guid.NewGuid(), new Money(200), new DateOnly(2024, 1, 2))
		];

		Mock<ITransactionRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByReceiptIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(transactions);

		GetTransactionsByReceiptIdQueryHandler handler = new(mockRepository.Object);
		GetTransactionsByReceiptIdQuery query = new(receiptId);

		List<Domain.Core.Transaction>? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(transactions.Count, result.Count);
		Assert.True(transactions.All(t => result.Any(rt =>
			rt.Id == t.Id &&
			rt.ReceiptId == t.ReceiptId &&
			rt.AccountId == t.AccountId &&
			rt.Amount == t.Amount &&
			rt.Date == t.Date)));

		mockRepository.Verify(r => r.GetByReceiptIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldReturnEmptyList_WhenReceiptHasNoItems()
	{
		Mock<ITransactionRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByReceiptIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

		GetTransactionsByReceiptIdQueryHandler handler = new(mockRepository.Object);
		GetTransactionsByReceiptIdQuery query = new(Guid.NewGuid());

		List<Domain.Core.Transaction>? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Empty(result);
		mockRepository.Verify(r => r.GetByReceiptIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenReceiptDoesNotExist()
	{
		Mock<ITransactionRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByReceiptIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((List<Domain.Core.Transaction>?)null);

		GetTransactionsByReceiptIdQueryHandler handler = new(mockRepository.Object);
		GetTransactionsByReceiptIdQuery query = new(Guid.NewGuid());

		List<Domain.Core.Transaction>? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
		mockRepository.Verify(r => r.GetByReceiptIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
	}
}
using Application.Interfaces.Repositories;
using Application.Queries.Transaction;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Queries.Transaction;

public class GetTransactionsByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnTransactions_WhenReceiptExistsAndHasItems()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		List<Domain.Core.Transaction> transactions = TransactionGenerator.GenerateList(2, receipt.Id!.Value);

		Mock<ITransactionRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(transactions);

		GetTransactionsByReceiptIdQueryHandler handler = new(mockRepository.Object);
		GetTransactionsByReceiptIdQuery query = new(receipt.Id!.Value);

		List<Domain.Core.Transaction>? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(transactions.Count, result.Count);
		Assert.True(transactions.All(t => result.Any(rt =>
			rt.Id == t.Id &&
			rt.ReceiptId == t.ReceiptId &&
			rt.AccountId == t.AccountId &&
			rt.Amount == t.Amount &&
			rt.Date == t.Date)));

		mockRepository.Verify(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldReturnEmptyList_WhenReceiptHasNoItems()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();

		Mock<ITransactionRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync([]);

		GetTransactionsByReceiptIdQueryHandler handler = new(mockRepository.Object);
		GetTransactionsByReceiptIdQuery query = new(receipt.Id!.Value);

		List<Domain.Core.Transaction>? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Empty(result);
		mockRepository.Verify(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenReceiptDoesNotExist()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();

		Mock<ITransactionRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync((List<Domain.Core.Transaction>?)null);

		GetTransactionsByReceiptIdQueryHandler handler = new(mockRepository.Object);
		GetTransactionsByReceiptIdQuery query = new(receipt.Id!.Value);

		List<Domain.Core.Transaction>? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
		mockRepository.Verify(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>()), Times.Once);
	}
}
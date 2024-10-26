using SampleData.Domain.Core;
using Moq;
using Application.Queries.Core.Transaction;
using Application.Interfaces.Services;

namespace Application.Tests.Queries.Core.Transaction;

public class GetTransactionsByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnTransactions_WhenReceiptExistsAndHasItems()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		List<Domain.Core.Transaction> expected = TransactionGenerator.GenerateList(2);

		Mock<ITransactionService> mockService = new();
		mockService.Setup(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetTransactionsByReceiptIdQueryHandler handler = new(mockService.Object);
		GetTransactionsByReceiptIdQuery query = new(receipt.Id!.Value);

		List<Domain.Core.Transaction>? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(expected.Count, result.Count);
		Assert.True(expected.All(result.Contains));
		Assert.True(result.All(expected.Contains));
	}

	[Fact]
	public async Task Handle_ShouldReturnEmptyList_WhenReceiptHasNoItems()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();

		Mock<ITransactionService> mockService = new();
		mockService.Setup(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync([]);

		GetTransactionsByReceiptIdQueryHandler handler = new(mockService.Object);
		GetTransactionsByReceiptIdQuery query = new(receipt.Id!.Value);

		List<Domain.Core.Transaction>? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Empty(result);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenReceiptDoesNotExist()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();

		Mock<ITransactionService> mockService = new();
		mockService.Setup(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync((List<Domain.Core.Transaction>?)null);

		GetTransactionsByReceiptIdQueryHandler handler = new(mockService.Object);
		GetTransactionsByReceiptIdQuery query = new(receipt.Id!.Value);

		List<Domain.Core.Transaction>? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
	}
}
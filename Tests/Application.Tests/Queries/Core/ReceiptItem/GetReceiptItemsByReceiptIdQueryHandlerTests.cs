using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;
using Application.Queries.Core.ReceiptItem;

namespace Application.Tests.Queries.Core.ReceiptItem;

public class GetReceiptItemsByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnReceiptItems_WhenReceiptExistsAndHasItems()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		List<Domain.Core.ReceiptItem> receiptItems = ReceiptItemGenerator.GenerateList(2, receipt.Id!.Value);

		Mock<IReceiptItemRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(receiptItems);

		GetReceiptItemsByReceiptIdQueryHandler handler = new(mockRepository.Object);
		GetReceiptItemsByReceiptIdQuery query = new(receipt.Id!.Value);
		List<Domain.Core.ReceiptItem>? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(receiptItems.Count, result.Count);
		Assert.True(receiptItems.All(ri => result.Any(rr =>
			rr.ReceiptId == ri.ReceiptId &&
			rr.ReceiptItemCode == ri.ReceiptItemCode &&
			rr.Description == ri.Description &&
			rr.Quantity == ri.Quantity &&
			rr.UnitPrice == ri.UnitPrice &&
			rr.TotalAmount == ri.TotalAmount &&
			rr.Category == ri.Category &&
			rr.Subcategory == ri.Subcategory)));

		mockRepository.Verify(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldReturnEmptyList_WhenReceiptHasNoItems()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();

		Mock<IReceiptItemRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync([]);

		GetReceiptItemsByReceiptIdQueryHandler handler = new(mockRepository.Object);
		GetReceiptItemsByReceiptIdQuery query = new(receipt.Id!.Value);
		List<Domain.Core.ReceiptItem>? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Empty(result);
		mockRepository.Verify(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenReceiptDoesNotExist()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();

		Mock<IReceiptItemRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync((List<Domain.Core.ReceiptItem>?)null);

		GetReceiptItemsByReceiptIdQueryHandler handler = new(mockRepository.Object);
		GetReceiptItemsByReceiptIdQuery query = new(receipt.Id!.Value);

		List<Domain.Core.ReceiptItem>? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
		mockRepository.Verify(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>()), Times.Once);
	}
}
using Application.Interfaces.Repositories;
using Application.Queries.ReceiptItem;
using Domain;
using Moq;

namespace Application.Tests.Queries.ReceiptItem;

public class GetReceiptItemsByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnReceiptItems_WhenReceiptExistsAndHasItems()
	{
		Guid receiptId = Guid.NewGuid();
		List<Domain.Core.ReceiptItem> receiptItems =
		[
			new(Guid.NewGuid(), receiptId, "Test Receipt Item 1", "Test Receipt Item 1", 1, new Money(10), new Money(10), "Test Category 1", "Test Subcategory 1")
		];

		Mock<IReceiptItemRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByReceiptIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(receiptItems);

		GetReceiptItemsByReceiptIdQueryHandler handler = new(mockRepository.Object);
		GetReceiptItemsByReceiptIdQuery query = new(receiptId);

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

		mockRepository.Verify(r => r.GetByReceiptIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldReturnEmptyList_WhenReceiptHasNoItems()
	{
		Mock<IReceiptItemRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByReceiptIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

		GetReceiptItemsByReceiptIdQueryHandler handler = new(mockRepository.Object);
		GetReceiptItemsByReceiptIdQuery query = new(Guid.NewGuid());

		List<Domain.Core.ReceiptItem>? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Empty(result);
		mockRepository.Verify(r => r.GetByReceiptIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenReceiptDoesNotExist()
	{
		Mock<IReceiptItemRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByReceiptIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((List<Domain.Core.ReceiptItem>?)null);

		GetReceiptItemsByReceiptIdQueryHandler handler = new(mockRepository.Object);
		GetReceiptItemsByReceiptIdQuery query = new(Guid.NewGuid());

		List<Domain.Core.ReceiptItem>? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
		mockRepository.Verify(r => r.GetByReceiptIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
	}
}
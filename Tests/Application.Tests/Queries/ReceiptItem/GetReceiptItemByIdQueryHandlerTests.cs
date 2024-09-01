using Moq;
using Application.Queries.ReceiptItem;
using Application.Interfaces.Repositories;
using Domain;

namespace Application.Tests.Queries.ReceiptItem;

public class GetReceiptItemByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnReceiptItem_WhenReceiptItemExists()
	{
		Guid receiptItemId = Guid.NewGuid();
		Domain.Core.ReceiptItem receiptItem = new(receiptItemId, Guid.NewGuid(), "Test Receipt Item 1", "Test Receipt Item 1", 1, new Money(10), new Money(10), "Test Category 1", "Test Subcategory 1");

		Mock<IReceiptItemRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(receiptItem);

		GetReceiptItemByIdQueryHandler handler = new(mockRepository.Object);
		GetReceiptItemByIdQuery query = new(receiptItemId);

		Domain.Core.ReceiptItem? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenAccountDoesNotExist()
	{
		Mock<IReceiptItemRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.ReceiptItem?)null);

		GetReceiptItemByIdQueryHandler handler = new(mockRepository.Object);
		GetReceiptItemByIdQuery query = new(Guid.NewGuid());

		Domain.Core.ReceiptItem? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
		mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
	}
}
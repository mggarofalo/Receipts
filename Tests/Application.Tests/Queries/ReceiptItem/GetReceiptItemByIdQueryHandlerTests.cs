using Application.Interfaces.Repositories;
using Application.Queries.ReceiptItem;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Queries.ReceiptItem;

public class GetReceiptItemByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnReceiptItem_WhenReceiptItemExists()
	{
		Domain.Core.ReceiptItem receiptItem = ReceiptItemGenerator.Generate();

		Mock<IReceiptItemRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(receiptItem);

		GetReceiptItemByIdQueryHandler handler = new(mockRepository.Object);
		GetReceiptItemByIdQuery query = new(receiptItem.Id!.Value);
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
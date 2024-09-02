using Application.Interfaces.Repositories;
using Application.Queries.Aggregates.ReceiptsWithItems;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Aggregates.ReceiptsWithItems;

public class GetReceiptWithItemsByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnReceiptWithItems_WhenReceiptExists()
	{
		// Arrange
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		List<Domain.Core.ReceiptItem> receiptItems = ReceiptItemGenerator.GenerateList(3);

		Mock<IReceiptRepository> mockReceiptRepository = new();
		mockReceiptRepository.Setup(r => r.GetByIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(receipt);

		Mock<IReceiptItemRepository> mockReceiptItemRepository = new();
		mockReceiptItemRepository.Setup(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(receiptItems);

		GetReceiptWithItemsByReceiptIdQueryHandler handler = new(mockReceiptRepository.Object, mockReceiptItemRepository.Object);
		GetReceiptWithItemsByReceiptIdQuery query = new(receipt.Id!.Value);

		// Act
		Domain.Aggregates.ReceiptWithItems? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receipt, result.Receipt);
		Assert.Equal(receiptItems, result.Items);
		mockReceiptRepository.Verify(r => r.GetByIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>()), Times.Once);
		mockReceiptItemRepository.Verify(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenReceiptDoesNotExist()
	{
		// Arrange
		Guid missingReceiptId = Guid.NewGuid();
		Mock<IReceiptRepository> mockReceiptRepository = new();
		mockReceiptRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Receipt?)null);

		Mock<IReceiptItemRepository> mockReceiptItemRepository = new();

		GetReceiptWithItemsByReceiptIdQueryHandler handler = new(mockReceiptRepository.Object, mockReceiptItemRepository.Object);
		GetReceiptWithItemsByReceiptIdQuery query = new(missingReceiptId);

		// Act
		Domain.Aggregates.ReceiptWithItems? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.Null(result);
		mockReceiptRepository.Verify(r => r.GetByIdAsync(missingReceiptId, It.IsAny<CancellationToken>()), Times.Once);
		mockReceiptItemRepository.Verify(r => r.GetByReceiptIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}

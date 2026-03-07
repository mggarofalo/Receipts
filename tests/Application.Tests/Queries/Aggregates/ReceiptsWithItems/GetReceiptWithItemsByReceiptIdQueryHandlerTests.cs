using Application.Interfaces.Services;
using Application.Models;
using Application.Queries.Aggregates.ReceiptsWithItems;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Aggregates.ReceiptsWithItems;

public class GetReceiptWithItemsByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnReceiptWithItems_WhenReceiptExists()
	{
		// Arrange
		Domain.Core.Receipt expectedReceipt = ReceiptGenerator.Generate();
		List<Domain.Core.ReceiptItem> expectedReceiptItems = ReceiptItemGenerator.GenerateList(3);
		List<Domain.Core.Adjustment> expectedAdjustments = AdjustmentGenerator.GenerateList(2);

		Mock<IReceiptService> mockReceiptService = new();
		mockReceiptService.Setup(r => r.GetByIdAsync(expectedReceipt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(expectedReceipt);

		Mock<IReceiptItemService> mockReceiptItemService = new();
		mockReceiptItemService.Setup(r => r.GetByReceiptIdAsync(expectedReceipt.Id, 0, int.MaxValue, It.IsAny<SortParams>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PagedResult<Domain.Core.ReceiptItem>(expectedReceiptItems, expectedReceiptItems.Count, 0, int.MaxValue));

		Mock<IAdjustmentService> mockAdjustmentService = new();
		mockAdjustmentService.Setup(a => a.GetByReceiptIdAsync(expectedReceipt.Id, 0, int.MaxValue, It.IsAny<SortParams>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PagedResult<Domain.Core.Adjustment>(expectedAdjustments, expectedAdjustments.Count, 0, int.MaxValue));

		GetReceiptWithItemsByReceiptIdQueryHandler handler = new(mockReceiptService.Object, mockReceiptItemService.Object, mockAdjustmentService.Object);
		GetReceiptWithItemsByReceiptIdQuery query = new(expectedReceipt.Id);

		// Act
		Domain.Aggregates.ReceiptWithItems? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		result.Receipt.Should().BeSameAs(expectedReceipt);
		result.Items.Should().BeSameAs(expectedReceiptItems);
		result.Adjustments.Should().BeSameAs(expectedAdjustments);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenReceiptDoesNotExist()
	{
		// Arrange
		Guid missingReceiptId = Guid.NewGuid();
		Mock<IReceiptService> mockReceiptService = new();
		mockReceiptService.Setup(r => r.GetByIdAsync(missingReceiptId, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Receipt?)null);

		Mock<IReceiptItemService> mockReceiptItemService = new();
		Mock<IAdjustmentService> mockAdjustmentService = new();

		GetReceiptWithItemsByReceiptIdQueryHandler handler = new(mockReceiptService.Object, mockReceiptItemService.Object, mockAdjustmentService.Object);
		GetReceiptWithItemsByReceiptIdQuery query = new(missingReceiptId);

		// Act
		Domain.Aggregates.ReceiptWithItems? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.Null(result);
	}
}

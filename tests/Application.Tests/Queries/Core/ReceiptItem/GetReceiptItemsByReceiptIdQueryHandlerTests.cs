using SampleData.Domain.Core;
using Moq;
using Application.Queries.Core.ReceiptItem;
using Application.Interfaces.Services;

namespace Application.Tests.Queries.Core.ReceiptItem;

public class GetReceiptItemsByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnReceiptItems_WhenReceiptExistsAndHasItems()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		List<Domain.Core.ReceiptItem> expected = ReceiptItemGenerator.GenerateList(2);

		Mock<IReceiptItemService> mockService = new();
		mockService.Setup(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetReceiptItemsByReceiptIdQueryHandler handler = new(mockService.Object);
		GetReceiptItemsByReceiptIdQuery query = new(receipt.Id!.Value);
		List<Domain.Core.ReceiptItem>? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(expected.Count, result.Count);
		Assert.True(expected.All(result.Contains));
		Assert.True(result.All(expected.Contains));
	}

	[Fact]
	public async Task Handle_ShouldReturnEmptyList_WhenReceiptHasNoItems()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();

		Mock<IReceiptItemService> mockService = new();
		mockService.Setup(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync([]);

		GetReceiptItemsByReceiptIdQueryHandler handler = new(mockService.Object);
		GetReceiptItemsByReceiptIdQuery query = new(receipt.Id!.Value);
		List<Domain.Core.ReceiptItem>? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Empty(result);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenReceiptDoesNotExist()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();

		Mock<IReceiptItemService> mockService = new();
		mockService.Setup(r => r.GetByReceiptIdAsync(receipt.Id!.Value, It.IsAny<CancellationToken>())).ReturnsAsync((List<Domain.Core.ReceiptItem>?)null);

		GetReceiptItemsByReceiptIdQueryHandler handler = new(mockService.Object);
		GetReceiptItemsByReceiptIdQuery query = new(receipt.Id!.Value);

		List<Domain.Core.ReceiptItem>? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
	}
}
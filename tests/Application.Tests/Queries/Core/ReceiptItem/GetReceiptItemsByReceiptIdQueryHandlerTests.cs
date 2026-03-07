using Application.Interfaces.Services;
using Application.Models;
using Application.Queries.Core.ReceiptItem;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.ReceiptItem;

public class GetReceiptItemsByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnReceiptItems_WhenReceiptExistsAndHasItems()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		List<Domain.Core.ReceiptItem> expected = ReceiptItemGenerator.GenerateList(2);

		Mock<IReceiptItemService> mockService = new();
		mockService.Setup(r => r.GetByReceiptIdAsync(receipt.Id, 0, 50, It.IsAny<SortParams>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PagedResult<Domain.Core.ReceiptItem>(expected, expected.Count, 0, 50));

		GetReceiptItemsByReceiptIdQueryHandler handler = new(mockService.Object);
		GetReceiptItemsByReceiptIdQuery query = new(receipt.Id, 0, 50, SortParams.Default);
		PagedResult<Domain.Core.ReceiptItem> result = await handler.Handle(query, CancellationToken.None);

		result.Data.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task Handle_ShouldReturnEmptyList_WhenReceiptHasNoItems()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();

		Mock<IReceiptItemService> mockService = new();
		mockService.Setup(r => r.GetByReceiptIdAsync(receipt.Id, 0, 50, It.IsAny<SortParams>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PagedResult<Domain.Core.ReceiptItem>([], 0, 0, 50));

		GetReceiptItemsByReceiptIdQueryHandler handler = new(mockService.Object);
		GetReceiptItemsByReceiptIdQuery query = new(receipt.Id, 0, 50, SortParams.Default);
		PagedResult<Domain.Core.ReceiptItem> result = await handler.Handle(query, CancellationToken.None);

		result.Data.Should().BeEmpty();
		result.Total.Should().Be(0);
	}

	[Fact]
	public async Task Handle_ShouldReturnEmpty_WhenReceiptDoesNotExist()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();

		Mock<IReceiptItemService> mockService = new();
		mockService.Setup(r => r.GetByReceiptIdAsync(receipt.Id, 0, 50, It.IsAny<SortParams>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PagedResult<Domain.Core.ReceiptItem>([], 0, 0, 50));

		GetReceiptItemsByReceiptIdQueryHandler handler = new(mockService.Object);
		GetReceiptItemsByReceiptIdQuery query = new(receipt.Id, 0, 50, SortParams.Default);

		PagedResult<Domain.Core.ReceiptItem> result = await handler.Handle(query, CancellationToken.None);

		result.Data.Should().BeEmpty();
		result.Total.Should().Be(0);
	}
}

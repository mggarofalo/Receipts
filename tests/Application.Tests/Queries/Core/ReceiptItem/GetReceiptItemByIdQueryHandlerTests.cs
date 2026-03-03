using Application.Interfaces.Services;
using Application.Queries.Core.ReceiptItem;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.ReceiptItem;

public class GetReceiptItemByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnReceiptItem_WhenReceiptItemExists()
	{
		Domain.Core.ReceiptItem expected = ReceiptItemGenerator.Generate();

		Mock<IReceiptItemService> mockService = new();
		mockService.Setup(r => r.GetByIdAsync(expected.Id, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetReceiptItemByIdQueryHandler handler = new(mockService.Object);
		GetReceiptItemByIdQuery query = new(expected.Id);
		Domain.Core.ReceiptItem? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		result.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenAccountDoesNotExist()
	{
		Guid missingId = Guid.NewGuid();
		Mock<IReceiptItemService> mockService = new();
		mockService.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.ReceiptItem?)null);

		GetReceiptItemByIdQueryHandler handler = new(mockService.Object);
		GetReceiptItemByIdQuery query = new(missingId);
		Domain.Core.ReceiptItem? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
	}
}
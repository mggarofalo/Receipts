using Application.Interfaces.Services;
using Application.Queries.Core.ReceiptItem;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.ReceiptItem;

public class GetAllReceiptItemsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllReceiptItems()
	{
		List<Domain.Core.ReceiptItem> expected = ReceiptItemGenerator.GenerateList(2);

		Mock<IReceiptItemService> mockService = new();
		mockService.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetAllReceiptItemsQueryHandler handler = new(mockService.Object);
		GetAllReceiptItemsQuery query = new();

		List<Domain.Core.ReceiptItem> result = await handler.Handle(query, CancellationToken.None);

		result.Should().BeSameAs(expected);
	}
}
using Application.Interfaces.Services;
using Application.Models;
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
		mockService.Setup(r => r.GetAllAsync(0, 50, It.IsAny<CancellationToken>())).ReturnsAsync(new PagedResult<Domain.Core.ReceiptItem>(expected, expected.Count, 0, 50));

		GetAllReceiptItemsQueryHandler handler = new(mockService.Object);
		GetAllReceiptItemsQuery query = new(0, 50);

		PagedResult<Domain.Core.ReceiptItem> result = await handler.Handle(query, CancellationToken.None);

		result.Data.Should().BeSameAs(expected);
	}
}

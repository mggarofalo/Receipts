using Application.Interfaces.Services;
using Application.Queries.Core.ReceiptItem.GetReceiptItemSuggestions;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.Core.ReceiptItem;

public class GetReceiptItemSuggestionsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnSuggestions_WhenServiceReturnsResults()
	{
		List<ReceiptItemSuggestion> expected =
		[
			new()
			{
				ItemCode = "MILK-GAL",
				Description = "Whole Milk",
				Category = "Groceries",
				Subcategory = "Dairy",
				UnitPrice = 3.99m,
				MatchType = "location",
			},
		];

		Mock<IReceiptItemService> mockService = new();
		mockService
			.Setup(s => s.GetSuggestionsAsync("MILK", "Walmart", 10, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		GetReceiptItemSuggestionsQueryHandler handler = new(mockService.Object);
		GetReceiptItemSuggestionsQuery query = new("MILK", "Walmart", 10);
		IEnumerable<ReceiptItemSuggestion> result = await handler.Handle(query, CancellationToken.None);

		result.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public async Task Handle_ShouldReturnEmpty_WhenServiceReturnsNoResults()
	{
		Mock<IReceiptItemService> mockService = new();
		mockService
			.Setup(s => s.GetSuggestionsAsync("XYZ", null, 10, It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);

		GetReceiptItemSuggestionsQueryHandler handler = new(mockService.Object);
		GetReceiptItemSuggestionsQuery query = new("XYZ", null, 10);
		IEnumerable<ReceiptItemSuggestion> result = await handler.Handle(query, CancellationToken.None);

		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_ShouldPassAllParameters_ToService()
	{
		Mock<IReceiptItemService> mockService = new();
		mockService
			.Setup(s => s.GetSuggestionsAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);

		GetReceiptItemSuggestionsQueryHandler handler = new(mockService.Object);
		GetReceiptItemSuggestionsQuery query = new("TEST", "Target", 5);
		await handler.Handle(query, CancellationToken.None);

		mockService.Verify(s => s.GetSuggestionsAsync("TEST", "Target", 5, It.IsAny<CancellationToken>()), Times.Once);
	}
}

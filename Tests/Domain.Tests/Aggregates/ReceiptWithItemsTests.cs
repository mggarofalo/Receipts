using Domain.Aggregates;
using Domain.Core;

namespace Domain.Tests.Aggregates;

public class ReceiptWithItemsTests
{
	[Fact]
	public void ReceiptWithItems_ShouldHaveRequiredProperties()
	{
		// Arrange
		Receipt receipt = new(Guid.NewGuid(), "Test Receipt", DateOnly.FromDateTime(DateTime.Now), new Money(100), "Test Description");
		List<ReceiptItem> items =
		[
			new(Guid.NewGuid(), Guid.NewGuid(), "Test Item 1", "Test Description 1", 100, new Money(100), new Money(100), "Test Category 1", "Test Subcategory 1"),
			new(Guid.NewGuid(), Guid.NewGuid(), "Test Item 2", "Test Description 2", 200, new Money(200), new Money(200), "Test Category 2", "Test Subcategory 2")
		];

		// Act
		ReceiptWithItems receiptWithItems = new()
		{
			Receipt = receipt,
			Items = items
		};

		// Assert
		Assert.NotNull(receiptWithItems.Receipt);
		Assert.NotNull(receiptWithItems.Items);
		Assert.Equal(2, receiptWithItems.Items.Count);
	}
}
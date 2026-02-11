using Domain.Aggregates;
using Domain.Core;
using FluentAssertions;
using SampleData.Domain.Core;

namespace Domain.Tests.Aggregates;

public class ReceiptWithItemsTests
{
	[Fact]
	public void ReceiptWithItems_ShouldHaveRequiredProperties()
	{
		// Arrange
		Receipt receipt = ReceiptGenerator.Generate();
		List<ReceiptItem> items = ReceiptItemGenerator.GenerateList(2);

		ReceiptWithItems receiptWithItems = new()
		{
			Receipt = receipt,
			Items = items
		};

		// Assert
		Assert.NotNull(receiptWithItems.Receipt);
		receiptWithItems.Receipt.Should().BeSameAs(receipt);
		Assert.NotNull(receiptWithItems.Items);
		receiptWithItems.Items.Should().BeSameAs(items);
	}
}

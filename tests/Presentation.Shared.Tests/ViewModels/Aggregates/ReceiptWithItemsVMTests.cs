using FluentAssertions;
using SampleData.ViewModels.Core;
using Shared.ViewModels.Aggregates;
using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.ViewModels.Aggregates;

public class ReceiptWithItemsVMTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesReceiptWithItemsVM()
	{
		// Arrange
		ReceiptVM expectedReceipt = ReceiptVMGenerator.Generate();
		List<ReceiptItemVM> expectedItems = ReceiptItemVMGenerator.GenerateList(2);

		// Act
		ReceiptWithItemsVM receiptWithItemsVM = new()
		{
			Receipt = expectedReceipt,
			Items = expectedItems
		};

		// Assert
		receiptWithItemsVM.Receipt.Should().BeSameAs(expectedReceipt);
		receiptWithItemsVM.Items.Should().BeSameAs(expectedItems);
		Assert.Equal(2, receiptWithItemsVM.Items.Count);
	}
}

using Domain.Aggregates;
using Domain.Core;
using SampleData.Domain.Aggregates;
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
		Assert.NotNull(receiptWithItems.Items);
		Assert.Equal(2, receiptWithItems.Items.Count);
	}

	[Fact]
	public void Equals_SameReceiptWithItems_ReturnsTrue()
	{
		// Arrange
		Receipt receipt = ReceiptGenerator.Generate();
		List<ReceiptItem> items = ReceiptItemGenerator.GenerateList(2);

		ReceiptWithItems receiptWithItems1 = new()
		{
			Receipt = receipt,
			Items = items
		};

		ReceiptWithItems receiptWithItems2 = new()
		{
			Receipt = receipt,
			Items = items
		};

		// Act & Assert
		Assert.True(receiptWithItems1 == receiptWithItems2);
		Assert.False(receiptWithItems1 != receiptWithItems2);
		Assert.True(receiptWithItems1.Equals(receiptWithItems2));
	}

	[Fact]
	public void Equals_DifferentReceiptWithItems_ReturnsFalse()
	{
		// Arrange
		ReceiptWithItems receiptWithItems1 = ReceiptWithItemsGenerator.Generate();
		ReceiptWithItems receiptWithItems2 = ReceiptWithItemsGenerator.Generate();

		// Act & Assert
		Assert.False(receiptWithItems1 == receiptWithItems2);
		Assert.True(receiptWithItems1 != receiptWithItems2);
		Assert.False(receiptWithItems1.Equals(receiptWithItems2));
	}

	[Fact]
	public void Equals_NullReceiptWithItems_ReturnsFalse()
	{
		// Arrange
		ReceiptWithItems receiptWithItems = ReceiptWithItemsGenerator.Generate();

		// Act & Assert
		Assert.False(receiptWithItems.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		ReceiptWithItems receiptWithItems = ReceiptWithItemsGenerator.Generate();

		// Act & Assert
		Assert.False(receiptWithItems.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		ReceiptWithItems receiptWithItems = ReceiptWithItemsGenerator.Generate();

		// Act & Assert
		Assert.False(receiptWithItems.Equals("not a receipt with items"));
	}

	[Fact]
	public void GetHashCode_SameReceiptWithItems_ReturnsSameHashCode()
	{
		// Arrange
		Receipt receipt = ReceiptGenerator.Generate();
		List<ReceiptItem> items = ReceiptItemGenerator.GenerateList(2);

		ReceiptWithItems receiptWithItems1 = new()
		{
			Receipt = receipt,
			Items = items
		};

		ReceiptWithItems receiptWithItems2 = new()
		{
			Receipt = receipt,
			Items = items
		};

		// Act & Assert
		Assert.Equal(receiptWithItems1.GetHashCode(), receiptWithItems2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentReceiptWithItems_ReturnsDifferentHashCode()
	{
		// Arrange
		ReceiptWithItems receiptWithItems1 = ReceiptWithItemsGenerator.Generate();
		ReceiptWithItems receiptWithItems2 = ReceiptWithItemsGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(receiptWithItems1.GetHashCode(), receiptWithItems2.GetHashCode());
	}
}
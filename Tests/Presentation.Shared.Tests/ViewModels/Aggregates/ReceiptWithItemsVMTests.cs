using SampleData.ViewModels.Aggregates;
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
		ReceiptVM receipt = ReceiptVMGenerator.Generate();
		List<ReceiptItemVM> items = ReceiptItemVMGenerator.GenerateList(2);

		// Act
		ReceiptWithItemsVM receiptWithItemsVM = new()
		{
			Receipt = receipt,
			Items = items
		};

		// Assert
		Assert.Equal(receipt, receiptWithItemsVM.Receipt);
		Assert.Equal(items, receiptWithItemsVM.Items);
		Assert.Equal(2, receiptWithItemsVM.Items.Count);
	}

	[Fact]
	public void Equals_SameReceiptWithItemsVM_ReturnsTrue()
	{
		// Arrange
		ReceiptVM receipt = ReceiptVMGenerator.Generate();
		List<ReceiptItemVM> items = ReceiptItemVMGenerator.GenerateList(2);

		ReceiptWithItemsVM receiptWithItemsVM1 = new()
		{
			Receipt = receipt,
			Items = items
		};

		ReceiptWithItemsVM receiptWithItemsVM2 = new()
		{
			Receipt = receipt,
			Items = items
		};

		// Act & Assert
		Assert.Equal(receiptWithItemsVM1, receiptWithItemsVM2);
	}

	[Fact]
	public void Equals_DifferentReceiptWithItemsVM_ReturnsFalse()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM1 = ReceiptWithItemsVMGenerator.Generate();
		ReceiptWithItemsVM receiptWithItemsVM2 = ReceiptWithItemsVMGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(receiptWithItemsVM1, receiptWithItemsVM2);
	}

	[Fact]
	public void Equals_NullReceiptWithItemsVM_ReturnsFalse()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM = ReceiptWithItemsVMGenerator.Generate();

		// Act & Assert
		Assert.False(receiptWithItemsVM.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM = ReceiptWithItemsVMGenerator.Generate();

		// Act & Assert
		Assert.False(receiptWithItemsVM.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM = ReceiptWithItemsVMGenerator.Generate();

		// Act & Assert
		Assert.False(receiptWithItemsVM.Equals("not a receipt with items VM"));
	}

	[Fact]
	public void GetHashCode_SameReceiptWithItemsVM_ReturnsSameHashCode()
	{
		// Arrange
		ReceiptVM receipt = ReceiptVMGenerator.Generate();
		List<ReceiptItemVM> items = ReceiptItemVMGenerator.GenerateList(2);

		ReceiptWithItemsVM receiptWithItemsVM1 = new()
		{
			Receipt = receipt,
			Items = items
		};

		ReceiptWithItemsVM receiptWithItemsVM2 = new()
		{
			Receipt = receipt,
			Items = items
		};

		// Act & Assert
		Assert.Equal(receiptWithItemsVM1.GetHashCode(), receiptWithItemsVM2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentReceiptWithItemsVM_ReturnsDifferentHashCode()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM1 = ReceiptWithItemsVMGenerator.Generate();
		ReceiptWithItemsVM receiptWithItemsVM2 = ReceiptWithItemsVMGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(receiptWithItemsVM1.GetHashCode(), receiptWithItemsVM2.GetHashCode());
	}

	[Fact]
	public void OperatorEquals_SameReceiptWithItemsVM_ReturnsTrue()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM1 = ReceiptWithItemsVMGenerator.Generate();
		ReceiptWithItemsVM receiptWithItemsVM2 = new()
		{
			Receipt = receiptWithItemsVM1.Receipt,
			Items = receiptWithItemsVM1.Items
		};

		// Act
		bool result = receiptWithItemsVM1 == receiptWithItemsVM2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEquals_DifferentReceiptWithItemsVM_ReturnsFalse()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM1 = ReceiptWithItemsVMGenerator.Generate();
		ReceiptWithItemsVM receiptWithItemsVM2 = ReceiptWithItemsVMGenerator.Generate();

		// Act
		bool result = receiptWithItemsVM1 == receiptWithItemsVM2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_SameReceiptWithItemsVM_ReturnsFalse()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM1 = ReceiptWithItemsVMGenerator.Generate();
		ReceiptWithItemsVM receiptWithItemsVM2 = new()
		{
			Receipt = receiptWithItemsVM1.Receipt,
			Items = receiptWithItemsVM1.Items
		};

		// Act
		bool result = receiptWithItemsVM1 != receiptWithItemsVM2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_DifferentReceiptWithItemsVM_ReturnsTrue()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM1 = ReceiptWithItemsVMGenerator.Generate();
		ReceiptWithItemsVM receiptWithItemsVM2 = ReceiptWithItemsVMGenerator.Generate();

		// Act
		bool result = receiptWithItemsVM1 != receiptWithItemsVM2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEquals_NullReceiptWithItemsVM_ReturnsFalse()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM = ReceiptWithItemsVMGenerator.Generate();

		// Act
		bool result = receiptWithItemsVM == null;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_NullReceiptWithItemsVM_ReturnsTrue()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM = ReceiptWithItemsVMGenerator.Generate();

		// Act
		bool result = receiptWithItemsVM != null;

		// Assert
		Assert.True(result);
	}
}
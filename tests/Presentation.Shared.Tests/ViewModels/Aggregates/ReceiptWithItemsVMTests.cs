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
		ReceiptVM expectedReceipt = ReceiptVMGenerator.Generate();
		List<ReceiptItemVM> expectedItems = ReceiptItemVMGenerator.GenerateList(2);

		// Act
		ReceiptWithItemsVM receiptWithItemsVM = new()
		{
			Receipt = expectedReceipt,
			Items = expectedItems
		};

		// Assert
		Assert.Equal(expectedReceipt, receiptWithItemsVM.Receipt);
		Assert.Equal(expectedItems, receiptWithItemsVM.Items);
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
	public void Equals_BothReceiptWithItemsVMHaveNullItems_ReturnsTrue()
	{
		// Arrange
		ReceiptVM receipt = ReceiptVMGenerator.Generate();

		ReceiptWithItemsVM receiptWithItemsVM1 = new()
		{
			Receipt = receipt,
			Items = null
		};

		ReceiptWithItemsVM receiptWithItemsVM2 = new()
		{
			Receipt = receipt,
			Items = null
		};

		// Act & Assert
		Assert.Equal(receiptWithItemsVM1, receiptWithItemsVM2);
	}

	[Fact]
	public void Equals_OneReceiptWithItemsVMHasNullItemsOtherHasEmptyItems_ReturnsTrue()
	{
		// Arrange
		ReceiptVM receipt = ReceiptVMGenerator.Generate();

		ReceiptWithItemsVM receiptWithItemsVM1 = new()
		{
			Receipt = receipt,
			Items = null
		};

		ReceiptWithItemsVM receiptWithItemsVM2 = new()
		{
			Receipt = receipt,
			Items = []
		};

		// Act & Assert
		Assert.Equal(receiptWithItemsVM1, receiptWithItemsVM2);
	}

	[Fact]
	public void Equals_DifferentReceiptWithItemsVM_ReturnsFalse()
	{
		// Arrange
		ReceiptWithItemsVM expected = ReceiptWithItemsVMGenerator.Generate();
		ReceiptWithItemsVM actual = ReceiptWithItemsVMGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(expected, actual);
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

		// Act
		int hashCode1 = receiptWithItemsVM1.GetHashCode();
		int hashCode2 = receiptWithItemsVM2.GetHashCode();

		// Assert
		Assert.Equal(hashCode1, hashCode2);
	}

	[Fact]
	public void GetHashCode_DifferentReceiptWithItemsVM_ReturnsDifferentHashCode()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM1 = ReceiptWithItemsVMGenerator.Generate();
		ReceiptWithItemsVM receiptWithItemsVM2 = ReceiptWithItemsVMGenerator.Generate();

		// Act
		int hashCode1 = receiptWithItemsVM1.GetHashCode();
		int hashCode2 = receiptWithItemsVM2.GetHashCode();

		// Assert
		Assert.NotEqual(hashCode1, hashCode2);
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
		bool actual = receiptWithItemsVM1 == receiptWithItemsVM2;

		// Assert
		Assert.True(actual);
	}

	[Fact]
	public void OperatorEquals_DifferentReceiptWithItemsVM_ReturnsFalse()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM1 = ReceiptWithItemsVMGenerator.Generate();
		ReceiptWithItemsVM receiptWithItemsVM2 = ReceiptWithItemsVMGenerator.Generate();

		// Act
		bool actual = receiptWithItemsVM1 == receiptWithItemsVM2;

		// Assert
		Assert.False(actual);
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
		bool actual = receiptWithItemsVM1 != receiptWithItemsVM2;

		// Assert
		Assert.False(actual);
	}

	[Fact]
	public void OperatorNotEquals_DifferentReceiptWithItemsVM_ReturnsTrue()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM1 = ReceiptWithItemsVMGenerator.Generate();
		ReceiptWithItemsVM receiptWithItemsVM2 = ReceiptWithItemsVMGenerator.Generate();

		// Act
		bool actual = receiptWithItemsVM1 != receiptWithItemsVM2;

		// Assert
		Assert.True(actual);
	}

	[Fact]
	public void OperatorEquals_NullReceiptWithItemsVM_ReturnsFalse()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM = ReceiptWithItemsVMGenerator.Generate();

		// Act
		bool actual = receiptWithItemsVM == null;

		// Assert
		Assert.False(actual);
	}

	[Fact]
	public void OperatorNotEquals_NullReceiptWithItemsVM_ReturnsTrue()
	{
		// Arrange
		ReceiptWithItemsVM receiptWithItemsVM = ReceiptWithItemsVMGenerator.Generate();

		// Act
		bool actual = receiptWithItemsVM != null;

		// Assert
		Assert.True(actual);
	}
}
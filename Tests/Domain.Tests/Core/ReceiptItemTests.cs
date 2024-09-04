using Domain.Core;
using SampleData.Domain.Core;

namespace Domain.Tests.Core;

public class ReceiptItemTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesReceiptItem()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act
		ReceiptItem receiptItem = new(id, Guid.NewGuid(), receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);

		// Assert
		Assert.Equal(id, receiptItem.Id);
		Assert.Equal(receiptItemCode, receiptItem.ReceiptItemCode);
		Assert.Equal(description, receiptItem.Description);
		Assert.Equal(quantity, receiptItem.Quantity);
		Assert.Equal(unitPrice, receiptItem.UnitPrice);
		Assert.Equal(totalAmount, receiptItem.TotalAmount);
		Assert.Equal(category, receiptItem.Category);
		Assert.Equal(subcategory, receiptItem.Subcategory);
	}

	[Fact]
	public void Constructor_NullId_CreatesReceiptItemWithNullId()
	{
		// Arrange
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act
		ReceiptItem receiptItem = new(null, Guid.NewGuid(), receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);

		// Assert
		Assert.Null(receiptItem.Id);
	}

	[Fact]
	public void Constructor_InvalidReceiptId_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ReceiptItem(id, Guid.Empty, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory));
		Assert.StartsWith(ReceiptItem.ReceiptIdCannotBeEmpty, exception.Message);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void Constructor_InvalidReceiptItemCode_ThrowsArgumentException(string invalidReceiptItemCode)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ReceiptItem(id, Guid.NewGuid(), invalidReceiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory));
		Assert.StartsWith(ReceiptItem.ReceiptItemCodeCannotBeEmpty, exception.Message);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void Constructor_InvalidDescription_ThrowsArgumentException(string invalidDescription)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ReceiptItem(id, Guid.NewGuid(), receiptItemCode, invalidDescription, quantity, unitPrice, totalAmount, category, subcategory));
		Assert.StartsWith(ReceiptItem.DescriptionCannotBeEmpty, exception.Message);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void Constructor_InvalidQuantity_ThrowsArgumentException(decimal invalidQuantity)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ReceiptItem(id, Guid.NewGuid(), receiptItemCode, description, invalidQuantity, unitPrice, totalAmount, category, subcategory));
		Assert.StartsWith(ReceiptItem.QuantityMustBePositive, exception.Message);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void Constructor_InvalidCategory_ThrowsArgumentException(string invalidCategory)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string subcategory = "Test Subcategory";

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ReceiptItem(id, Guid.NewGuid(), receiptItemCode, description, quantity, unitPrice, totalAmount, invalidCategory, subcategory));
		Assert.StartsWith(ReceiptItem.CategoryCannotBeEmpty, exception.Message);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void Constructor_InvalidSubcategory_ThrowsArgumentException(string invalidSubcategory)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ReceiptItem(id, Guid.NewGuid(), receiptItemCode, description, quantity, unitPrice, totalAmount, category, invalidSubcategory));
		Assert.StartsWith(ReceiptItem.SubcategoryCannotBeEmpty, exception.Message);
	}

	[Fact]
	public void Equals_SameReceiptItem_ReturnsTrue()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		ReceiptItem receiptItem1 = new(id, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);
		ReceiptItem receiptItem2 = new(id, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);

		// Act & Assert
		Assert.True(receiptItem1 == receiptItem2);
		Assert.False(receiptItem1 != receiptItem2);
		Assert.True(receiptItem1.Equals(receiptItem2));
	}

	[Fact]
	public void Equals_DifferentReceiptItem_ReturnsFalse()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		ReceiptItem receiptItem1 = new(id1, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);
		ReceiptItem receiptItem2 = new(id2, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);

		// Act & Assert
		Assert.False(receiptItem1 == receiptItem2);
		Assert.True(receiptItem1 != receiptItem2);
		Assert.False(receiptItem1.Equals(receiptItem2));
	}

	[Fact]
	public void Equals_DifferentReceiptItemCode_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode1 = "ITEM001";
		string receiptItemCode2 = "ITEM002";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		ReceiptItem receiptItem1 = new(id, receiptId, receiptItemCode1, description, quantity, unitPrice, totalAmount, category, subcategory);
		ReceiptItem receiptItem2 = new(id, receiptId, receiptItemCode2, description, quantity, unitPrice, totalAmount, category, subcategory);

		// Act & Assert
		Assert.False(receiptItem1 == receiptItem2);
		Assert.True(receiptItem1 != receiptItem2);
		Assert.False(receiptItem1.Equals(receiptItem2));
	}

	[Fact]
	public void Equals_DifferentDescription_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description1 = "Test Item";
		string description2 = "Test Item 2";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		ReceiptItem receiptItem1 = new(id, receiptId, receiptItemCode, description1, quantity, unitPrice, totalAmount, category, subcategory);
		ReceiptItem receiptItem2 = new(id, receiptId, receiptItemCode, description2, quantity, unitPrice, totalAmount, category, subcategory);

		// Act & Assert
		Assert.False(receiptItem1 == receiptItem2);
		Assert.True(receiptItem1 != receiptItem2);
		Assert.False(receiptItem1.Equals(receiptItem2));
	}

	[Fact]
	public void Equals_DifferentQuantity_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity1 = 2;
		decimal quantity2 = 3;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		ReceiptItem receiptItem1 = new(id, receiptId, receiptItemCode, description, quantity1, unitPrice, totalAmount, category, subcategory);
		ReceiptItem receiptItem2 = new(id, receiptId, receiptItemCode, description, quantity2, unitPrice, totalAmount, category, subcategory);

		// Act & Assert
		Assert.False(receiptItem1 == receiptItem2);
		Assert.True(receiptItem1 != receiptItem2);
		Assert.False(receiptItem1.Equals(receiptItem2));
	}

	[Fact]
	public void Equals_DifferentUnitPrice_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice1 = new(10.00m);
		Money unitPrice2 = new(15.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		ReceiptItem receiptItem1 = new(id, receiptId, receiptItemCode, description, quantity, unitPrice1, totalAmount, category, subcategory);
		ReceiptItem receiptItem2 = new(id, receiptId, receiptItemCode, description, quantity, unitPrice2, totalAmount, category, subcategory);

		// Act & Assert
		Assert.False(receiptItem1 == receiptItem2);
		Assert.True(receiptItem1 != receiptItem2);
		Assert.False(receiptItem1.Equals(receiptItem2));
	}

	[Fact]
	public void Equals_DifferentTotalAmount_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount1 = new(20.00m);
		Money totalAmount2 = new(30.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		ReceiptItem receiptItem1 = new(id, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount1, category, subcategory);
		ReceiptItem receiptItem2 = new(id, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount2, category, subcategory);

		// Act & Assert
		Assert.False(receiptItem1 == receiptItem2);
		Assert.True(receiptItem1 != receiptItem2);
		Assert.False(receiptItem1.Equals(receiptItem2));
	}

	[Fact]
	public void Equals_DifferentCategory_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category1 = "Test Category";
		string category2 = "Test Category 2";
		string subcategory = "Test Subcategory";

		ReceiptItem receiptItem1 = new(id, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category1, subcategory);
		ReceiptItem receiptItem2 = new(id, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category2, subcategory);

		// Act & Assert
		Assert.False(receiptItem1 == receiptItem2);
		Assert.True(receiptItem1 != receiptItem2);
		Assert.False(receiptItem1.Equals(receiptItem2));
	}

	[Fact]
	public void Equals_DifferentSubcategory_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory1 = "Test Subcategory";
		string subcategory2 = "Test Subcategory 2";

		ReceiptItem receiptItem1 = new(id, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory1);
		ReceiptItem receiptItem2 = new(id, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory2);

		// Act & Assert
		Assert.False(receiptItem1 == receiptItem2);
		Assert.True(receiptItem1 != receiptItem2);
		Assert.False(receiptItem1.Equals(receiptItem2));
	}

	[Fact]
	public void Equals_NullReceiptItem_ReturnsFalse()
	{
		// Arrange
		ReceiptItem receiptItem = ReceiptItemGenerator.Generate();

		// Act & Assert
		Assert.False(receiptItem.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		ReceiptItem receiptItem = ReceiptItemGenerator.Generate();

		// Act & Assert
		Assert.False(receiptItem.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		ReceiptItem receiptItem = ReceiptItemGenerator.Generate();

		// Act & Assert
		Assert.False(receiptItem.Equals("not a receipt item"));
	}

	[Fact]
	public void GetHashCode_SameReceiptItem_ReturnsSameHashCode()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		ReceiptItem receiptItem1 = new(id, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);
		ReceiptItem receiptItem2 = new(id, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);

		// Act & Assert
		Assert.Equal(receiptItem1.GetHashCode(), receiptItem2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentReceiptItem_ReturnsDifferentHashCode()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		ReceiptItem receiptItem1 = new(id1, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);
		ReceiptItem receiptItem2 = new(id2, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);

		// Act & Assert
		Assert.NotEqual(receiptItem1.GetHashCode(), receiptItem2.GetHashCode());
	}
}
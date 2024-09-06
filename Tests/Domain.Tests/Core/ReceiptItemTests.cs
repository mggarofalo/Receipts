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
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act
		ReceiptItem receiptItem = new(id, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);

		// Assert
		Assert.Equal(id, receiptItem.Id);
		Assert.Equal(receiptId, receiptItem.ReceiptId);
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
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act
		ReceiptItem receiptItem = new(null, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);

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
		Guid receiptId = Guid.NewGuid();
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ReceiptItem(id, receiptId, invalidReceiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory));
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
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ReceiptItem(id, receiptId, receiptItemCode, invalidDescription, quantity, unitPrice, totalAmount, category, subcategory));
		Assert.StartsWith(ReceiptItem.DescriptionCannotBeEmpty, exception.Message);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void Constructor_InvalidQuantity_ThrowsArgumentException(decimal invalidQuantity)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ReceiptItem(id, receiptId, receiptItemCode, description, invalidQuantity, unitPrice, totalAmount, category, subcategory));
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
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string subcategory = "Test Subcategory";

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ReceiptItem(id, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, invalidCategory, subcategory));
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
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ReceiptItem(id, receiptId, receiptItemCode, description, quantity, unitPrice, totalAmount, category, invalidSubcategory));
		Assert.StartsWith(ReceiptItem.SubcategoryCannotBeEmpty, exception.Message);
	}

	[Fact]
	public void Equals_SameReceiptItem_ReturnsTrue()
	{
		// Arrange
		ReceiptItem receiptItem1 = ReceiptItemGenerator.Generate();
		ReceiptItem receiptItem2 = new(receiptItem1.Id, receiptItem1.ReceiptId, receiptItem1.ReceiptItemCode, receiptItem1.Description, receiptItem1.Quantity, receiptItem1.UnitPrice, receiptItem1.TotalAmount, receiptItem1.Category, receiptItem1.Subcategory);

		// Act & Assert
		Assert.Equal(receiptItem1, receiptItem2);
	}

	[Fact]
	public void Equals_DifferentReceiptItem_ReturnsFalse()
	{
		// Arrange
		ReceiptItem receiptItem1 = ReceiptItemGenerator.Generate();
		ReceiptItem receiptItem2 = ReceiptItemGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(receiptItem1, receiptItem2);
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
		ReceiptItem receiptItem1 = ReceiptItemGenerator.Generate();
		ReceiptItem receiptItem2 = new(receiptItem1.Id, receiptItem1.ReceiptId, receiptItem1.ReceiptItemCode, receiptItem1.Description, receiptItem1.Quantity, receiptItem1.UnitPrice, receiptItem1.TotalAmount, receiptItem1.Category, receiptItem1.Subcategory);

		// Act & Assert
		Assert.Equal(receiptItem1.GetHashCode(), receiptItem2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentReceiptItem_ReturnsDifferentHashCode()
	{
		// Arrange
		ReceiptItem receiptItem1 = ReceiptItemGenerator.Generate();
		ReceiptItem receiptItem2 = ReceiptItemGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(receiptItem1.GetHashCode(), receiptItem2.GetHashCode());
	}

	[Fact]
	public void OperatorEqual_SameReceiptItem_ReturnsTrue()
	{
		// Arrange
		ReceiptItem receiptItem1 = ReceiptItemGenerator.Generate();
		ReceiptItem receiptItem2 = receiptItem1;

		// Act
		bool result = receiptItem1 == receiptItem2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEqual_DifferentReceiptItem_ReturnsFalse()
	{
		// Arrange
		ReceiptItem receiptItem1 = ReceiptItemGenerator.Generate();
		ReceiptItem receiptItem2 = ReceiptItemGenerator.Generate();

		// Act
		bool result = receiptItem1 == receiptItem2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_SameReceiptItem_ReturnsFalse()
	{
		// Arrange
		ReceiptItem receiptItem1 = ReceiptItemGenerator.Generate();
		ReceiptItem receiptItem2 = receiptItem1;

		// Act
		bool result = receiptItem1 != receiptItem2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_DifferentReceiptItem_ReturnsTrue()
	{
		// Arrange
		ReceiptItem receiptItem1 = ReceiptItemGenerator.Generate();
		ReceiptItem receiptItem2 = ReceiptItemGenerator.Generate();

		// Act
		bool result = receiptItem1 != receiptItem2;

		// Assert
		Assert.True(result);
	}
}
using Common;
using Domain.Core;

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
		ReceiptItem receiptItem = new(id, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);

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
	public void Constructor_EmptyId_CreatesReceiptItemWithEmptyId()
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
		ReceiptItem receiptItem = new(Guid.Empty, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);

		// Assert
		Assert.Equal(Guid.Empty, receiptItem.Id);
	}

	[Fact]
	public void Constructor_NullReceiptItemCode_CreatesReceiptItem()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";

		// Act
		ReceiptItem receiptItem = new(id, null, description, quantity, unitPrice, totalAmount, category, "Test Subcategory");

		// Assert
		Assert.Null(receiptItem.ReceiptItemCode);
	}

	[Fact]
	public void Constructor_NullSubcategory_CreatesReceiptItem()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string description = "Test Item";
		decimal quantity = 2;
		Money unitPrice = new(10.00m);
		Money totalAmount = new(20.00m);
		string category = "Test Category";

		// Act
		ReceiptItem receiptItem = new(id, "ITEM001", description, quantity, unitPrice, totalAmount, category, null);

		// Assert
		Assert.Null(receiptItem.Subcategory);
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
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ReceiptItem(id, receiptItemCode, invalidDescription, quantity, unitPrice, totalAmount, category, subcategory));
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
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ReceiptItem(id, receiptItemCode, description, invalidQuantity, unitPrice, totalAmount, category, subcategory));
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
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ReceiptItem(id, receiptItemCode, description, quantity, unitPrice, totalAmount, invalidCategory, subcategory));
		Assert.StartsWith(ReceiptItem.CategoryCannotBeEmpty, exception.Message);
	}

	[Fact]
	public void Constructor_DefaultPricingMode_IsQuantity()
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
		ReceiptItem receiptItem = new(id, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory);

		// Assert
		Assert.Equal(PricingMode.Quantity, receiptItem.PricingMode);
	}

	[Fact]
	public void Constructor_FlatPricingMode_CreatesReceiptItem()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 1;
		Money unitPrice = new(15.00m);
		Money totalAmount = new(15.00m);
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act
		ReceiptItem receiptItem = new(id, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory, PricingMode.Flat);

		// Assert
		Assert.Equal(PricingMode.Flat, receiptItem.PricingMode);
	}

	[Fact]
	public void Constructor_FlatPricingModeWithQuantityNotOne_ThrowsArgumentException()
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
		ArgumentException exception = Assert.Throws<ArgumentException>(() =>
			new ReceiptItem(id, receiptItemCode, description, quantity, unitPrice, totalAmount, category, subcategory, PricingMode.Flat));
		Assert.StartsWith(ReceiptItem.FlatPricingModeQuantityMustBeOne, exception.Message);
	}
}

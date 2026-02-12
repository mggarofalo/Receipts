using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.ViewModels.Core;

public class ReceiptItemVMTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesReceiptItemVM()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2.0m;
		decimal unitPrice = 5.0m;
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act
		ReceiptItemVM receiptItemVM = new()
		{
			Id = id,
			ReceiptItemCode = receiptItemCode,
			Description = description,
			Quantity = quantity,
			UnitPrice = unitPrice,
			Category = category,
			Subcategory = subcategory
		};

		// Assert
		Assert.Equal(id, receiptItemVM.Id);
		Assert.Equal(receiptItemCode, receiptItemVM.ReceiptItemCode);
		Assert.Equal(description, receiptItemVM.Description);
		Assert.Equal(quantity, receiptItemVM.Quantity);
		Assert.Equal(unitPrice, receiptItemVM.UnitPrice);
		Assert.Equal(category, receiptItemVM.Category);
		Assert.Equal(subcategory, receiptItemVM.Subcategory);
	}

	[Fact]
	public void Constructor_NullId_CreatesReceiptItemVMWithNullId()
	{
		// Arrange
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2.0m;
		decimal unitPrice = 5.0m;
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act
		ReceiptItemVM receiptItemVM = new()
		{
			ReceiptItemCode = receiptItemCode,
			Description = description,
			Quantity = quantity,
			UnitPrice = unitPrice,
			Category = category,
			Subcategory = subcategory
		};

		// Assert
		Assert.Null(receiptItemVM.Id);
	}
}

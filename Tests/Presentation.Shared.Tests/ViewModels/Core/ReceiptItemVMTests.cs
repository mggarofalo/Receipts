using SampleData.ViewModels.Core;
using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.ViewModels.Core;

public class ReceiptItemVMTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesReceiptItemVM()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2.0m;
		decimal unitPrice = 5.0m;
		decimal totalAmount = 10.0m;
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act
		ReceiptItemVM receiptItemVM = new()
		{
			Id = id,
			ReceiptId = receiptId,
			ReceiptItemCode = receiptItemCode,
			Description = description,
			Quantity = quantity,
			UnitPrice = unitPrice,
			TotalAmount = totalAmount,
			Category = category,
			Subcategory = subcategory
		};

		// Assert
		Assert.Equal(id, receiptItemVM.Id);
		Assert.Equal(receiptId, receiptItemVM.ReceiptId);
		Assert.Equal(receiptItemCode, receiptItemVM.ReceiptItemCode);
		Assert.Equal(description, receiptItemVM.Description);
		Assert.Equal(quantity, receiptItemVM.Quantity);
		Assert.Equal(unitPrice, receiptItemVM.UnitPrice);
		Assert.Equal(totalAmount, receiptItemVM.TotalAmount);
		Assert.Equal(category, receiptItemVM.Category);
		Assert.Equal(subcategory, receiptItemVM.Subcategory);
	}

	[Fact]
	public void Constructor_NullId_CreatesReceiptItemVMWithNullId()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2.0m;
		decimal unitPrice = 5.0m;
		decimal totalAmount = 10.0m;
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act
		ReceiptItemVM receiptItemVM = new()
		{
			ReceiptId = receiptId,
			ReceiptItemCode = receiptItemCode,
			Description = description,
			Quantity = quantity,
			UnitPrice = unitPrice,
			TotalAmount = totalAmount,
			Category = category,
			Subcategory = subcategory
		};

		// Assert
		Assert.Null(receiptItemVM.Id);
	}

	[Fact]
	public void Equals_SameReceiptItemVM_ReturnsTrue()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2.0m;
		decimal unitPrice = 5.0m;
		decimal totalAmount = 10.0m;
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		ReceiptItemVM receiptItemVM1 = new()
		{
			Id = id,
			ReceiptId = receiptId,
			ReceiptItemCode = receiptItemCode,
			Description = description,
			Quantity = quantity,
			UnitPrice = unitPrice,
			TotalAmount = totalAmount,
			Category = category,
			Subcategory = subcategory
		};
		ReceiptItemVM receiptItemVM2 = new()
		{
			Id = id,
			ReceiptId = receiptId,
			ReceiptItemCode = receiptItemCode,
			Description = description,
			Quantity = quantity,
			UnitPrice = unitPrice,
			TotalAmount = totalAmount,
			Category = category,
			Subcategory = subcategory
		};

		// Act & Assert
		Assert.True(receiptItemVM1 == receiptItemVM2);
		Assert.False(receiptItemVM1 != receiptItemVM2);
		Assert.True(receiptItemVM1.Equals(receiptItemVM2));
	}

	[Fact]
	public void Equals_DifferentReceiptItemVM_ReturnsFalse()
	{
		// Arrange
		ReceiptItemVM receiptItemVM1 = ReceiptItemVMGenerator.Generate();
		ReceiptItemVM receiptItemVM2 = ReceiptItemVMGenerator.Generate();

		// Act & Assert
		Assert.False(receiptItemVM1 == receiptItemVM2);
		Assert.True(receiptItemVM1 != receiptItemVM2);
		Assert.False(receiptItemVM1.Equals(receiptItemVM2));
	}

	[Fact]
	public void Equals_NullReceiptItemVM_ReturnsFalse()
	{
		// Arrange
		ReceiptItemVM receiptItemVM = ReceiptItemVMGenerator.Generate();

		// Act & Assert
		Assert.False(receiptItemVM.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		ReceiptItemVM receiptItemVM = ReceiptItemVMGenerator.Generate();

		// Act & Assert
		Assert.False(receiptItemVM.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		ReceiptItemVM receiptItemVM = ReceiptItemVMGenerator.Generate();

		// Act & Assert
		Assert.False(receiptItemVM.Equals("not a receiptItemVM"));
	}

	[Fact]
	public void GetHashCode_SameReceiptItemVM_ReturnsSameHashCode()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2.0m;
		decimal unitPrice = 5.0m;
		decimal totalAmount = 10.0m;
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		ReceiptItemVM receiptItemVM1 = new()
		{
			Id = id,
			ReceiptId = receiptId,
			ReceiptItemCode = receiptItemCode,
			Description = description,
			Quantity = quantity,
			UnitPrice = unitPrice,
			TotalAmount = totalAmount,
			Category = category,
			Subcategory = subcategory
		};
		ReceiptItemVM receiptItemVM2 = new()
		{
			Id = id,
			ReceiptId = receiptId,
			ReceiptItemCode = receiptItemCode,
			Description = description,
			Quantity = quantity,
			UnitPrice = unitPrice,
			TotalAmount = totalAmount,
			Category = category,
			Subcategory = subcategory
		};

		// Act & Assert
		Assert.Equal(receiptItemVM1.GetHashCode(), receiptItemVM2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_NullId_ReturnsSameHashCode()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		ReceiptItemVM receiptItemVM1 = new()
		{
			ReceiptId = receiptId,
			ReceiptItemCode = "ITEM001",
			Description = "Test Item",
			Quantity = 2.0m,
			UnitPrice = 5.0m,
			TotalAmount = 10.0m,
			Category = "Test Category",
			Subcategory = "Test Subcategory"
		};
		ReceiptItemVM receiptItemVM2 = new()
		{
			ReceiptId = receiptId,
			ReceiptItemCode = "ITEM001",
			Description = "Test Item",
			Quantity = 2.0m,
			UnitPrice = 5.0m,
			TotalAmount = 10.0m,
			Category = "Test Category",
			Subcategory = "Test Subcategory"
		};

		// Act & Assert
		Assert.Equal(receiptItemVM1.GetHashCode(), receiptItemVM2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentReceiptItemVM_ReturnsDifferentHashCode()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2.0m;
		decimal unitPrice = 5.0m;
		decimal totalAmount = 10.0m;
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		ReceiptItemVM receiptItemVM1 = new()
		{
			Id = id1,
			ReceiptId = receiptId,
			ReceiptItemCode = receiptItemCode,
			Description = description,
			Quantity = quantity,
			UnitPrice = unitPrice,
			TotalAmount = totalAmount,
			Category = category,
			Subcategory = subcategory
		};
		ReceiptItemVM receiptItemVM2 = new()
		{
			Id = id2,
			ReceiptId = receiptId,
			ReceiptItemCode = receiptItemCode,
			Description = description,
			Quantity = quantity,
			UnitPrice = unitPrice,
			TotalAmount = totalAmount,
			Category = category,
			Subcategory = subcategory
		};

		// Act & Assert
		Assert.NotEqual(receiptItemVM1.GetHashCode(), receiptItemVM2.GetHashCode());
	}
}

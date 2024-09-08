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
			ReceiptItemCode = receiptItemCode,
			Description = description,
			Quantity = quantity,
			UnitPrice = unitPrice,
			TotalAmount = totalAmount,
			Category = category,
			Subcategory = subcategory
		};

		// Act & Assert
		Assert.Equal(receiptItemVM1, receiptItemVM2);
	}

	[Fact]
	public void Equals_DifferentReceiptItemVM_ReturnsFalse()
	{
		// Arrange
		ReceiptItemVM receiptItemVM1 = ReceiptItemVMGenerator.Generate();
		ReceiptItemVM receiptItemVM2 = ReceiptItemVMGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(receiptItemVM1, receiptItemVM2);
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
		ReceiptItemVM receiptItemVM1 = new()
		{
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

	[Fact]
	public void OperatorEquals_SameReceiptItemVM_ReturnsTrue()
	{
		// Arrange
		ReceiptItemVM receiptItemVM1 = ReceiptItemVMGenerator.Generate();
		ReceiptItemVM receiptItemVM2 = new()
		{
			Id = receiptItemVM1.Id,
			ReceiptItemCode = receiptItemVM1.ReceiptItemCode,
			Description = receiptItemVM1.Description,
			Quantity = receiptItemVM1.Quantity,
			UnitPrice = receiptItemVM1.UnitPrice,
			TotalAmount = receiptItemVM1.TotalAmount,
			Category = receiptItemVM1.Category,
			Subcategory = receiptItemVM1.Subcategory
		};

		// Act
		bool result = receiptItemVM1 == receiptItemVM2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEquals_DifferentReceiptItemVM_ReturnsFalse()
	{
		// Arrange
		ReceiptItemVM receiptItemVM1 = ReceiptItemVMGenerator.Generate();
		ReceiptItemVM receiptItemVM2 = ReceiptItemVMGenerator.Generate();

		// Act
		bool result = receiptItemVM1 == receiptItemVM2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_SameReceiptItemVM_ReturnsFalse()
	{
		// Arrange
		ReceiptItemVM receiptItemVM1 = ReceiptItemVMGenerator.Generate();
		ReceiptItemVM receiptItemVM2 = new()
		{
			Id = receiptItemVM1.Id,
			ReceiptItemCode = receiptItemVM1.ReceiptItemCode,
			Description = receiptItemVM1.Description,
			Quantity = receiptItemVM1.Quantity,
			UnitPrice = receiptItemVM1.UnitPrice,
			TotalAmount = receiptItemVM1.TotalAmount,
			Category = receiptItemVM1.Category,
			Subcategory = receiptItemVM1.Subcategory
		};

		// Act
		bool result = receiptItemVM1 != receiptItemVM2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_DifferentReceiptItemVM_ReturnsTrue()
	{
		// Arrange
		ReceiptItemVM receiptItemVM1 = ReceiptItemVMGenerator.Generate();
		ReceiptItemVM receiptItemVM2 = ReceiptItemVMGenerator.Generate();

		// Act
		bool result = receiptItemVM1 != receiptItemVM2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEquals_NullReceiptItemVM_ReturnsFalse()
	{
		// Arrange
		ReceiptItemVM receiptItemVM = ReceiptItemVMGenerator.Generate();

		// Act
		bool result = receiptItemVM == null;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEquals_NullReceiptItemVM_ReturnsTrue()
	{
		// Arrange
		ReceiptItemVM receiptItemVM = ReceiptItemVMGenerator.Generate();

		// Act
		bool result = receiptItemVM != null;

		// Assert
		Assert.True(result);
	}
}

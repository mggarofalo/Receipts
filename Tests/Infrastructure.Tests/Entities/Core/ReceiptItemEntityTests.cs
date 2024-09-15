using Common;
using Infrastructure.Entities.Core;
using SampleData.Entities;

namespace Infrastructure.Tests.Entities.Core;

public class ReceiptItemEntityTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesReceiptItemEntity()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		string receiptItemCode = "ITEM001";
		string description = "Test Item";
		decimal quantity = 2;
		decimal unitPrice = 10.00m;
		Currency unitPriceCurrency = Currency.USD;
		decimal totalAmount = 20.00m;
		Currency totalAmountCurrency = Currency.USD;
		string category = "Test Category";
		string subcategory = "Test Subcategory";

		// Act
		ReceiptItemEntity receiptItem = new()
		{
			Id = id,
			ReceiptId = receiptId,
			ReceiptItemCode = receiptItemCode,
			Description = description,
			Quantity = quantity,
			UnitPrice = unitPrice,
			UnitPriceCurrency = unitPriceCurrency,
			TotalAmount = totalAmount,
			TotalAmountCurrency = totalAmountCurrency,
			Category = category,
			Subcategory = subcategory
		};

		// Assert
		Assert.Equal(id, receiptItem.Id);
		Assert.Equal(receiptId, receiptItem.ReceiptId);
		Assert.Equal(receiptItemCode, receiptItem.ReceiptItemCode);
		Assert.Equal(description, receiptItem.Description);
		Assert.Equal(quantity, receiptItem.Quantity);
		Assert.Equal(unitPrice, receiptItem.UnitPrice);
		Assert.Equal(unitPriceCurrency, receiptItem.UnitPriceCurrency);
		Assert.Equal(totalAmount, receiptItem.TotalAmount);
		Assert.Equal(totalAmountCurrency, receiptItem.TotalAmountCurrency);
		Assert.Equal(category, receiptItem.Category);
		Assert.Equal(subcategory, receiptItem.Subcategory);
	}

	[Fact]
	public async Task VirtualReceiptEntity_IsNavigable()
	{
		// Arrange
		ApplicationDbContext context = DbContextHelpers.CreateInMemoryContext();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		ReceiptItemEntity receiptItem = ReceiptItemEntityGenerator.Generate(receipt.Id);

		await context.Receipts.AddAsync(receipt);
		await context.ReceiptItems.AddAsync(receiptItem);
		await context.SaveChangesAsync(CancellationToken.None);

		ReceiptEntity? loadedReceipt = await context.Receipts.FindAsync(receipt.Id);
		ReceiptItemEntity? loadedReceiptItem = await context.ReceiptItems.FindAsync(receiptItem.Id);

		// Act & Assert
		Assert.NotNull(loadedReceipt);
		Assert.NotNull(loadedReceiptItem);
		Assert.Equal(loadedReceipt, loadedReceiptItem.Receipt);
	}

	[Fact]
	public void Equals_SameReceiptItemEntity_ReturnsTrue()
	{
		// Arrange
		ReceiptItemEntity receiptItem1 = ReceiptItemEntityGenerator.Generate();
		ReceiptItemEntity receiptItem2 = new()
		{
			Id = receiptItem1.Id,
			ReceiptId = receiptItem1.ReceiptId,
			ReceiptItemCode = receiptItem1.ReceiptItemCode,
			Description = receiptItem1.Description,
			Quantity = receiptItem1.Quantity,
			UnitPrice = receiptItem1.UnitPrice,
			UnitPriceCurrency = receiptItem1.UnitPriceCurrency,
			TotalAmount = receiptItem1.TotalAmount,
			TotalAmountCurrency = receiptItem1.TotalAmountCurrency,
			Category = receiptItem1.Category,
			Subcategory = receiptItem1.Subcategory
		};

		// Act & Assert
		Assert.Equal(receiptItem1, receiptItem2);
	}

	[Fact]
	public void Equals_DifferentReceiptItemEntity_ReturnsFalse()
	{
		// Arrange
		ReceiptItemEntity receiptItem1 = ReceiptItemEntityGenerator.Generate();
		ReceiptItemEntity receiptItem2 = ReceiptItemEntityGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(receiptItem1, receiptItem2);
	}

	[Fact]
	public void Equals_NullReceiptItemEntity_ReturnsFalse()
	{
		// Arrange
		ReceiptItemEntity receiptItem = ReceiptItemEntityGenerator.Generate();

		// Act & Assert
		Assert.False(receiptItem.Equals(null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		ReceiptItemEntity receiptItem = ReceiptItemEntityGenerator.Generate();

		// Act & Assert
		Assert.False(receiptItem.Equals("not a receipt item entity"));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		ReceiptItemEntity receiptItem = ReceiptItemEntityGenerator.Generate();

		// Act & Assert
		Assert.False(receiptItem.Equals((object?)null));
	}

	[Fact]
	public void GetHashCode_SameReceiptItemEntity_ReturnsSameHashCode()
	{
		// Arrange
		ReceiptItemEntity receiptItem1 = ReceiptItemEntityGenerator.Generate();
		ReceiptItemEntity receiptItem2 = new()
		{
			Id = receiptItem1.Id,
			ReceiptId = receiptItem1.ReceiptId,
			ReceiptItemCode = receiptItem1.ReceiptItemCode,
			Description = receiptItem1.Description,
			Quantity = receiptItem1.Quantity,
			UnitPrice = receiptItem1.UnitPrice,
			UnitPriceCurrency = receiptItem1.UnitPriceCurrency,
			TotalAmount = receiptItem1.TotalAmount,
			TotalAmountCurrency = receiptItem1.TotalAmountCurrency,
			Category = receiptItem1.Category,
			Subcategory = receiptItem1.Subcategory
		};

		// Act & Assert
		Assert.Equal(receiptItem1.GetHashCode(), receiptItem2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentReceiptItemEntity_ReturnsDifferentHashCode()
	{
		// Arrange
		ReceiptItemEntity receiptItem1 = ReceiptItemEntityGenerator.Generate();
		ReceiptItemEntity receiptItem2 = ReceiptItemEntityGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(receiptItem1.GetHashCode(), receiptItem2.GetHashCode());
	}

	[Fact]
	public void OperatorEqual_SameReceiptItemEntity_ReturnsTrue()
	{
		// Arrange
		ReceiptItemEntity receiptItem1 = ReceiptItemEntityGenerator.Generate();
		ReceiptItemEntity receiptItem2 = receiptItem1;

		// Act
		bool result = receiptItem1 == receiptItem2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEqual_DifferentReceiptItemEntity_ReturnsFalse()
	{
		// Arrange
		ReceiptItemEntity receiptItem1 = ReceiptItemEntityGenerator.Generate();
		ReceiptItemEntity receiptItem2 = ReceiptItemEntityGenerator.Generate();

		// Act
		bool result = receiptItem1 == receiptItem2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_SameReceiptItemEntity_ReturnsFalse()
	{
		// Arrange
		ReceiptItemEntity receiptItem1 = ReceiptItemEntityGenerator.Generate();
		ReceiptItemEntity receiptItem2 = receiptItem1;

		// Act
		bool result = receiptItem1 != receiptItem2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_DifferentReceiptItemEntity_ReturnsTrue()
	{
		// Arrange
		ReceiptItemEntity receiptItem1 = ReceiptItemEntityGenerator.Generate();
		ReceiptItemEntity receiptItem2 = ReceiptItemEntityGenerator.Generate();

		// Act
		bool result = receiptItem1 != receiptItem2;

		// Assert
		Assert.True(result);
	}
}
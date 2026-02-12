using Common;
using Infrastructure.Entities.Core;
using Infrastructure.Tests.Repositories;
using Microsoft.EntityFrameworkCore;
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
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		using ApplicationDbContext context = contextFactory.CreateDbContext();
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

		contextFactory.ResetDatabase();
	}
}

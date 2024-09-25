using Common;
using Infrastructure.Entities.Core;

namespace SampleData.Entities;

public static class ReceiptItemEntityGenerator
{
	public static ReceiptItemEntity Generate(Guid? receiptId = null)
	{
		return new ReceiptItemEntity
		{
			Id = Guid.NewGuid(),
			ReceiptId = receiptId ?? Guid.NewGuid(),
			ReceiptItemCode = "ITEMCODE",
			Description = "Test Item",
			Quantity = 1,
			UnitPrice = 5m,
			UnitPriceCurrency = Currency.USD,
			TotalAmount = 5m,
			TotalAmountCurrency = Currency.USD,
			Category = "Test Category",
			Subcategory = "Test Subcategory"
		};
	}

	public static List<ReceiptItemEntity> GenerateList(int count, Guid? receiptId = null)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate(receiptId))
			.ToList();
	}
}

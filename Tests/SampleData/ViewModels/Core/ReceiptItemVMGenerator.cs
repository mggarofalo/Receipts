using Shared.ViewModels.Core;

namespace SampleData.ViewModels.Core;

public static class ReceiptItemVMGenerator
{
	public static ReceiptItemVM Generate(Guid? receiptId = null)
	{
		return new ReceiptItemVM
		{
			Id = Guid.NewGuid(),
			ReceiptId = receiptId ?? Guid.NewGuid(),
			ReceiptItemCode = "ITEMCODE",
			Description = "Test Item",
			Quantity = 1,
			UnitPrice = 5m,
			TotalAmount = 5m,
			Category = "Test Category",
			Subcategory = "Test Subcategory"
		};
	}

	public static List<ReceiptItemVM> GenerateList(int count, Guid? receiptId = null)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate(receiptId))
			.ToList();
	}
}
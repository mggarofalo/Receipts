using Shared.ViewModels.Core;

namespace SampleData.ViewModels.Core;

public static class ReceiptItemVMGenerator
{
	public static ReceiptItemVM Generate()
	{
		return new ReceiptItemVM
		{
			Id = Guid.NewGuid(),
			ReceiptItemCode = "ITEMCODE",
			Description = "Test Item",
			Quantity = 1,
			UnitPrice = 5m,
			Category = "Test Category",
			Subcategory = "Test Subcategory"
		};
	}

	public static List<ReceiptItemVM> GenerateList(int count)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate())
			.ToList();
	}

	public static ReceiptItemVM WithNullId(ReceiptItemVM model)
	{
		model.Id = null;
		return model;
	}

	public static List<ReceiptItemVM> WithNullIds(this List<ReceiptItemVM> models)
	{
		return models.Select(WithNullId).ToList();
	}
}
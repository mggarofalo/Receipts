using Shared.ViewModels.Core;

namespace SampleData.ViewModels.Core;

public static class ReceiptVMGenerator
{
	public static ReceiptVM Generate()
	{
		return new ReceiptVM
		{
			Id = Guid.NewGuid(),
			Description = "Test Receipt",
			Location = "Test Location",
			Date = DateOnly.FromDateTime(DateTime.Now),
			TaxAmount = 10m
		};
	}

	public static List<ReceiptVM> GenerateList(int count)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate())
			.ToList();
	}
}
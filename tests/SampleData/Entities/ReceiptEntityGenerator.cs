using Common;
using Infrastructure.Entities.Core;

namespace SampleData.Entities;

public static class ReceiptEntityGenerator
{
	public static ReceiptEntity Generate()
	{
		return new ReceiptEntity
		{
			Id = Guid.NewGuid(),
			Description = "Test Description",
			Location = "Test Location",
			Date = DateOnly.FromDateTime(DateTime.Now),
			TaxAmount = 10m,
			TaxAmountCurrency = Currency.USD
		};
	}

	public static List<ReceiptEntity> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}

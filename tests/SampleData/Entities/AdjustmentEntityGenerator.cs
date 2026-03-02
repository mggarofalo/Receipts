using Common;
using Infrastructure.Entities.Core;

namespace SampleData.Entities;

public static class AdjustmentEntityGenerator
{
	public static AdjustmentEntity Generate()
	{
		return new AdjustmentEntity
		{
			Id = Guid.NewGuid(),
			ReceiptId = Guid.NewGuid(),
			Type = AdjustmentType.Tip,
			Amount = 5.00m,
			AmountCurrency = Currency.USD,
			Description = null
		};
	}

	public static List<AdjustmentEntity> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}

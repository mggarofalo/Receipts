using Common;
using Domain;
using Domain.Core;

namespace SampleData.Domain.Core;

public static class AdjustmentGenerator
{
	public static Adjustment Generate()
	{
		return new Adjustment(
			Guid.NewGuid(),
			AdjustmentType.Tip,
			new Money(5.00m)
		);
	}

	public static List<Adjustment> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}

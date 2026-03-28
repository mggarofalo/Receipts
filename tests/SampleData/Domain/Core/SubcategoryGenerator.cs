using Domain.Core;

namespace SampleData.Domain.Core;

public static class SubcategoryGenerator
{
	public static Subcategory Generate()
	{
		return new Subcategory(
			Guid.NewGuid(),
			"Test Subcategory",
			Guid.NewGuid(),
			"Test Description",
			true
		);
	}

	public static List<Subcategory> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}

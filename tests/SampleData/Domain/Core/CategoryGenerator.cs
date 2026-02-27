using Domain.Core;

namespace SampleData.Domain.Core;

public static class CategoryGenerator
{
	public static Category Generate()
	{
		return new Category(
			Guid.NewGuid(),
			"Test Category",
			"Test Description"
		);
	}

	public static List<Category> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}

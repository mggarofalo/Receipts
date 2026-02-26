using Infrastructure.Entities.Core;

namespace SampleData.Entities;

public static class CategoryEntityGenerator
{
	public static CategoryEntity Generate()
	{
		return new CategoryEntity
		{
			Id = Guid.NewGuid(),
			Name = "Test Category",
			Description = "Test Description"
		};
	}

	public static List<CategoryEntity> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}

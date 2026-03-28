using Infrastructure.Entities.Core;

namespace SampleData.Entities;

public static class SubcategoryEntityGenerator
{
	public static SubcategoryEntity Generate()
	{
		return new SubcategoryEntity
		{
			Id = Guid.NewGuid(),
			Name = "Test Subcategory",
			CategoryId = Guid.NewGuid(),
			Description = "Test Description",
			IsActive = true
		};
	}

	public static List<SubcategoryEntity> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}

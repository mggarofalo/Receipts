using Domain;
using Domain.Core;

namespace SampleData.Domain.Core;

public static class ItemTemplateGenerator
{
	public static ItemTemplate Generate()
	{
		return new ItemTemplate(
			Guid.NewGuid(),
			"Test Item Template",
			"Test Category",
			"Test Subcategory",
			new Money(9.99m),
			"quantity",
			"ITEM-001",
			"Test Description"
		);
	}

	public static List<ItemTemplate> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}

using Common;
using Infrastructure.Entities.Core;

namespace SampleData.Entities;

public static class ItemTemplateEntityGenerator
{
	public static ItemTemplateEntity Generate()
	{
		return new ItemTemplateEntity
		{
			Id = Guid.NewGuid(),
			Name = "Test Item Template",
			DefaultCategory = "Test Category",
			DefaultSubcategory = "Test Subcategory",
			DefaultUnitPrice = 9.99m,
			DefaultUnitPriceCurrency = Currency.USD,
			DefaultPricingMode = "quantity",
			DefaultItemCode = "ITEM-001",
			Description = "Test Description"
		};
	}

	public static List<ItemTemplateEntity> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}

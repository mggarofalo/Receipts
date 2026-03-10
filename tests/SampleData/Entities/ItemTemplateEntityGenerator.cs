using Infrastructure.Entities.Core;

namespace SampleData.Entities;

public static class ItemTemplateEntityGenerator
{
	public static ItemTemplateEntity Generate()
	{
		return new ItemTemplateEntity
		{
			Id = Guid.NewGuid(),
			Name = $"TestTemplate_{Guid.NewGuid():N}"
		};
	}

	public static List<ItemTemplateEntity> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}

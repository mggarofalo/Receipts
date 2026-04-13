using Infrastructure.Entities.Core;

namespace SampleData.Entities;

public static class CardEntityGenerator
{
	public static CardEntity Generate()
	{
		return new CardEntity
		{
			Id = Guid.NewGuid(),
			CardCode = "TestCardCode",
			Name = "Test Card",
			IsActive = true
		};
	}

	public static List<CardEntity> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}

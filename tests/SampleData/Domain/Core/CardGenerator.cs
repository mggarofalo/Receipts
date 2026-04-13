using Domain.Core;

namespace SampleData.Domain.Core;

public static class CardGenerator
{
	public static Card Generate()
	{
		return new Card(
			Guid.NewGuid(),
			"Test Card",
			"Test Description",
			true
		);
	}

	public static List<Card> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}

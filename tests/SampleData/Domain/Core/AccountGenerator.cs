using Domain.Core;

namespace SampleData.Domain.Core;

public static class AccountGenerator
{
	public static Account Generate()
	{
		return new Account(
			Guid.NewGuid(),
			"Test Account",
			"Test Description",
			true
		);
	}

	public static List<Account> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}

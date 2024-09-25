using Infrastructure.Entities.Core;

namespace SampleData.Entities;

public static class AccountEntityGenerator
{
	public static AccountEntity Generate()
	{
		return new AccountEntity
		{
			Id = Guid.NewGuid(),
			AccountCode = "TestAccountCode",
			Name = "Test Account",
			IsActive = true
		};
	}

	public static List<AccountEntity> GenerateList(int count)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate())
			.ToList();
	}
}

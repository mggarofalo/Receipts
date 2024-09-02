using Shared.ViewModels.Core;

namespace SampleData.ViewModels.Core;

public static class AccountVMGenerator
{
	public static AccountVM Generate()
	{
		return new AccountVM
		{
			Id = Guid.NewGuid(),
			AccountCode = "TestAccountCode",
			Name = "Test Account",
			IsActive = true
		};
	}

	public static List<AccountVM> GenerateList(int count)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate())
			.ToList();
	}
}
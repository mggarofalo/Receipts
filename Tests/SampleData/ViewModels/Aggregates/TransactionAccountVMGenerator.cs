using SampleData.ViewModels.Core;
using Shared.ViewModels.Aggregates;
using Shared.ViewModels.Core;

namespace SampleData.ViewModels.Aggregates;

public static class TransactionAccountVMGenerator
{
	public static TransactionAccountVM Generate()
	{
		TransactionVM transaction = TransactionVMGenerator.Generate();
		AccountVM account = AccountVMGenerator.Generate();

		return new TransactionAccountVM()
		{
			Transaction = transaction,
			Account = account
		};
	}

	public static List<TransactionAccountVM> GenerateList(int count)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate())
			.ToList();
	}
}

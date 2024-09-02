using Domain.Aggregates;
using Domain.Core;
using SampleData.Domain.Core;

namespace SampleData.Domain.Aggregates;

public static class TransactionAccountGenerator
{
	public static TransactionAccount Generate()
	{
		Transaction transaction = TransactionGenerator.Generate();
		Account account = AccountGenerator.Generate();

		return new TransactionAccount()
		{
			Transaction = transaction,
			Account = account
		};
	}

	public static List<TransactionAccount> GenerateList(int count)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate())
			.ToList();
	}
}

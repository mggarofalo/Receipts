using Domain;
using Domain.Core;

namespace SampleData.Domain.Core;

public static class TransactionGenerator
{
	public static Transaction Generate()
	{
		return new Transaction(
			Guid.NewGuid(),
			new Money(100),
			DateOnly.FromDateTime(DateTime.Now)
		);
	}

	public static List<Transaction> GenerateList(int count)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate())
			.ToList();
	}
}

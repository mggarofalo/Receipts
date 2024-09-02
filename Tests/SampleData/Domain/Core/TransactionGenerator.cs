using Domain;
using Domain.Core;

namespace SampleData.Domain.Core;

public static class TransactionGenerator
{
	public static Transaction Generate(Guid? receiptId = null, Guid? accountId = null)
	{
		return new Transaction(
			Guid.NewGuid(),
			receiptId ?? Guid.NewGuid(),
			accountId ?? Guid.NewGuid(),
			new Money(100),
			DateOnly.FromDateTime(DateTime.Now)
		);
	}

	public static List<Transaction> GenerateList(int count, Guid? receiptId = null, Guid? accountId = null)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate(receiptId, accountId))
			.ToList();
	}
}

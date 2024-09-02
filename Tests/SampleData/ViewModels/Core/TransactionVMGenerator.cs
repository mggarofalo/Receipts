using Shared.ViewModels.Core;

namespace SampleData.ViewModels.Core;

public static class TransactionVMGenerator
{
	public static TransactionVM Generate(Guid? receiptId = null, Guid? accountId = null)
	{
		return new TransactionVM
		{
			Id = Guid.NewGuid(),
			ReceiptId = receiptId ?? Guid.NewGuid(),
			AccountId = accountId ?? Guid.NewGuid(),
			Amount = 100m,
			Date = DateOnly.FromDateTime(DateTime.Now)
		};
	}

	public static List<TransactionVM> GenerateList(int count, Guid? receiptId = null, Guid? accountId = null)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate(receiptId, accountId))
			.ToList();
	}
}
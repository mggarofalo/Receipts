using Shared.ViewModels.Core;

namespace SampleData.ViewModels.Core;

public static class TransactionVMGenerator
{
	public static TransactionVM Generate()
	{
		return new TransactionVM
		{
			Id = Guid.NewGuid(),
			Amount = 100m,
			Date = DateOnly.FromDateTime(DateTime.Now)
		};
	}

	public static List<TransactionVM> GenerateList(int count)
	{
		return Enumerable.Range(0, count)
			.Select(_ => Generate())
			.ToList();
	}

	public static TransactionVM WithNullId(TransactionVM model)
	{
		model.Id = null;
		return model;
	}

	public static List<TransactionVM> WithNullIds(this List<TransactionVM> models)
	{
		return models.Select(WithNullId).ToList();
	}
}
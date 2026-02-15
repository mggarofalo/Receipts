using Domain;
using Domain.Core;

namespace SampleData.Domain.Core;

public static class ReceiptGenerator
{
	public static Receipt Generate()
	{
		return new Receipt(
			Guid.NewGuid(),
			"Test Location",
			DateOnly.FromDateTime(DateTime.Now),
			new Money(10),
			"Test Description"
		);
	}

	public static List<Receipt> GenerateList(int count)
	{
		return [.. Enumerable.Range(0, count).Select(_ => Generate())];
	}
}
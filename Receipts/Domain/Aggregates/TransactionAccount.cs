using Domain.Core;

namespace Domain.Aggregates;

public class TransactionAccount
{
	public required Transaction Transaction { get; set; }
	public required Account Account { get; set; }
}
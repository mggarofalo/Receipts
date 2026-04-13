using Domain.Core;

namespace Domain.Aggregates;

// Property name `Account` preserves the semantic Transaction→Account relationship.
// The concrete type is temporarily `Card` until Stage 2 introduces the logical Account layer.
public class TransactionAccount
{
	public required Transaction Transaction { get; set; }
	public required Card Account { get; set; }
}

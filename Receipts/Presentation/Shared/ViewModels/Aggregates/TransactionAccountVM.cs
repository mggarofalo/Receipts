using Shared.ViewModels.Core;

namespace Shared.ViewModels.Aggregates;

public class TransactionAccountVM
{
	public required TransactionVM Transaction { get; set; }
	public required AccountVM Account { get; set; }
}
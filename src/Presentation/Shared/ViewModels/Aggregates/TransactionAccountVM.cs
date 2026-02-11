using Shared.ViewModels.Core;

namespace Shared.ViewModels.Aggregates;

public class TransactionAccountVM
{
	public TransactionVM? Transaction { get; set; }
	public AccountVM? Account { get; set; }
}
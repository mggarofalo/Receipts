using Common;
using Domain;
using Domain.Core;
using Riok.Mapperly.Abstractions;
using Shared.ViewModels.Core;

namespace API.Mapping.Core;

[Mapper]
public partial class TransactionMapper
{
	[MapProperty(nameof(Transaction.Amount.Amount), nameof(TransactionVM.Amount))]
	public partial TransactionVM ToViewModel(Transaction source);

	private Money MapAmount(decimal? amount) => new(amount ?? 0, Currency.USD);
	private Guid MapId(Guid? id) => id ?? Guid.Empty;

	public partial Transaction ToDomain(TransactionVM source);
}

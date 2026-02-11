using Common;
using Domain;
using Domain.Core;
using Riok.Mapperly.Abstractions;
using Shared.ViewModels.Core;

namespace API.Mapping.Core;

[Mapper]
public partial class ReceiptMapper
{
	[MapProperty(nameof(Receipt.TaxAmount.Amount), nameof(ReceiptVM.TaxAmount))]
	public partial ReceiptVM ToViewModel(Receipt source);

	private Money MapTaxAmount(decimal? taxAmount) => new(taxAmount ?? 0, Currency.USD);

	public partial Receipt ToDomain(ReceiptVM source);
}

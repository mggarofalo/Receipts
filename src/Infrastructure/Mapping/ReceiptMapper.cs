using Common;
using Domain;
using Domain.Core;
using Infrastructure.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Infrastructure.Mapping;

[Mapper]
public partial class ReceiptMapper
{
	[MapProperty(nameof(Receipt.TaxAmount.Amount), nameof(ReceiptEntity.TaxAmount))]
	[MapProperty(nameof(Receipt.TaxAmount.Currency), nameof(ReceiptEntity.TaxAmountCurrency))]
	public partial ReceiptEntity ToEntity(Receipt source);

	private Money MapTaxAmount(decimal amount, Currency currency) => new(amount, currency);

	public partial Receipt ToDomain(ReceiptEntity source);
}

using Common;
using Domain;
using Domain.Core;
using Infrastructure.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Infrastructure.Mapping;

[Mapper]
public partial class AdjustmentMapper
{
	[MapProperty(nameof(Adjustment.Amount.Amount), nameof(AdjustmentEntity.Amount))]
	[MapProperty(nameof(Adjustment.Amount.Currency), nameof(AdjustmentEntity.AmountCurrency))]
	[MapperIgnoreSource(nameof(Adjustment.ReceiptId))]
	[MapperIgnoreTarget(nameof(AdjustmentEntity.Receipt))]
	[MapperIgnoreTarget(nameof(AdjustmentEntity.ReceiptId))]
	[MapperIgnoreTarget(nameof(AdjustmentEntity.DeletedAt))]
	[MapperIgnoreTarget(nameof(AdjustmentEntity.DeletedByUserId))]
	[MapperIgnoreTarget(nameof(AdjustmentEntity.DeletedByApiKeyId))]
	public partial AdjustmentEntity ToEntity(Adjustment source);

	private Money MapAmount(decimal amount, Currency currency) => new(amount, currency);

	[MapperIgnoreSource(nameof(AdjustmentEntity.Receipt))]
	[MapperIgnoreSource(nameof(AdjustmentEntity.AmountCurrency))]
	[MapperIgnoreSource(nameof(AdjustmentEntity.DeletedAt))]
	[MapperIgnoreSource(nameof(AdjustmentEntity.DeletedByUserId))]
	[MapperIgnoreSource(nameof(AdjustmentEntity.DeletedByApiKeyId))]
	public partial Adjustment ToDomain(AdjustmentEntity source);
}

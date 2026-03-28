using Common;
using Domain;
using Domain.Core;
using Infrastructure.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Infrastructure.Mapping;

[Mapper]
public partial class ReceiptItemMapper
{
	[MapProperty(nameof(ReceiptItem.UnitPrice.Amount), nameof(ReceiptItemEntity.UnitPrice))]
	[MapProperty(nameof(ReceiptItem.UnitPrice.Currency), nameof(ReceiptItemEntity.UnitPriceCurrency))]
	[MapProperty(nameof(ReceiptItem.TotalAmount.Amount), nameof(ReceiptItemEntity.TotalAmount))]
	[MapProperty(nameof(ReceiptItem.TotalAmount.Currency), nameof(ReceiptItemEntity.TotalAmountCurrency))]
	[MapperIgnoreSource(nameof(ReceiptItem.ReceiptId))]
	[MapperIgnoreTarget(nameof(ReceiptItemEntity.Receipt))]
	[MapperIgnoreTarget(nameof(ReceiptItemEntity.ReceiptId))]
	[MapperIgnoreTarget(nameof(ReceiptItemEntity.DeletedAt))]
	[MapperIgnoreTarget(nameof(ReceiptItemEntity.DeletedByUserId))]
	[MapperIgnoreTarget(nameof(ReceiptItemEntity.DeletedByApiKeyId))]
	[MapperIgnoreTarget(nameof(ReceiptItemEntity.CascadeDeletedByParentId))]
	public partial ReceiptItemEntity ToEntity(ReceiptItem source);

	private Money MapUnitPrice(decimal amount, Currency currency) => new(amount, currency);
	private Money MapTotalAmount(decimal amount, Currency currency) => new(amount, currency);

	[MapperIgnoreSource(nameof(ReceiptItemEntity.Receipt))]
	[MapperIgnoreSource(nameof(ReceiptItemEntity.UnitPriceCurrency))]
	[MapperIgnoreSource(nameof(ReceiptItemEntity.TotalAmountCurrency))]
	[MapperIgnoreSource(nameof(ReceiptItemEntity.DeletedAt))]
	[MapperIgnoreSource(nameof(ReceiptItemEntity.DeletedByUserId))]
	[MapperIgnoreSource(nameof(ReceiptItemEntity.DeletedByApiKeyId))]
	[MapperIgnoreSource(nameof(ReceiptItemEntity.CascadeDeletedByParentId))]
	public partial ReceiptItem ToDomain(ReceiptItemEntity source);
}

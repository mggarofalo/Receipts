using Common;
using Domain;
using Domain.Core;
using Infrastructure.Entities.Core;
using Riok.Mapperly.Abstractions;

namespace Infrastructure.Mapping;

[Mapper]
public partial class TransactionMapper
{
	[MapProperty(nameof(Transaction.Amount.Amount), nameof(TransactionEntity.Amount))]
	[MapProperty(nameof(Transaction.Amount.Currency), nameof(TransactionEntity.AmountCurrency))]
	[MapperIgnoreTarget(nameof(TransactionEntity.Receipt))]
	[MapperIgnoreTarget(nameof(TransactionEntity.ReceiptId))]
	[MapperIgnoreTarget(nameof(TransactionEntity.Account))]
	[MapperIgnoreTarget(nameof(TransactionEntity.AccountId))]
	[MapperIgnoreTarget(nameof(TransactionEntity.DeletedAt))]
	[MapperIgnoreTarget(nameof(TransactionEntity.DeletedByUserId))]
	[MapperIgnoreTarget(nameof(TransactionEntity.DeletedByApiKeyId))]
	public partial TransactionEntity ToEntity(Transaction source);

	private Money MapAmount(decimal amount, Currency currency) => new(amount, currency);

	[MapperIgnoreSource(nameof(TransactionEntity.Receipt))]
	[MapperIgnoreSource(nameof(TransactionEntity.ReceiptId))]
	[MapperIgnoreSource(nameof(TransactionEntity.Account))]
	[MapperIgnoreSource(nameof(TransactionEntity.AccountId))]
	[MapperIgnoreSource(nameof(TransactionEntity.AmountCurrency))]
	[MapperIgnoreSource(nameof(TransactionEntity.DeletedAt))]
	[MapperIgnoreSource(nameof(TransactionEntity.DeletedByUserId))]
	[MapperIgnoreSource(nameof(TransactionEntity.DeletedByApiKeyId))]
	public partial Transaction ToDomain(TransactionEntity source);
}

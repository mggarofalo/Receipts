using AutoMapper;
using Domain;
using Domain.Core;
using Infrastructure.Entities.Core;

namespace Infrastructure.Mapping;

public class TransactionMappingProfile : Profile
{
	public TransactionMappingProfile()
	{
		CreateMap<Transaction, TransactionEntity>()
			.ConstructUsing((src, context) =>
			{
				return new TransactionEntity
				{
					Id = src.Id ?? Guid.Empty,
					ReceiptId = context.GetValueFromContext(nameof(TransactionEntity.ReceiptId)),
					AccountId = context.GetValueFromContext(nameof(TransactionEntity.AccountId)),
					Amount = src.Amount.Amount,
					AmountCurrency = src.Amount.Currency
				};
			});

		CreateMap<TransactionEntity, Transaction>()
			.ConstructUsing((src, context) =>
			{
				return new Transaction(
					src.Id == Guid.Empty ? null : src.Id,
					new Money(src.Amount, src.AmountCurrency),
					src.Date
				);
			});
	}
}
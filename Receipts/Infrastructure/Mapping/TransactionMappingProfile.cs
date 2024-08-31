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
			.ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount))
			.ForMember(dest => dest.AmountCurrency, opt => opt.MapFrom(src => src.Amount.Currency));

		CreateMap<TransactionEntity, Transaction>()
			.ConstructUsing(src => new(
				src.Id,
				src.ReceiptId,
				src.AccountId,
				new Money(src.Amount, src.AmountCurrency),
				src.Date
			));
	}
}
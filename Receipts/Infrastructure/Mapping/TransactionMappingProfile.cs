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
			.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? Guid.Empty))
			.ForMember(dest => dest.Receipt, opt => opt.Ignore())
			.ForMember(dest => dest.ReceiptId, opt => opt.MapFrom((src, dest, _, context) => context.GetValueFromContext(nameof(TransactionEntity.ReceiptId))))
			.ForMember(dest => dest.Account, opt => opt.Ignore())
			.ForMember(dest => dest.AccountId, opt => opt.MapFrom((src, dest, _, context) => context.GetValueFromContext(nameof(TransactionEntity.AccountId))))
			.ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount))
			.ForMember(dest => dest.AmountCurrency, opt => opt.MapFrom(src => src.Amount.Currency));

		CreateMap<TransactionEntity, Transaction>()
			.ForMember(dest => dest.Amount, opt => opt.MapFrom(src => new Money(src.Amount, src.AmountCurrency)));
	}
}
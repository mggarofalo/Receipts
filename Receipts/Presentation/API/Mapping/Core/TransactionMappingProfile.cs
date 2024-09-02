using AutoMapper;
using Common;
using Domain;
using Domain.Core;
using Shared.ViewModels.Core;

namespace API.Mapping.Core;

public class TransactionMappingProfile : Profile
{
	public TransactionMappingProfile()
	{
		CreateMap<Transaction, TransactionVM>()
			.ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount));

		CreateMap<TransactionVM, Transaction>()
			.ConstructUsing(src => new(
				src.Id,
				src.ReceiptId,
				src.AccountId,
				new Money(src.Amount, Currency.USD),
				src.Date
			));
	}
}
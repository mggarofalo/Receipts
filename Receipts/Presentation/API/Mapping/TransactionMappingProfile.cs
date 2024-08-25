using AutoMapper;
using Domain;
using Domain.Core;
using Shared.ViewModels;

namespace API.Mapping;

public class TransactionMappingProfile : Profile
{
	public TransactionMappingProfile()
	{
		CreateMap<Transaction, TransactionVM>()
			.ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount));

		CreateMap<TransactionVM, Transaction>()
			.ConstructUsing(src => new(null, src.ReceiptId, src.Account.Id!.Value, new Money(src.Amount, "USD"), src.Date));
	}
}
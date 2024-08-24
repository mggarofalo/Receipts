using AutoMapper;
using Domain;
using Infrastructure.Entities;

namespace Application.Mapping;

public class MappingProfile : Profile
{
	public MappingProfile()
	{
		CreateMap<Account, AccountEntity>().ReverseMap();
		CreateMap<Receipt, ReceiptEntity>().ReverseMap();
		CreateMap<Transaction, TransactionEntity>().ReverseMap();
		CreateMap<TransactionItem, TransactionItemEntity>().ReverseMap();
	}
}

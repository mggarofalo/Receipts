using AutoMapper;
using Domain;
using Domain.Core;
using Infrastructure.Entities.Core;

namespace Infrastructure.Mapping;

public class MappingProfile : Profile
{
	public MappingProfile()
	{
		CreateMap<Account, AccountEntity>()
			.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
			.ForMember(dest => dest.AccountCode, opt => opt.MapFrom(src => src.AccountCode))
			.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
			.ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

		CreateMap<AccountEntity, Account>()
			.ConstructUsing(src => new(
				null,
				src.AccountCode,
				src.Name,
				src.IsActive
			))
			.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

		CreateMap<Receipt, ReceiptEntity>()
			.ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount.Amount));

		CreateMap<ReceiptEntity, Receipt>()
			.ConstructUsing(src => new(
				null,
				src.Location,
				src.Date,
				new Money(src.TaxAmount, "USD"),
				src.Description
			))
			.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

		CreateMap<Transaction, TransactionEntity>()
			.ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount));

		CreateMap<TransactionEntity, Transaction>()
			.ConstructUsing(src => new(
				null,
				src.ReceiptId,
				src.AccountId,
				new Money(src.Amount, "USD"),
				src.Date
			))
			.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

		CreateMap<ReceiptItem, ReceiptItemEntity>()
			.ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice.Amount))
			.ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount));

		CreateMap<ReceiptItemEntity, ReceiptItem>()
			.ConstructUsing(src => new(
				null,
				src.ReceiptItemCode,
				src.Description,
				src.Quantity,
				new Money(src.UnitPrice, "USD"),
				src.Category,
				src.Subcategory
			))
			.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
	}
}
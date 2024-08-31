using AutoMapper;
using Common;
using Domain;
using Domain.Core;
using Shared.ViewModels;

namespace API.Mapping;

public class ReceiptMappingProfile : Profile
{
	public ReceiptMappingProfile()
	{
		CreateMap<Receipt, ReceiptVM>()
			.ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount.Amount));

		CreateMap<ReceiptVM, Receipt>()
			.ConstructUsing(src => new(
				null,
				src.Location,
				src.Date,
				new Money(src.TaxAmount, Currency.USD),
				src.Description
			));
	}
}
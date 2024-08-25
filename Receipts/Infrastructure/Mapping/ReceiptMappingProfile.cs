using AutoMapper;
using Domain;
using Domain.Core;
using Infrastructure.Entities.Core;

namespace Infrastructure.Mapping;

public class ReceiptMappingProfile : Profile
{
	public ReceiptMappingProfile()
	{
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
	}
}
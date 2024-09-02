using AutoMapper;
using Domain.Aggregates;
using Shared.ViewModels.Aggregates;

namespace API.Mapping.Aggregates;

public class ReceiptWithItemsMappingProfile : Profile
{
	public ReceiptWithItemsMappingProfile()
	{
		CreateMap<ReceiptWithItems, ReceiptWithItemsVM>()
			.ForMember(dest => dest.Receipt, opt => opt.MapFrom(src => src.Receipt))
			.ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
	}
}
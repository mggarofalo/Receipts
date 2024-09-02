using AutoMapper;
using Domain.Aggregates;
using Shared.ViewModels.Aggregates;

namespace API.Mapping.Aggregates;

public class ReceiptWithItemsMappingProfile : Profile
{
	public ReceiptWithItemsMappingProfile()
	{
		CreateMap<ReceiptWithItems, ReceiptWithItemsVM>().ReverseMap();
	}
}
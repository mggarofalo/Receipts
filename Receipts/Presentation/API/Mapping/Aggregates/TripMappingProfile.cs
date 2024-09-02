using AutoMapper;
using Domain.Aggregates;
using Shared.ViewModels.Aggregates;

namespace API.Mapping.Aggregates;

public class TripMappingProfile : Profile
{
	public TripMappingProfile()
	{
		CreateMap<Trip, TripVM>().ReverseMap();
	}
}
using API.Mapping.Aggregates;
using AutoMapper;
using Domain.Aggregates;
using Shared.ViewModels.Aggregates;
using SampleData.Domain.Aggregates;
using SampleData.ViewModels.Aggregates;
using API.Mapping.Core;

namespace Presentation.API.Tests.Mapping.Aggregates;

public class TripMappingProfileTests
{
	private readonly IMapper _mapper;

	public TripMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<TripMappingProfile>();
			cfg.AddProfile<ReceiptWithItemsMappingProfile>();
			cfg.AddProfile<ReceiptMappingProfile>();
			cfg.AddProfile<ReceiptItemMappingProfile>();
			cfg.AddProfile<TransactionAccountMappingProfile>();
			cfg.AddProfile<TransactionMappingProfile>();
			cfg.AddProfile<AccountMappingProfile>();
		});

		_mapper = configuration.CreateMapper();
		_mapper.ConfigurationProvider.AssertConfigurationIsValid();
	}

	[Fact]
	public void ShouldMapTripToTripVM()
	{
		// Arrange
		Trip trip = TripGenerator.Generate();

		// Act
		TripVM tripVM = _mapper.Map<TripVM>(trip);
		Trip reverseMapped = _mapper.Map<Trip>(tripVM);

		// Assert
		Assert.Equal(trip, reverseMapped);
	}

	[Fact]
	public void ShouldMapTripVMToTrip()
	{
		// Arrange
		TripVM tripVM = TripVMGenerator.Generate();

		// Act
		Trip trip = _mapper.Map<Trip>(tripVM);
		TripVM reverseMapped = _mapper.Map<TripVM>(trip);

		// Assert
		Assert.Equal(tripVM, reverseMapped);
	}
}